using System;
using System.Collections.Generic;
using System.Linq;

namespace TextEdit.Rendering.Layout;

/// <summary>
/// Stores line layout information for a virtualized viewport with simple LRU eviction.
/// </summary>
public sealed class VirtualizedLineCache
{
    private const int DefaultMaximumCapacity = 16_384;

    private readonly object _gate = new();
    private readonly Dictionary<int, LineLayoutInfo> _entries = new();
    private readonly LinkedList<int> _lru = new();
    private int _capacity;
    private int _minimumCapacity;
    private int _maximumCapacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualizedLineCache"/> class.
    /// </summary>
    public VirtualizedLineCache(int capacity = 512)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0);

        _minimumCapacity = capacity;
        _maximumCapacity = Math.Max(capacity, DefaultMaximumCapacity);
        _capacity = Math.Clamp(capacity, _minimumCapacity, _maximumCapacity);
    }

    /// <summary>
    /// Gets the number of cached lines.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count;
            }
        }
    }

    /// <summary>
    /// Gets the current cache capacity.
    /// </summary>
    public int Capacity
    {
        get
        {
            lock (_gate)
            {
                return _capacity;
            }
        }
    }

    /// <summary>
    /// Updates the minimum and maximum cache capacity bounds.
    /// </summary>
    public void ConfigureBounds(int minimumCapacity, int maximumCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(minimumCapacity, 0);

        ArgumentOutOfRangeException.ThrowIfLessThan(maximumCapacity, minimumCapacity, nameof(maximumCapacity));

        lock (_gate)
        {
            _minimumCapacity = minimumCapacity;
            _maximumCapacity = maximumCapacity;
            var clamped = Math.Clamp(_capacity, _minimumCapacity, _maximumCapacity);
            ResizeCore(clamped);
        }
    }

    /// <summary>
    /// Adjusts the cache capacity according to the supplied viewport heuristics.
    /// </summary>
    public void AdjustCapacity(int visibleLineCount, int overscanLineCount, double headroomRatio)
    {
        if (double.IsNaN(headroomRatio) || headroomRatio < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(headroomRatio));
        }

        lock (_gate)
        {
            var target = VirtualizedLineCacheHeuristics.CalculateCapacity(
                visibleLineCount,
                overscanLineCount,
                headroomRatio,
                _minimumCapacity,
                _maximumCapacity);

            ResizeCore(target);
        }
    }

    /// <summary>
    /// Attempts to retrieve layout information for the specified line.
    /// </summary>
    public bool TryGet(int lineIndex, out LineLayoutInfo info)
    {
        lock (_gate)
        {
            if (_entries.TryGetValue(lineIndex, out var stored))
            {
                MoveToFront(lineIndex);
                info = stored;
                return true;
            }

            info = null!;
            return false;
        }
    }

    /// <summary>
    /// Inserts or updates layout information for the specified line.
    /// </summary>
    public void Set(LineLayoutInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        lock (_gate)
        {
            if (_entries.ContainsKey(info.LineIndex))
            {
                _entries[info.LineIndex] = info;
                MoveToFront(info.LineIndex);
                return;
            }

            _entries[info.LineIndex] = info;
            _lru.AddFirst(info.LineIndex);

            if (_entries.Count > _capacity && _lru.Last is { } node)
            {
                _entries.Remove(node.Value);
                _lru.RemoveLast();
            }
        }
    }

    /// <summary>
    /// Removes cached entries outside the specified inclusive range.
    /// </summary>
    public void TrimOutsideRange(int startLine, int endLine)
    {
        if (endLine < startLine)
        {
            (startLine, endLine) = (endLine, startLine);
        }

        lock (_gate)
        {
            var toRemove = _entries.Keys.Where(k => k < startLine || k > endLine).ToList();
            foreach (var key in toRemove)
            {
                _entries.Remove(key);
                RemoveFromLru(key);
            }
        }
    }

    /// <summary>
    /// Returns a snapshot of cached line indices.
    /// </summary>
    public IReadOnlyCollection<int> SnapshotLineIndices()
    {
        lock (_gate)
        {
            return _entries.Keys.ToArray();
        }
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void Clear()
    {
        lock (_gate)
        {
            _entries.Clear();
            _lru.Clear();
        }
    }

    private void MoveToFront(int lineIndex)
    {
        var node = _lru.Find(lineIndex);
        if (node is null)
        {
            _lru.AddFirst(lineIndex);
        }
        else
        {
            _lru.Remove(node);
            _lru.AddFirst(node);
        }
    }

    private void RemoveFromLru(int lineIndex)
    {
        var node = _lru.Find(lineIndex);
        if (node is not null)
        {
            _lru.Remove(node);
        }
    }

    private void ResizeCore(int newCapacity)
    {
        if (newCapacity != _capacity)
        {
            _capacity = newCapacity;
        }

        while (_entries.Count > _capacity && _lru.Last is { } nodeToEvict)
        {
            _entries.Remove(nodeToEvict.Value);
            _lru.RemoveLast();
        }
    }
}
