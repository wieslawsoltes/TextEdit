namespace TextEdit.Rendering.Overview;

/// <summary>
/// Represents a marker on the overview ruler (diagnostics, changes, bookmarks).
/// </summary>
public sealed record OverviewMarker(int LineIndex, OverviewMarker.MarkerKind Kind, double Thickness = 2.0)
{
    public enum MarkerKind
    {
        Error,
        Warning,
        Info,
        Selection,
        Change,
    }
}
