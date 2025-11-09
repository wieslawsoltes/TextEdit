using System;
using System.Collections.Generic;

namespace TextEdit.Rendering.Viewport;

/// <summary>
/// Computes the visible line window for a virtualized text view based on scroll position.
/// </summary>
public sealed class ViewportManager
{
    private readonly ILineMetricsProvider _lineMetrics;
    private readonly SortedDictionary<int, double> _lineTopCache = new() { { 0, 0 } };
    private double? _totalHeightCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportManager"/> class.
    /// </summary>
    public ViewportManager(ILineMetricsProvider lineMetrics)
    {
        _lineMetrics = lineMetrics ?? throw new ArgumentNullException(nameof(lineMetrics));
    }

    /// <summary>
    /// Gets the total height of the document in device-independent pixels.
    /// </summary>
    public double TotalHeight => ComputeTotalHeight();

    /// <summary>
    /// Computes the visible and overscan line ranges for the supplied viewport state.
    /// </summary>
    public ViewportComputationResult Update(ViewportState state)
    {
        var lineCount = _lineMetrics.LineCount;
        if (lineCount == 0 || state.ViewportHeight <= 0)
        {
            return new ViewportComputationResult
            {
                ScrollOffset = 0,
                ViewportHeight = state.ViewportHeight,
                TotalHeight = 0,
                FirstVisibleLine = 0,
                LastVisibleLine = 0,
                LastLineBottom = 0,
                OverscanStartLine = 0,
                OverscanEndLine = 0,
                OverscanStartOffset = 0,
                OverscanEndOffset = 0,
            };
        }

        var totalHeight = ComputeTotalHeight();
        var maxOffset = Math.Max(0, totalHeight - state.ViewportHeight);
        var scrollOffset = Math.Clamp(state.ScrollOffset, 0, maxOffset);

        var firstLine = FindLineAtOffset(scrollOffset);
        var firstLineTop = EnsureLineTop(firstLine);
        var firstLineOffset = scrollOffset - firstLineTop;

        var viewportBottom = Math.Min(totalHeight, scrollOffset + state.ViewportHeight);
        var lastLine = firstLine;
        var lastLineBottom = firstLineTop;

        while (lastLine < lineCount)
        {
            var height = _lineMetrics.GetLineHeight(lastLine);
            lastLineBottom += height;
            if (lastLineBottom >= viewportBottom || lastLine == lineCount - 1)
            {
                break;
            }

            lastLine++;
        }

        var overscanStartOffset = Math.Max(0, scrollOffset - state.OverscanMargin);
        var overscanEndOffset = Math.Min(totalHeight, viewportBottom + state.OverscanMargin);

        var overscanStartLine = FindLineAtOffset(overscanStartOffset);
        var overscanEndLine = FindLineAtOffset(Math.Max(0, overscanEndOffset - double.Epsilon));

        return new ViewportComputationResult
        {
            ScrollOffset = scrollOffset,
            ViewportHeight = state.ViewportHeight,
            TotalHeight = totalHeight,
            FirstVisibleLine = firstLine,
            FirstLineOffset = firstLineOffset,
            LastVisibleLine = lastLine,
            LastLineBottom = lastLineBottom,
            OverscanStartLine = overscanStartLine,
            OverscanEndLine = overscanEndLine,
            OverscanStartOffset = overscanStartOffset,
            OverscanEndOffset = overscanEndOffset,
        };
    }

    /// <summary>
    /// Invalidate caches from the specified line index onward.
    /// </summary>
    public void InvalidateFromLine(int lineIndex)
    {
        if (lineIndex <= 0)
        {
            _lineTopCache.Clear();
            _lineTopCache[0] = 0;
        }
        else
        {
            var keysToRemove = new List<int>();
            foreach (var key in _lineTopCache.Keys)
            {
                if (key >= lineIndex)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (key == 0)
                {
                    continue;
                }

                _lineTopCache.Remove(key);
            }
        }

        _totalHeightCache = null;
    }

    /// <summary>
    /// Clears all cached line positions forcing recalculation on next update.
    /// </summary>
    public void Reset()
    {
        _lineTopCache.Clear();
        _lineTopCache[0] = 0;
        _totalHeightCache = null;
    }

    /// <summary>
    /// Returns the top position (in pixels) of the specified line index.
    /// </summary>
    public double GetLineTop(int lineIndex) => EnsureLineTop(lineIndex);

    /// <summary>
    /// Returns the bottom position (in pixels) of the specified line index.
    /// </summary>
    public double GetLineBottom(int lineIndex) => EnsureLineTop(lineIndex) + _lineMetrics.GetLineHeight(lineIndex);

    private double ComputeTotalHeight()
    {
        if (_totalHeightCache.HasValue)
        {
            return _totalHeightCache.Value;
        }

        var lineCount = _lineMetrics.LineCount;
        if (lineCount == 0)
        {
            _totalHeightCache = 0;
            return 0;
        }

        var top = EnsureLineTop(lineCount - 1);
        _totalHeightCache = top + _lineMetrics.GetLineHeight(lineCount - 1);
        return _totalHeightCache.Value;
    }

    private int FindLineAtOffset(double offset)
    {
        var lineCount = _lineMetrics.LineCount;
        if (lineCount == 0)
        {
            return 0;
        }

        offset = Math.Clamp(offset, 0, Math.Max(0, ComputeTotalHeight() - double.Epsilon));

        int low = 0;
        int high = lineCount - 1;

        while (low <= high)
        {
            var mid = low + ((high - low) / 2);
            var top = EnsureLineTop(mid);
            var bottom = top + _lineMetrics.GetLineHeight(mid);

            if (offset < top)
            {
                high = mid - 1;
            }
            else if (offset >= bottom)
            {
                low = mid + 1;
            }
            else
            {
                return mid;
            }
        }

        return Math.Clamp(low, 0, lineCount - 1);
    }

    private double EnsureLineTop(int lineIndex)
    {
        if (lineIndex <= 0)
        {
            return 0;
        }

        if (_lineTopCache.TryGetValue(lineIndex, out var cached))
        {
            return cached;
        }

        var lowerKey = -1;
        var lowerValue = 0d;
        var upperKey = -1;
        var upperValue = 0d;

        foreach (var kvp in _lineTopCache)
        {
            if (kvp.Key < lineIndex)
            {
                lowerKey = kvp.Key;
                lowerValue = kvp.Value;
            }
            else if (kvp.Key > lineIndex)
            {
                upperKey = kvp.Key;
                upperValue = kvp.Value;
                break;
            }
            else
            {
                return kvp.Value;
            }
        }

        if (lowerKey >= 0)
        {
            var top = lowerValue;
            for (var i = lowerKey; i < lineIndex; i++)
            {
                top += _lineMetrics.GetLineHeight(i);
                var key = i + 1;
                if (!_lineTopCache.ContainsKey(key))
                {
                    _lineTopCache[key] = top;
                }
            }

            return _lineTopCache[lineIndex];
        }

        if (upperKey >= 0)
        {
            var top = upperValue;
            for (var i = upperKey - 1; i >= lineIndex; i--)
            {
                top -= _lineMetrics.GetLineHeight(i);
                _lineTopCache[i] = top;
            }

            return _lineTopCache[lineIndex];
        }

        var cumulative = 0d;
        for (var i = 0; i < lineIndex; i++)
        {
            cumulative += _lineMetrics.GetLineHeight(i);
            _lineTopCache[i + 1] = cumulative;
        }

        return cumulative;
    }
}
