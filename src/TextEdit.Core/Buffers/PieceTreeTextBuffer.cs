using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace TextEdit.Core.Buffers;

/// <summary>
/// Piece-table based text buffer optimized for large documents and cheap snapshots.
/// </summary>
public sealed class PieceTreeTextBuffer
{
    internal const int DefaultCoalesceThresholdMilliseconds = 750;

    private const int OriginalBufferId = 0;

    private readonly string _originalBuffer;
    private readonly List<string> _addBuffers = new();
    private readonly List<Piece> _pieces = new();
    private readonly Stack<TextBufferOperation> _undoStack = new();
    private readonly Stack<TextBufferOperation> _redoStack = new();

    private int _length;
    private int _version;
    private bool _journalPaused;
    private DateTime _lastOperationUtc = DateTime.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PieceTreeTextBuffer"/> class.
    /// </summary>
    /// <param name="initialText">Initial content. May be <see langword="null"/> for an empty buffer.</param>
    public PieceTreeTextBuffer(string? initialText = null)
    {
        _originalBuffer = initialText ?? string.Empty;
        if (_originalBuffer.Length > 0)
        {
            _pieces.Add(new Piece(BufferKind.Original, OriginalBufferId, 0, _originalBuffer.Length));
            _length = _originalBuffer.Length;
        }
    }

    /// <summary>
    /// Gets the current text length.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the current version, incremented after each mutation.
    /// </summary>
    public int Version => _version;

