namespace TextEdit.Rendering.Graph;

/// <summary>
/// Represents a GPU-friendly rendering operation with ordering metadata.
/// </summary>
public sealed record RenderOperation(
    RenderLayerKind Layer,
    double ZIndex,
    Action<IRenderContext, RenderState> Execute);
