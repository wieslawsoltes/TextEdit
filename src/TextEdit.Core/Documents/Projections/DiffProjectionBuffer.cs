using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TextEdit.Core.Documents.Projections;

/// <summary>
/// Projection buffer that compares the live document text to a comparison snapshot.
/// </summary>
public sealed class DiffProjectionBuffer : ProjectionBuffer
{
    private DocumentSnapshot? _comparisonSnapshot;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffProjectionBuffer"/> class.
    /// </summary>
    public DiffProjectionBuffer(
        Document document,
        DocumentSnapshot? comparisonSnapshot = null,
        DocumentChangeBus? changeBus = null)
        : base(document, changeBus, initializeSnapshot: false)
    {
        _comparisonSnapshot = comparisonSnapshot;
        InitializeSnapshot();
    }

    /// <summary>
    /// Replaces the comparison snapshot and rebuilds the projection.
    /// </summary>
    public void UpdateComparisonSnapshot(DocumentSnapshot comparisonSnapshot)
    {
        _comparisonSnapshot = comparisonSnapshot ?? throw new ArgumentNullException(nameof(comparisonSnapshot));
        RebuildSnapshot();
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<ProjectionSegment> BuildSegments(DocumentSnapshot documentSnapshot)
    {
        var current = documentSnapshot.GetText();
        var comparison = _comparisonSnapshot?.GetText() ?? string.Empty;

        if (string.Equals(current, comparison, StringComparison.Ordinal))
        {
            return new[]
            {
                new ProjectionSegment(current, ProjectionSegmentKind.Original),
            };
        }

        var segments = new List<ProjectionSegment>();
        if (!string.IsNullOrEmpty(comparison))
        {
            segments.Add(new ProjectionSegment(
                comparison,
                ProjectionSegmentKind.Removed,
                ImmutableDictionary<string, object?>.Empty.Add("label", "comparison")));
        }

        if (!string.IsNullOrEmpty(current))
        {
            segments.Add(new ProjectionSegment(
                current,
                ProjectionSegmentKind.Added,
                ImmutableDictionary<string, object?>.Empty.Add("label", "current")));
        }

        return segments;
    }
}
