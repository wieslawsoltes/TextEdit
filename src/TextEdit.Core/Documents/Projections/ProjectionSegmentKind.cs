namespace TextEdit.Core.Documents.Projections;

/// <summary>
/// Indicates how a projection segment should be interpreted.
/// </summary>
public enum ProjectionSegmentKind
{
    Original,
    Added,
    Removed,
    Literal,
    Metadata,
}
