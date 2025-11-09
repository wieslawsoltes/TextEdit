namespace TextEdit.Rendering.Layout;

/// <summary>
/// Contains measurement information for a single line.
/// </summary>
public sealed record LineLayoutInfo(int LineIndex, double Width, double Height, double Baseline);
