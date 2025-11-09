using System;
using System.Collections.Generic;
using System.Linq;

namespace TextEdit.Core.Documents.Projections;

/// <summary>
/// Immutable representation of the data exposed by a projection buffer.
/// </summary>
public sealed class ProjectionSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionSnapshot"/> class.
    /// </summary>
    public ProjectionSnapshot(DocumentId documentId, DocumentVersion version, IReadOnlyList<ProjectionSegment> segments)
    {
        DocumentId = documentId;
        Version = version;
        Segments = segments;
        Text = string.Concat(segments.Select(static s => s.Text));
    }

    /// <summary>
    /// Gets the originating document identifier.
    /// </summary>
    public DocumentId DocumentId { get; }

    /// <summary>
    /// Gets the document version used to populate this snapshot.
    /// </summary>
    public DocumentVersion Version { get; }

    /// <summary>
    /// Gets the concatenated text of the projection.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the individual segments composing the projection.
    /// </summary>
    public IReadOnlyList<ProjectionSegment> Segments { get; }
}
