using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace TextEdit.Core.Buffers;

/// <summary>
/// Immutable snapshot of a <see cref="PieceTreeTextBuffer"/>.
/// </summary>
public sealed class PieceTreeSnapshot
{
    private readonly string _original;
    private readonly ImmutableArray<string> _addBuffers;
    private readonly ImmutableArray<PieceTreeTextBuffer.Piece> _pieces;

    internal PieceTreeSnapshot(
        int version,
        int length,
        string original,
        ImmutableArray<PieceTreeTextBuffer.Piece> pieces,
        ImmutableArray<string> addBuffers)
    {
        Version = version;
        Length = length;
        _original = original;
        _pieces = pieces;
        _addBuffers = addBuffers;
    }

    /// <summary>
    /// Gets the snapshot version.
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Gets the snapshot length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Materializes the snapshot content as a string.
    /// </summary>
    public string GetText()
    {
        if (_pieces.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(Length);
        foreach (var piece in _pieces)
        {
            builder.Append(GetSpan(piece));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Enumerates underlying chunks for downstream persistence/pipelines.
    /// </summary>
    public IEnumerable<ReadOnlyMemory<char>> GetChunks()
    {
        foreach (var piece in _pieces)
        {
            yield return GetMemory(piece);
        }
    }

    private ReadOnlyMemory<char> GetMemory(in PieceTreeTextBuffer.Piece piece)
    {
        return piece.Kind switch
        {
            PieceTreeTextBuffer.BufferKind.Original =>
                _original.AsMemory(piece.Start, piece.Length),
            PieceTreeTextBuffer.BufferKind.Add =>
                _addBuffers[piece.BufferId - 1].AsMemory(piece.Start, piece.Length),
            _ => ReadOnlyMemory<char>.Empty,
        };
    }

    private ReadOnlySpan<char> GetSpan(in PieceTreeTextBuffer.Piece piece)
    {
        return GetMemory(piece).Span;
    }
}
