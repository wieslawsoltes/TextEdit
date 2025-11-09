using System;
using System.Collections.Generic;
using TextEdit.Core.Buffers;

namespace TextEdit.Core.Documents;

/// <summary>
/// Immutable view of a document at a specific point in time.
/// </summary>
public sealed class DocumentSnapshot
{
    private readonly PieceTreeSnapshot _bufferSnapshot;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentSnapshot"/> class.
    /// </summary>
    public DocumentSnapshot(DocumentId id, DocumentVersion version, Uri? uri, PieceTreeSnapshot bufferSnapshot)
    {
        ArgumentNullException.ThrowIfNull(bufferSnapshot);
        Id = id;
        Version = version;
        Uri = uri;
        _bufferSnapshot = bufferSnapshot;
    }

    /// <summary>
    /// Gets the document identifier associated with this snapshot.
    /// </summary>
    public DocumentId Id { get; }

    /// <summary>
    /// Gets the document version captured by the snapshot.
    /// </summary>
    public DocumentVersion Version { get; }

    /// <summary>
    /// Gets the optional document URI (e.g., file path).
    /// </summary>
    public Uri? Uri { get; }

    /// <summary>
    /// Gets the length of the snapshot in characters.
    /// </summary>
    public int Length => _bufferSnapshot.Length;

    /// <summary>
    /// Gets the underlying piece-tree snapshot.
    /// </summary>
    public PieceTreeSnapshot BufferSnapshot => _bufferSnapshot;

    /// <summary>
    /// Materializes the snapshot text content.
    /// </summary>
    public string GetText() => _bufferSnapshot.GetText();

    /// <summary>
    /// Enumerates the underlying text chunks that compose this snapshot.
    /// </summary>
    public IEnumerable<ReadOnlyMemory<char>> GetChunks() => _bufferSnapshot.GetChunks();
}
