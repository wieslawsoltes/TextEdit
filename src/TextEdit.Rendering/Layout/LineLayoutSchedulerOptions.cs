using System;

namespace TextEdit.Rendering.Layout;

/// <summary>
/// Configuration options for <see cref="LineLayoutScheduler"/>.
/// </summary>
public sealed class LineLayoutSchedulerOptions
{
    private int _maxConcurrentMeasurements = Math.Max(Environment.ProcessorCount / 2, 1);
    private int _cacheMinimumCapacity = 512;
    private int _cacheMaximumCapacity = 16_384;
    private double _cacheHeadroomRatio = 0.75;

    /// <summary>
    /// Gets or sets the maximum number of lines measured concurrently.
    /// </summary>
    public int MaxConcurrentMeasurements
    {
        get => _maxConcurrentMeasurements;
        set => _maxConcurrentMeasurements = value <= 0
            ? throw new ArgumentOutOfRangeException(nameof(value))
            : value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the cache should be trimmed to the overscan range after each update.
    /// </summary>
    public bool TrimCacheAfterUpdate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the cache should automatically resize to match the viewport window.
    /// </summary>
    public bool AutoAdjustCacheSize { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum cache capacity.
    /// </summary>
    public int CacheMinimumCapacity
    {
        get => _cacheMinimumCapacity;
        set => _cacheMinimumCapacity = value <= 0
            ? throw new ArgumentOutOfRangeException(nameof(value))
            : value > _cacheMaximumCapacity
                ? throw new ArgumentOutOfRangeException(nameof(value), "Minimum cache capacity cannot exceed the maximum.")
                : value;
    }

    /// <summary>
    /// Gets or sets the maximum cache capacity.
    /// </summary>
    public int CacheMaximumCapacity
    {
        get => _cacheMaximumCapacity;
        set => _cacheMaximumCapacity = value <= 0
            ? throw new ArgumentOutOfRangeException(nameof(value))
            : value < _cacheMinimumCapacity
                ? throw new ArgumentOutOfRangeException(nameof(value), "Maximum cache capacity cannot be smaller than the minimum.")
                : value;
    }

    /// <summary>
    /// Gets or sets the ratio of headroom lines kept in the cache relative to the overscan window.
    /// </summary>
    public double CacheHeadroomRatio
    {
        get => _cacheHeadroomRatio;
        set => _cacheHeadroomRatio = double.IsNaN(value) || double.IsInfinity(value) || value < 0
            ? throw new ArgumentOutOfRangeException(nameof(value))
            : value;
    }
}
