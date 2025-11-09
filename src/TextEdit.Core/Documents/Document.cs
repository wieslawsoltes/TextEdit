using System;
using TextEdit.Core.Buffers;

namespace TextEdit.Core.Documents;

/// <summary>
/// Represents a live document instance backed by a piece-tree text buffer.
/// </summary>
public sealed class Document
{
    private readonly PieceTreeTextBuffer _buffer;
    private readonly DocumentChangeBus _changeBus;
    private DocumentVersion _version;

    /// <summary>
    /// Initializes a new instance of the <see cref="Document"/> class.
    /// </summary>
    /// <param name="initialText">Initial document content.</param>
    /// <param name="uri">Optional document URI (file path, network resource).</param>
    /// <param name="id">Optional identifier; generated when not provided.</param>
    public Document(string? initialText = null, Uri? uri = null, DocumentId? id = null, DocumentChangeBus? changeBus = null)
    {
        Id = id ?? DocumentId.CreateNew();
        Uri = uri;
        _buffer = new PieceTreeTextBuffer(initialText);
        _version = DocumentVersion.CreateInitial();
        IsDirty = false;
        _changeBus = changeBus ?? DocumentChangeBus.Global;
    }

    /// <summary>
    /// Gets the document identifier.
    /// </summary>
    public DocumentId Id { get; }

    /// <summary>
    /// Gets the optional document URI.
    /// </summary>
    public Uri? Uri { get; }

    /// <summary>
    /// Gets the current document version.
    /// </summary>
    public DocumentVersion Version => _version;

    /// <summary>
    /// Gets the document length.
    /// </summary>
    public int Length => _buffer.Length;

    /// <summary>
    /// Gets a value indicating whether the document has unsaved changes.
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <summary>
    /// Gets a value indicating whether an undo operation can be performed.
    /// </summary>
    public bool CanUndo => _buffer.CanUndo;

    /// <summary>
    /// Gets a value indicating whether a redo operation can be performed.
    /// </summary>
    public bool CanRedo => _buffer.CanRedo;

    /// <summary>
    /// Gets the underlying text content.
    /// </summary>
    public string GetText() => _buffer.ToString();

    /// <summary>
    /// Marks the document as clean (e.g., after persisting).
    /// </summary>
    public void MarkClean() => IsDirty = false;

    /// <summary>
    /// Inserts text into the document.
    /// </summary>
    public void Insert(int position, string text)
    {
        var previousVersion = _buffer.Version;
        _buffer.Insert(position, text);
        AdvanceVersion(previousVersion);
        if (!string.IsNullOrEmpty(text))
        {
            _changeBus.PublishChange(Id, _version, DocumentChange.Insert(position, text));
        }
    }

    /// <summary>
    /// Deletes text from the document.
    /// </summary>
    public void Delete(int position, int length)
    {
        var previousVersion = _buffer.Version;
        var deleteLength = Math.Min(length, Math.Max(0, _buffer.Length - position));
        if (deleteLength <= 0)
        {
            return;
        }

        var removed = _buffer.GetText(position, deleteLength);
        _buffer.Delete(position, deleteLength);
        AdvanceVersion(previousVersion);
        if (!string.IsNullOrEmpty(removed))
        {
            _changeBus.PublishChange(Id, _version, DocumentChange.Delete(position, removed));
        }
    }

    /// <summary>
    /// Performs an undo operation if available.
    /// </summary>
    public bool Undo()
    {
        var previousVersion = _buffer.Version;
        if (!_buffer.Undo())
        {
            return false;
        }

        AdvanceVersion(previousVersion);
        _changeBus.PublishChange(Id, _version, DocumentChange.Undo());
        return true;
    }

    /// <summary>
    /// Performs a redo operation if available.
    /// </summary>
    public bool Redo()
    {
        var previousVersion = _buffer.Version;
        if (!_buffer.Redo())
        {
            return false;
        }

        AdvanceVersion(previousVersion);
        _changeBus.PublishChange(Id, _version, DocumentChange.Redo());
        return true;
    }

    /// <summary>
    /// Creates a snapshot of the current document state.
    /// </summary>
    public DocumentSnapshot CreateSnapshot()
    {
        return new DocumentSnapshot(Id, _version, Uri, _buffer.CreateSnapshot());
    }

    /// <summary>
    /// Provides internal access to the underlying piece-tree buffer.
    /// </summary>
    internal PieceTreeTextBuffer InnerBuffer => _buffer;

    /// <summary>
    /// Begins a change transaction that batches notifications.
    /// </summary>
    public IDisposable BeginChangeTransaction() => _changeBus.BeginTransaction(Id);

    /// <summary>
    /// Flushes any pending batched change notifications immediately.
    /// </summary>
    public void FlushPendingChanges() => _changeBus.FlushPending(Id);

    private void AdvanceVersion(int previousBufferVersion)
    {
        if (_buffer.Version == previousBufferVersion)
        {
            return;
        }

        _version = _version.Next();
        IsDirty = true;
    }
}
