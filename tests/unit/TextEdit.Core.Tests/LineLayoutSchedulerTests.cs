using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TextEdit.Rendering.Layout;
using TextEdit.Rendering.Viewport;

namespace TextEdit.Core.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007", Justification = "xUnit synchronization context handles continuations for async tests.")]
public sealed class LineLayoutSchedulerTests
{
    [Fact]
    public async Task SchedulerPopulatesCacheForOverscanRange()
    {
        var provider = new FakeLayoutProvider(lineCount: 100, measurementDelay: TimeSpan.FromMilliseconds(5));
        var cache = new VirtualizedLineCache(capacity: 200);
        using var scheduler = new LineLayoutScheduler(provider, cache);

        var viewport = new ViewportComputationResult
        {
            ScrollOffset = 0,
            ViewportHeight = 100,
            TotalHeight = 2000,
            FirstVisibleLine = 10,
            LastVisibleLine = 14,
            OverscanStartLine = 8,
            OverscanEndLine = 20,
            OverscanStartOffset = 0,
            OverscanEndOffset = 0,
        };

        scheduler.RequestLayout(viewport);
        await scheduler.WhenIdleAsync().WaitAsync(TimeSpan.FromSeconds(2));

        foreach (var line in Enumerable.Range(8, 13))
        {
            Assert.True(cache.TryGet(line, out _), $"Expected cache to contain line {line}.");
        }
    }

    [Fact]
    public async Task SchedulerCancelsPreviousRequests()
    {
        var provider = new FakeLayoutProvider(lineCount: 100, measurementDelay: TimeSpan.FromMilliseconds(50));
        var cache = new VirtualizedLineCache(capacity: 200);
        using var scheduler = new LineLayoutScheduler(provider, cache);

        var firstViewport = new ViewportComputationResult
        {
            FirstVisibleLine = 0,
            LastVisibleLine = 5,
            OverscanStartLine = 0,
            OverscanEndLine = 9,
        };

        scheduler.RequestLayout(firstViewport);
        await Task.Delay(10);

        var secondViewport = new ViewportComputationResult
        {
            FirstVisibleLine = 40,
            LastVisibleLine = 45,
            OverscanStartLine = 38,
            OverscanEndLine = 48,
        };

        scheduler.RequestLayout(secondViewport);
        await scheduler.WhenIdleAsync().WaitAsync(TimeSpan.FromSeconds(2));

        Assert.True(provider.MeasuredLines.All(line => line >= 38),
            $"Unexpected measurement outside requested range: {string.Join(',', provider.MeasuredLines)}");
    }

    [Fact]
    public async Task SchedulerTrimsCacheOutsideOverscan()
    {
        var provider = new FakeLayoutProvider(lineCount: 100, measurementDelay: TimeSpan.Zero);
        var cache = new VirtualizedLineCache(capacity: 200);
        using var scheduler = new LineLayoutScheduler(provider, cache);

        var firstViewport = new ViewportComputationResult
        {
            FirstVisibleLine = 0,
            LastVisibleLine = 4,
            OverscanStartLine = 0,
            OverscanEndLine = 9,
        };

        scheduler.RequestLayout(firstViewport);
        await scheduler.WhenIdleAsync().WaitAsync(TimeSpan.FromSeconds(1));

        var secondViewport = new ViewportComputationResult
        {
            FirstVisibleLine = 30,
            LastVisibleLine = 34,
            OverscanStartLine = 28,
            OverscanEndLine = 38,
        };

        scheduler.RequestLayout(secondViewport);
        await scheduler.WhenIdleAsync().WaitAsync(TimeSpan.FromSeconds(1));

        var cachedLines = cache.SnapshotLineIndices();
        Assert.True(cachedLines.All(line => line >= 28 && line <= 38), $"Cache contains lines outside range: {string.Join(',', cachedLines)}");
    }

    private sealed class FakeLayoutProvider : ILineLayoutProvider
    {
        private readonly TimeSpan _delay;
        private readonly ConcurrentDictionary<int, bool> _measured = new();

        public FakeLayoutProvider(int lineCount, TimeSpan measurementDelay)
        {
            LineCount = lineCount;
            _delay = measurementDelay;
        }

        public int LineCount { get; }

        public IReadOnlyCollection<int> MeasuredLines => _measured.Keys.ToArray();

        public async ValueTask<LineLayoutInfo> MeasureAsync(int lineIndex, CancellationToken cancellationToken)
        {
            if (_delay > TimeSpan.Zero)
            {
                await Task.Delay(_delay, cancellationToken);
            }

            var info = new LineLayoutInfo(
                lineIndex,
                Width: 100,
                Height: 20,
                Baseline: 15);

            _measured[lineIndex] = true;
            return info;
        }
    }
}
