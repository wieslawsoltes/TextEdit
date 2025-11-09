using System.Collections.Generic;

namespace TextEdit.Core.Documents.Projections;

/// <summary>
/// Represents a contiguous block of text within a projection along with semantic metadata.
/// </summary>
public sealed record ProjectionSegment(
    string Text,
    ProjectionSegmentKind Kind,
    IReadOnlyDictionary<string, object?>? Metadata = null);
