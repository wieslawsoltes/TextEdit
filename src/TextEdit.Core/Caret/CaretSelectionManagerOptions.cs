using System;

namespace TextEdit.Core.Caret;

/// <summary>
/// Configuration for <see cref="CaretSelectionManager"/>.
/// </summary>
public sealed class CaretSelectionManagerOptions
{
    private int _maxCaretCount = 128;

    /// <summary>
    /// Gets or sets the maximum number of carets that can be tracked simultaneously.
    /// </summary>
    public int MaxCaretCount
    {
        get => _maxCaretCount;
        set => _maxCaretCount = value <= 0
            ? throw new ArgumentOutOfRangeException(nameof(value))
            : value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether caret entries should be deduplicated.
    /// </summary>
    public bool DeduplicateCarets { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether caret entries should be sorted by document order.
    /// </summary>
    public bool SortCaretsByPosition { get; set; } = true;
}
