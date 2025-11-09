using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using TextEdit.Rendering.Layout;
using TextEdit.Rendering.Viewport;

namespace TextEdit.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks focused on measuring line layout scheduling throughput for large documents.
/// </summary>
[MemoryDiagnoser]
public class RenderingThroughputBenchmarks
{
    private const int LargeDocumentLineCount = 200_000;

    private LargeDocumentLayoutProvider _layoutProvider = null!;
    private ViewportComputationResult _largeViewport = null!;
    private LineLayoutSchedulerOptions _adaptiveOptions = null!;
    private LineLayoutSchedulerOptions _fixedOptions = null!;

    [GlobalSetup]
    public void Setup()
    {
        _layoutProvider = new LargeDocumentLayoutProvider(LargeDocumentLineCount);

        _adaptiveOptions = new LineLayoutSchedulerOptions
        {
            MaxConcurrentMeasurements = Math.Max(Environment.ProcessorCount, 1),
            CacheMinimumCapacity = 512,
            CacheMaximumCapacity = 16_384,
            CacheHeadroomRatio = 0.75,
            AutoAdjustCacheSize = true,
            TrimCacheAfterUpdate = false,
        };

        _fixedOptions = new LineLayoutSchedulerOptions
        {
            MaxConcurrentMeasurements = Math.Max(Environment.ProcessorCount, 1),
            AutoAdjustCacheSize = false,
            CacheMinimumCapacity = 512,
            CacheMaximumCapacity = 512,
            TrimCacheAfterUpdate = false,
        };

        _largeViewport = new ViewportComputationResult
        {
            ScrollOffset = 3_000_000,
            ViewportHeight = 1_200,
            TotalHeight = _layoutProvider.LineCount * LargeDocumentLayoutProvider.LineHeight,
            FirstVisibleLine = 150_000,
            FirstLineOffset = 0,
            LastVisibleLine = 150_140,
            LastLineBottom = (150_141) * LargeDocumentLayoutProvider.LineHeight,
            OverscanStartLine = 148_000,
            OverscanEndLine = 152_200,
            OverscanStartOffset = 148_000 * LargeDocumentLayoutProvider.LineHeight,
            OverscanEndOffset = (152_201) * LargeDocumentLayoutProvider.LineHeight,
        };
    }

    [Benchmark(Description = "Adaptive cache sizing (auto)")]
    public async Task AdaptiveCacheSchedulingAsync()
    {
        using var scheduler = new LineLayoutScheduler(_layoutProvider, options: _adaptiveOptions);
        scheduler.RequestLayout(_largeViewport);
        await scheduler.WhenIdleAsync().ConfigureAwait(false);
    }

    [Benchmark(Description = "Fixed 512-line cache")]
    public async Task FixedCacheSchedulingAsync()
    {
        using var scheduler = new LineLayoutScheduler(_layoutProvider, options: _fixedOptions);
        scheduler.RequestLayout(_largeViewport);
        await scheduler.WhenIdleAsync().ConfigureAwait(false);
    }

    private sealed class LargeDocumentLayoutProvider : ILineLayoutProvider
    {
        internal const double LineHeight = 20d;
        private const double LineWidth = 1_024d;
        private const double Baseline = 16d;

        public LargeDocumentLayoutProvider(int lineCount)
        {
            if (lineCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineCount));
            }

            LineCount = lineCount;
        }

        public int LineCount { get; }

        public ValueTask<LineLayoutInfo> MeasureAsync(int lineIndex, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<LineLayoutInfo>(CreateLayout(lineIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LineLayoutInfo CreateLayout(int lineIndex)
            => new(lineIndex, LineWidth, LineHeight, Baseline);
    }
}
