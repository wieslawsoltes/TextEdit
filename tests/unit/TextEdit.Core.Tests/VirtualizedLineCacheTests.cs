using TextEdit.Rendering.Layout;

namespace TextEdit.Core.Tests;

public sealed class VirtualizedLineCacheTests
{
    [Fact]
    public void AdjustCapacityExpandsForLargeWindow()
    {
        var cache = new VirtualizedLineCache();

        cache.AdjustCapacity(visibleLineCount: 120, overscanLineCount: 1_200, headroomRatio: 0.75);

        Assert.Equal(4_096, cache.Capacity);
    }

    [Fact]
    public void AdjustCapacityShrinksBackToMinimum()
    {
        var cache = new VirtualizedLineCache();

        cache.AdjustCapacity(visibleLineCount: 120, overscanLineCount: 1_200, headroomRatio: 0.75);
        Assert.True(cache.Capacity > 512);

        cache.AdjustCapacity(visibleLineCount: 50, overscanLineCount: 80, headroomRatio: 0.75);

        Assert.Equal(512, cache.Capacity);
    }

    [Fact]
    public void ConfigureBoundsConstrainsCapacity()
    {
        var cache = new VirtualizedLineCache();
        cache.ConfigureBounds(minimumCapacity: 256, maximumCapacity: 1_024);

        cache.AdjustCapacity(visibleLineCount: 200, overscanLineCount: 2_000, headroomRatio: 0.75);

        Assert.Equal(1_024, cache.Capacity);
    }
}