    /// <summary>
    /// Gets a value indicating whether undo is available.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Gets a value indicating whether redo is available.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Gets the entire text content as a <see cref="string"/>.
    /// </summary>
    public override string ToString()
    {
        if (_pieces.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(_length);
        foreach (var piece in _pieces)
        {
            builder.Append(GetSpan(piece));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Inserts text at the specified position.
    /// </summary>
    public void Insert(int position, string text) => InsertInternal(position, text, record: true);

    /// <summary>
    /// Deletes a span starting at <paramref name="position"/> for <paramref name="length"/> characters.
    /// </summary>
    public void Delete(int position, int length) => DeleteInternal(position, length, record: true);

    /// <summary>
    /// Creates an immutable snapshot of the buffer.
    /// </summary>
    public PieceTreeSnapshot CreateSnapshot()
    {
        return new PieceTreeSnapshot(
            _version,
            _length,
            _originalBuffer,
            ImmutableArray.CreateRange(_pieces),
            _addBuffers.ToImmutableArray());
    }

    /// <summary>
    /// Enumerates the underlying chunks that compose the current document.
    /// </summary>
    public IEnumerable<ReadOnlyMemory<char>> GetChunks()
    {
        foreach (var piece in _pieces)
        {
            yield return GetMemory(piece);
        }
    }

    /// <summary>
    /// Retrieves a substring from the buffer.
    /// </summary>
    public string GetText(int position, int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        if (position < 0 || position + length > _length)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        var builder = new StringBuilder(length);
        var end = position + length;
        var current = 0;

        foreach (var piece in _pieces)
        {
            var next = current + piece.Length;
            if (next <= position)
            {
                current = next;
                continue;
            }

            if (current >= end)
            {
                break;
            }

            var sliceStart = Math.Max(position - current, 0);
            var sliceLength = Math.Min(next, end) - (current + sliceStart);
            if (sliceLength > 0)
            {
                builder.Append(GetSpan(piece).Slice(sliceStart, sliceLength));
            }

            current = next;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Reverts the last recorded change.
    /// </summary>
    public bool Undo()
    {
        if (!CanUndo)
        {
            return false;
        }

        var op = _undoStack.Pop();
        var previousPause = _journalPaused;
        _journalPaused = true;
        try
        {
            switch (op.Kind)
            {
                case ChangeKind.Insert:
                    DeleteInternal(op.Position, op.Text.Length, record: false);
                    break;
                case ChangeKind.Delete:
                    InsertInternal(op.Position, op.Text, record: false);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported change kind: {op.Kind}");
            }
        }
        finally
        {
            _journalPaused = previousPause;
        }

        _redoStack.Push(op);
        _lastOperationUtc = DateTime.MinValue;
        return true;
    }

    /// <summary>
    /// Reapplies the last undone change.
    /// </summary>
    public bool Redo()
    {
        if (!CanRedo)
        {
            return false;
        }

        var op = _redoStack.Pop();
        var previousPause = _journalPaused;
        _journalPaused = true;
        try
        {
            switch (op.Kind)
            {
                case ChangeKind.Insert:
                    InsertInternal(op.Position, op.Text, record: false);
                    break;
                case ChangeKind.Delete:
                    DeleteInternal(op.Position, op.Text.Length, record: false);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported change kind: {op.Kind}");
            }
        }
        finally
        {
            _journalPaused = previousPause;
        }

        _undoStack.Push(op);
        _lastOperationUtc = DateTime.MinValue;
        return true;
    }

    private void InsertInternal(int position, string text, bool record)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
        {
            return;
        }

        ValidatePosition(position, allowEnd: true);

        var insertIndex = SplitAt(position);

        var bufferId = AddToAddBuffer(text);
        var newPiece = new Piece(BufferKind.Add, bufferId, 0, text.Length);

        _pieces.Insert(insertIndex, newPiece);
        _length += text.Length;
        _version++;

        if (record && !_journalPaused)
        {
            RecordOperation(new TextBufferOperation(ChangeKind.Insert, position, text));
        }
    }

    private void DeleteInternal(int position, int length, bool record, string? capturedText = null)
    {
        if (length <= 0)
        {
            return;
        }

        ValidatePosition(position, allowEnd: true);

        if (position >= _length)
        {
            return;
        }

        var clampedLength = Math.Min(length, _length - position);
        var end = position + clampedLength;

        string? removedText = capturedText;
        if (record && !_journalPaused)
        {
            removedText ??= GetText(position, clampedLength);
        }

        var startIndex = SplitAt(position);
        var endIndex = SplitAt(end);

        _pieces.RemoveRange(startIndex, endIndex - startIndex);

        _length -= clampedLength;
        _version++;

        if (record && !_journalPaused && !string.IsNullOrEmpty(removedText))
        {
            RecordOperation(new TextBufferOperation(ChangeKind.Delete, position, removedText!));
        }
    }

    private void RecordOperation(in TextBufferOperation operation)
    {
        var now = DateTime.UtcNow;

        if (_undoStack.TryPeek(out var last) &&
            last.TryMerge(operation, now - _lastOperationUtc, out var merged))
        {
            _undoStack.Pop();
            _undoStack.Push(merged);
        }
        else
        {
            _undoStack.Push(operation);
        }

        _lastOperationUtc = now;
        _redoStack.Clear();
    }

    private int AddToAddBuffer(string text)
    {
        _addBuffers.Add(text);
        return _addBuffers.Count; // Buffer ids > 0 represent add buffer entries (1-based).
    }

    private void ValidatePosition(int position, bool allowEnd)
    {
        var max = allowEnd ? _length : Math.Max(_length - 1, 0);
        if (position < 0 || position > max)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }
    }

    private int SplitAt(int position)
    {
        if (_pieces.Count == 0 || position == _length)
        {
            return _pieces.Count;
        }

        var current = 0;
        for (var i = 0; i < _pieces.Count; i++)
        {
            var piece = _pieces[i];
            var next = current + piece.Length;

            if (position == current)
            {
                return i;
            }

            if (position > current && position < next)
            {
                var offset = position - current;
                SplitPiece(i, offset);
                return i + 1;
            }

            current = next;
        }

        return _pieces.Count;
    }

    private void SplitPiece(int index, int offset)
    {
        var piece = _pieces[index];
        if (offset <= 0 || offset >= piece.Length)
        {
            return;
        }

        var left = piece with { Length = offset };
        var right = piece with { Start = piece.Start + offset, Length = piece.Length - offset };

        _pieces[index] = left;
        _pieces.Insert(index + 1, right);
    }

    private ReadOnlyMemory<char> GetMemory(in Piece piece)
    {
        return piece.Kind switch
        {
            BufferKind.Original => _originalBuffer.AsMemory(piece.Start, piece.Length),
            BufferKind.Add => _addBuffers[piece.BufferId - 1].AsMemory(piece.Start, piece.Length),
            _ => ReadOnlyMemory<char>.Empty,
        };
    }

    private ReadOnlySpan<char> GetSpan(in Piece piece) => GetMemory(piece).Span;

    internal enum BufferKind
    {
        Original,
        Add,
    }

    internal readonly record struct Piece(BufferKind Kind, int BufferId, int Start, int Length);
}

internal enum ChangeKind
{
    Insert,
    Delete,
}

internal readonly record struct TextBufferOperation(ChangeKind Kind, int Position, string Text)
{
    private static readonly TimeSpan Threshold = TimeSpan.FromMilliseconds(PieceTreeTextBuffer.DefaultCoalesceThresholdMilliseconds);

    public bool TryMerge(TextBufferOperation incoming, TimeSpan delta, out TextBufferOperation merged)
    {
        merged = default;
        if (Kind != incoming.Kind)
        {
            return false;
        }

        if (delta > Threshold)
        {
            return false;
        }

        if (Kind == ChangeKind.Insert)
        {
            if (incoming.Position == Position + Text.Length)
            {
                merged = this with { Text = Text + incoming.Text };
                return true;
            }

            if (Position == incoming.Position + incoming.Text.Length)
            {
                merged = this with { Position = incoming.Position, Text = incoming.Text + Text };
                return true;
            }

            return false;
        }

        // Delete coalescing scenarios: same start (forward delete) or contiguous backwards (backspace).
        if (incoming.Position == Position)
        {
            merged = this with { Text = Text + incoming.Text };
            return true;
        }

        if (incoming.Position + incoming.Text.Length == Position)
        {
            merged = this with { Position = incoming.Position, Text = incoming.Text + Text };
            return true;
        }

        return false;
    }
}
