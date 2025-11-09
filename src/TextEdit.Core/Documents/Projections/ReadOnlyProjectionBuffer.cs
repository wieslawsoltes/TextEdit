using System.Collections.Generic;

namespace TextEdit.Core.Documents.Projections;

/// <summary>
/// Projection buffer exposing a read-only view of the underlying document.
/// </summary>
public sealed class ReadOnlyProjectionBuffer : ProjectionBuffer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyProjectionBuffer"/> class.
    /// </summary>
    public ReadOnlyProjectionBuffer(Document document, DocumentChangeBus? changeBus = null)
        : base(document, changeBus)
    {
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<ProjectionSegment> BuildSegments(DocumentSnapshot documentSnapshot)
    {
        return new[]
        {
            new ProjectionSegment(documentSnapshot.GetText(), ProjectionSegmentKind.Original),
        };
    }
}
