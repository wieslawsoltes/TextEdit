namespace TextEdit.Rendering.Graph;

/// <summary>
/// Abstracts drawing commands for the rendering graph to target.
/// </summary>
public interface IRenderContext
{
    void DrawRectangle(double x, double y, double width, double height, object brush);

    void DrawGlyphRun(double x, double y, object glyphRun);

    void DrawLine(double x1, double y1, double x2, double y2, object pen);
}
