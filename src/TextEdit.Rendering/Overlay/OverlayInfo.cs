using System;
using TextEdit.Rendering.Graph;

namespace TextEdit.Rendering.Overlay;

/// <summary>
/// Describes an overlay element positioned relative to the viewport.
/// </summary>
public sealed class OverlayInfo
{
    public OverlayInfo(
        RenderLayerKind layer,
        double zIndex,
        double x,
        double y,
        double width,
        double height,
        Action<IRenderContext, RenderState> render)
    {
        Layer = layer;
        ZIndex = zIndex;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Render = render ?? throw new ArgumentNullException(nameof(render));
    }

    public RenderLayerKind Layer { get; }

    public double ZIndex { get; }

    public double X { get; }

    public double Y { get; }

    public double Width { get; }

    public double Height { get; }

    public Action<IRenderContext, RenderState> Render { get; }
}
