using System.Collections.Generic;
using TextEdit.Rendering.Graph;
using TextEdit.Rendering.Minimap;
using TextEdit.Rendering.Overlay;
using TextEdit.Rendering.Overview;
using TextEdit.Rendering;

namespace TextEdit.Core.Tests;

public sealed class RenderGraphBuilderTests
{
    [Fact]
    public void OperationsExecuteInLayerOrder()
    {
        var builder = new RenderGraphBuilder();
        var executed = new List<string>();

        builder.AddOperation(RenderLayerKind.Overlay, 0, (ctx, state) => executed.Add("overlay"));
        builder.AddOperation(RenderLayerKind.Background, 0, (ctx, state) => executed.Add("background"));
        builder.AddOperation(RenderLayerKind.Text, 0, (ctx, state) => executed.Add("text"));

        var graph = builder.Build();
        graph.Execute(new FakeContext());

        Assert.Equal(ExpectedLayerOrder, executed);
    }

    [Fact]
    public void ZIndexOrdersWithinSameLayer()
    {
        var builder = new RenderGraphBuilder();
        var executed = new List<string>();

        builder.AddOperation(RenderLayerKind.Text, 5, (ctx, state) => executed.Add("5"));
        builder.AddOperation(RenderLayerKind.Text, 1, (ctx, state) => executed.Add("1"));
        builder.AddOperation(RenderLayerKind.Text, 3, (ctx, state) => executed.Add("3"));

        builder.Build().Execute(new FakeContext());

        Assert.Equal(ExpectedZOrder, executed);
    }

    [Fact]
    public void RenderStateIsSharedAcrossOperations()
    {
        var builder = new RenderGraphBuilder();

        builder.AddOperation(RenderLayerKind.Background, 0, (ctx, state) => state.Set("value", 42));
        builder.AddOperation(RenderLayerKind.Text, 0, (ctx, state) =>
        {
            Assert.True(state.TryGet("value", out int retrieved));
            Assert.Equal(42, retrieved);
        });

        builder.Build().Execute(new FakeContext());
    }

    private static readonly string[] ExpectedLayerOrder = { "background", "text", "overlay" };
    private static readonly string[] ExpectedZOrder = { "1", "3", "5" };

    [Fact]
    public void OverlayRegistryAddsOperationsToGraph()
    {
        var registry = RenderingPipeline.CreateOverlayRegistry();
        registry.Register("caret", new OverlayInfo(
            RenderLayerKind.Overlay,
            zIndex: 5,
            x: 10,
            y: 12,
            width: 1,
            height: 16,
            render: (ctx, state) =>
            {
                state.Set("overlay.rect", (10, 12, 1, 16));
            }));

        var builder = RenderingPipeline.CreateRenderGraphBuilder();
        registry.Contribute(builder);

        var graph = builder.Build();
        graph.Execute(new FakeContext());

        var snapshot = graph.State;
        Assert.True(snapshot.TryGet("overlay.rect", out (int x, int y, int width, int height) rect));
        Assert.Equal((10, 12, 1, 16), rect);
    }

    [Fact]
    public void MinimapGeneratorAddsBackgroundOperations()
    {
        var source = new FakeMinimapSource(4);
        var generator = new MinimapGenerator(source, line => "brush-" + line);
        var builder = RenderingPipeline.CreateRenderGraphBuilder();

        generator.AddToGraph(builder, x: 0, y: 0, lineHeight: 2, width: 4);

        var graph = builder.Build();
        graph.Execute(new FakeContext());

        Assert.True(graph.State.TryGet("minimap.lastLine", out int line));
        Assert.Equal(3, line);
    }

    [Fact]
    public void OverviewRulerGeneratorAddsDiagnosticsOperations()
    {
        var markers = new[]
        {
            new OverviewMarker(0, OverviewMarker.MarkerKind.Error),
            new OverviewMarker(10, OverviewMarker.MarkerKind.Warning),
        };

        var generator = new OverviewRulerGenerator(
            markers,
            lineCount: 20,
            kind => "pen-" + kind);

        var builder = RenderingPipeline.CreateRenderGraphBuilder();
        generator.AddToGraph(builder, x: 0, y: 0, height: 100);

        var graph = builder.Build();
        graph.Execute(new FakeContext());

        Assert.True(graph.State.TryGet("overview.lastMarker", out int markerLine));
        Assert.Equal(10, markerLine);
    }

    private sealed class FakeMinimapSource : IMinimapSource
    {
        public FakeMinimapSource(int lines)
        {
            LineCount = lines;
        }

        public int LineCount { get; }

        public string GetLinePreview(int lineIndex) => $"line {lineIndex}";
    }

    private sealed class FakeContext : IRenderContext
    {
        public void DrawRectangle(double x, double y, double width, double height, object brush)
        {
        }

        public void DrawGlyphRun(double x, double y, object glyphRun)
        {
        }

        public void DrawLine(double x1, double y1, double x2, double y2, object pen)
        {
        }
    }
}
