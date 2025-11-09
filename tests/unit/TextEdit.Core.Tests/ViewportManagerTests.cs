using System.Collections.Generic;
using TextEdit.Rendering.Viewport;

namespace TextEdit.Core.Tests;

public sealed class ViewportManagerTests
{
    [Fact]
    public void ComputesVisibleRangeWithUniformHeights()
    {
        var provider = new TestLineMetricsProvider(GenerateUniformHeights(10, 20));
        var manager = new ViewportManager(provider);

        var result = manager.Update(new ViewportState(scrollOffset: 0, viewportHeight: 60, overscanMargin: 0));

        Assert.Equal(0, result.FirstVisibleLine);
        Assert.Equal(2, result.LastVisibleLine);
        Assert.Equal(3, result.VisibleLineCount);
        Assert.Equal(60, result.LastLineBottom);
    }

    [Fact]
    public void CalculatesPartialLineOffset()
    {
        var provider = new TestLineMetricsProvider(GenerateUniformHeights(20, 18));
        var manager = new ViewportManager(provider);

        var result = manager.Update(new ViewportState(scrollOffset: 25, viewportHeight: 40, overscanMargin: 0));

        Assert.Equal(1, result.FirstVisibleLine);
        Assert.InRange(result.FirstLineOffset, 6.9, 7.1);
    }

    [Fact]
    public void OverscanExtendsBeyondViewport()
    {
        var provider = new TestLineMetricsProvider(GenerateUniformHeights(50, 24));
        var manager = new ViewportManager(provider);

        var state = new ViewportState(scrollOffset: 120, viewportHeight: 80, overscanMargin: 48);
        var result = manager.Update(state);

        Assert.True(result.OverscanStartLine < result.FirstVisibleLine);
        Assert.True(result.OverscanEndLine > result.LastVisibleLine);
    }

    [Fact]
    public void InvalidateFromLineRecalculatesPositions()
    {
        var heights = GenerateUniformHeights(5, 20);
        var provider = new TestLineMetricsProvider(heights);
        var manager = new ViewportManager(provider);

        var initial = manager.Update(new ViewportState(scrollOffset: 0, viewportHeight: 60));
        Assert.Equal(60, initial.LastLineBottom);

        heights[1] = 40;
        manager.InvalidateFromLine(1);
        var updated = manager.Update(new ViewportState(scrollOffset: 0, viewportHeight: 60));

        Assert.Equal(60, updated.LastLineBottom);
        Assert.Equal(1, updated.LastVisibleLine);
    }

    [Fact]
    public void HandlesScrollNearDocumentEnd()
    {
        var provider = new TestLineMetricsProvider(GenerateUniformHeights(4, 22));
        var manager = new ViewportManager(provider);

        var totalHeight = manager.TotalHeight;
        var state = new ViewportState(scrollOffset: totalHeight - 10, viewportHeight: 40);
        var result = manager.Update(state);

        Assert.Equal(3, result.LastVisibleLine);
        Assert.True(result.LastLineBottom <= totalHeight);
    }

    private static List<double> GenerateUniformHeights(int count, double height)
    {
        var list = new List<double>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(height);
        }

        return list;
    }

    private sealed class TestLineMetricsProvider : ILineMetricsProvider
    {
        private readonly List<double> _heights;

        public TestLineMetricsProvider(List<double> heights)
        {
            _heights = heights;
        }

        public int LineCount => _heights.Count;

        public double GetLineHeight(int lineIndex) => _heights[lineIndex];
    }
}
