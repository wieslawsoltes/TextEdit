using System.Collections.Generic;

namespace TextEdit.Core.Documents;

/// <summary>
/// Represents a batched set of document changes delivered to observers.
/// </summary>
public sealed class DocumentChangeSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentChangeSet"/> class.
    /// </summary>
    public DocumentChangeSet(DocumentId id, DocumentVersion version, IReadOnlyList<DocumentChange> changes)
    {
        DocumentId = id;
        Version = version;
        Changes = changes;
    }

    /// <summary>
    /// Gets the document identifier.
    /// </summary>
    public DocumentId DocumentId { get; }

    /// <summary>
    /// Gets the document version most recently applied.
    /// </summary>
    public DocumentVersion Version { get; }

    /// <summary>
    /// Gets the list of changes included in this batch.
    /// </summary>
    public IReadOnlyList<DocumentChange> Changes { get; }
}
