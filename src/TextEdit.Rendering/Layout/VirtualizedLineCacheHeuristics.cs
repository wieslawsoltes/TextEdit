using System;

namespace TextEdit.Rendering.Layout;

/// <summary>
/// Helper methods that compute cache sizing heuristics for virtualized line layout.
/// </summary>
internal static class VirtualizedLineCacheHeuristics
{
    /// <summary>
    /// Calculates the cache capacity required to hold the current viewport window and headroom.
    /// </summary>
    public static int CalculateCapacity(
        int visibleLineCount,
        int overscanLineCount,
        double headroomRatio,
        int minimumCapacity,
        int maximumCapacity)
    {
        var window = Math.Max(overscanLineCount, visibleLineCount);
        if (window <= 0)
        {
            return minimumCapacity;
        }

        var ratio = double.IsNaN(headroomRatio) || double.IsInfinity(headroomRatio)
            ? 0d
            : headroomRatio;

        var headroom = (int)Math.Round(window * ratio, MidpointRounding.AwayFromZero);
        headroom = Math.Clamp(headroom, 128, 4_096);

        var desired = window + headroom;
        desired = AlignUp(desired, 64);

        var target = NextPowerOfTwo(desired);
        target = Math.Clamp(target, minimumCapacity, maximumCapacity);
        return target;
    }

    private static int AlignUp(int value, int step)
    {
        if (step <= 0)
        {
            return value;
        }

        var remainder = value % step;
        return remainder == 0 ? value : value + (step - remainder);
    }

    private static int NextPowerOfTwo(int value)
    {
        if (value <= 0)
        {
            return 1;
        }

        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value++;
        return value;
    }
}
