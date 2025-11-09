using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TextEdit.Rendering.Viewport;

namespace TextEdit.Rendering.Layout;

/// <summary>
/// Asynchronously measures line layouts for the visible viewport and surrounding overscan range.
/// </summary>
public sealed class LineLayoutScheduler : IDisposable
{
    private readonly ILineLayoutProvider _layoutProvider;
    private readonly LineLayoutSchedulerOptions _options;
    private readonly object _gate = new();
    private CancellationTokenSource _updateCts = new();
    private Task _activeTask = Task.CompletedTask;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineLayoutScheduler"/> class.
    /// </summary>
    public LineLayoutScheduler(
        ILineLayoutProvider layoutProvider,
        VirtualizedLineCache? cache = null,
        LineLayoutSchedulerOptions? options = null)
    {
        _layoutProvider = layoutProvider ?? throw new ArgumentNullException(nameof(layoutProvider));
        _options = options ?? new LineLayoutSchedulerOptions();
        Cache = cache ?? new VirtualizedLineCache(_options.CacheMinimumCapacity);
        if (cache is null || options is not null)
        {
            Cache.ConfigureBounds(_options.CacheMinimumCapacity, _options.CacheMaximumCapacity);
        }
    }

    /// <summary>
    /// Raised when a line layout measurement completes.
    /// </summary>
    public event EventHandler<LineLayoutMeasuredEventArgs>? LineMeasured;

    /// <summary>
    /// Gets the underlying cache storing measured lines.
    /// </summary>
    public VirtualizedLineCache Cache { get; }

    /// <summary>
    /// Requests layout measurements for the specified viewport.
    /// </summary>
    public void RequestLayout(ViewportComputationResult viewport)
    {
        ArgumentNullException.ThrowIfNull(viewport);
        ThrowIfDisposed();

        var (startLine, endLine) = NormalizeRange(viewport);

        if (_options.AutoAdjustCacheSize)
        {
            var windowLineCount = endLine >= startLine ? endLine - startLine + 1 : 0;
            Cache.AdjustCapacity(viewport.VisibleLineCount, windowLineCount, _options.CacheHeadroomRatio);
        }

        var newCts = new CancellationTokenSource();
        CancellationTokenSource? previousCts;

        lock (_gate)
        {
            previousCts = _updateCts;
            _updateCts = newCts;
            _activeTask = RunAsync(startLine, endLine, _updateCts.Token);
        }

        previousCts?.Cancel();
        previousCts?.Dispose();
    }

    /// <summary>
    /// Waits for the current update to complete.
    /// </summary>
    public Task WhenIdleAsync()
    {
        lock (_gate)
        {
            return _activeTask ?? Task.CompletedTask;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _updateCts.Cancel();
        _updateCts.Dispose();
    }

    private async Task RunAsync(int startLine, int endLine, CancellationToken token)
    {
        if (endLine < startLine)
        {
            return;
        }

        try
        {
            var lines = Enumerable.Range(startLine, endLine - startLine + 1);
            using var throttler = new SemaphoreSlim(_options.MaxConcurrentMeasurements);
            var tasks = new List<Task>();

            foreach (var line in lines)
            {
                if (Cache.TryGet(line, out _))
                {
                    continue;
                }

                await throttler.WaitAsync(token).ConfigureAwait(false);
                tasks.Add(MeasureLineAsync(line, throttler, token));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            if (_options.TrimCacheAfterUpdate)
            {
                Cache.TrimOutsideRange(startLine, endLine);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when a new viewport request arrives.
        }
    }

    private async Task MeasureLineAsync(int lineIndex, SemaphoreSlim throttler, CancellationToken token)
    {
        try
        {
            var info = await _layoutProvider.MeasureAsync(lineIndex, token).ConfigureAwait(false);
            Cache.Set(info);
            LineMeasured?.Invoke(this, new LineLayoutMeasuredEventArgs(info));
        }
        catch (OperationCanceledException)
        {
            // Swallow cancellation so other tasks can continue.
        }
        finally
        {
            throttler.Release();
        }
    }

    private (int Start, int End) NormalizeRange(ViewportComputationResult viewport)
    {
        var lineCount = _layoutProvider.LineCount;
        if (lineCount == 0)
        {
            return (0, -1);
        }

        var start = Math.Min(
            Math.Max(viewport.OverscanStartLine, 0),
            lineCount - 1);
        start = Math.Min(start, viewport.FirstVisibleLine);

        var end = Math.Max(
            Math.Min(viewport.OverscanEndLine, lineCount - 1),
            viewport.LastVisibleLine);
        end = Math.Clamp(end, start, lineCount - 1);

        return (start, end);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(LineLayoutScheduler));
    }
}
