using TextEdit.Core;
using TextEdit.Rendering.Graph;
using TextEdit.Rendering.Layout;
using TextEdit.Rendering.Minimap;
using TextEdit.Rendering.Overlay;
using TextEdit.Rendering.Overview;
using TextEdit.Rendering.Viewport;

namespace TextEdit.Rendering;

/// <summary>
/// Placeholder for the rendering pipeline coordinator.
/// Links to <see cref="EditorKernel"/> for shared state during future milestones.
/// </summary>
public sealed class RenderingPipeline
{
    /// <summary>
    /// Exposes the core version to confirm project wiring in the sample app.
    /// </summary>
    public static string KernelVersion => EditorKernel.Version;

    /// <summary>
    /// Creates a viewport manager bound to the supplied line metrics provider.
    /// </summary>
    public static ViewportManager CreateViewportManager(ILineMetricsProvider provider)
        => new(provider);

    /// <summary>
    /// Creates a line layout scheduler for asynchronous measurement.
    /// </summary>
    public static LineLayoutScheduler CreateLineLayoutScheduler(
        ILineLayoutProvider layoutProvider,
        VirtualizedLineCache? cache = null,
        LineLayoutSchedulerOptions? options = null)
        => new(layoutProvider, cache, options);

    /// <summary>
    /// Creates a render graph builder to compose layered drawing operations.
    /// </summary>
    public static RenderGraphBuilder CreateRenderGraphBuilder() => new();

    /// <summary>
    /// Creates a registry for overlays and adornments.
    /// </summary>
    public static OverlayRegistry CreateOverlayRegistry() => new();

    /// <summary>
    /// Creates a minimap generator.
    /// </summary>
    public static MinimapGenerator CreateMinimapGenerator(IMinimapSource source, Func<int, object> lineBrushFactory)
        => new(source, lineBrushFactory);

    /// <summary>
    /// Creates an overview ruler generator.
    /// </summary>
    public static OverviewRulerGenerator CreateOverviewRulerGenerator(
        IReadOnlyList<OverviewMarker> markers,
        int lineCount,
        Func<OverviewMarker.MarkerKind, object> penFactory)
        => new(markers, lineCount, penFactory);
}
