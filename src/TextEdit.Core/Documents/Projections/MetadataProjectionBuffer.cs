using System;
using System.Collections.Generic;

namespace TextEdit.Core.Documents.Projections;

/// <summary>
/// Projection buffer that applies metadata overlays to a document snapshot via a user-provided selector.
/// </summary>
public sealed class MetadataProjectionBuffer : ProjectionBuffer
{
    private readonly Func<DocumentSnapshot, IEnumerable<ProjectionSegment>> _segmentProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataProjectionBuffer"/> class.
    /// </summary>
    /// <param name="document">Source document.</param>
    /// <param name="segmentProvider">Delegate used to produce projection segments from a document snapshot.</param>
    /// <param name="changeBus">Optional change bus; uses global bus when <c>null</c>.</param>
    public MetadataProjectionBuffer(
        Document document,
        Func<DocumentSnapshot, IEnumerable<ProjectionSegment>> segmentProvider,
        DocumentChangeBus? changeBus = null)
        : base(document, changeBus, initializeSnapshot: false)
    {
        _segmentProvider = segmentProvider ?? throw new ArgumentNullException(nameof(segmentProvider));
        InitializeSnapshot();
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<ProjectionSegment> BuildSegments(DocumentSnapshot documentSnapshot)
    {
        var segments = new List<ProjectionSegment>();
        foreach (var segment in _segmentProvider(documentSnapshot))
        {
            segments.Add(segment);
        }

        return segments;
    }
}
