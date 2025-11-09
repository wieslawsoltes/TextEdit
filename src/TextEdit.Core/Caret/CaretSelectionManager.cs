using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TextEdit.Core.Caret;

/// <summary>
/// Manages caret and selection state, including multi-caret and columnar selections.
/// </summary>
public sealed class CaretSelectionManager
{
    private readonly object _gate = new();
    private readonly CaretSelectionManagerOptions _options;
    private ImmutableArray<TextSelection> _caretSelections = ImmutableArray<TextSelection>.Empty;
    private int _primaryIndex = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaretSelectionManager"/> class.
    /// </summary>
    public CaretSelectionManager(CaretSelectionManagerOptions? options = null)
    {
        _options = options ?? new CaretSelectionManagerOptions();
    }

    /// <summary>
    /// Raised when the caret or selection state changes.
    /// </summary>
    public event EventHandler<CaretSelectionChangedEventArgs>? CaretSelectionChanged;

    /// <summary>
    /// Gets the current number of tracked carets.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _caretSelections.Length;
            }
        }
    }

    /// <summary>
    /// Gets the index of the primary caret, or -1 if none.
    /// </summary>
    public int PrimaryCaretIndex
    {
        get
        {
            lock (_gate)
            {
                return _primaryIndex;
            }
        }
    }

    /// <summary>
    /// Gets the primary caret state, if available.
    /// </summary>
    public CaretState? PrimaryCaret
    {
        get
        {
            lock (_gate)
            {
                if (_primaryIndex < 0 || _caretSelections.IsDefaultOrEmpty)
                {
                    return null;
                }

                var selection = _caretSelections[_primaryIndex];
                return new CaretState(selection, true);
            }
        }
    }

    /// <summary>
    /// Returns a snapshot of all caret states.
    /// </summary>
    public ImmutableArray<CaretState> CaptureState()
    {
        lock (_gate)
        {
            return CreateState(_caretSelections, _primaryIndex);
        }
    }

    /// <summary>
    /// Clears all carets and selections.
    /// </summary>
    public void Clear() => Update(ImmutableArray<TextSelection>.Empty, -1);

    /// <summary>
    /// Sets a single caret without a selection.
    /// </summary>
    public void SetSingleCaret(TextPosition position, LogicalDirection direction = LogicalDirection.Forward)
        => SetCarets(new[] { TextSelection.Caret(position, direction) }, primaryIndex: 0);

    /// <summary>
    /// Replaces the current caret collection.
    /// </summary>
    public void SetCarets(IEnumerable<TextSelection> selections, int primaryIndex = 0)
    {
        var (normalized, normalizedPrimary) = NormalizeSelections(selections, primaryIndex);
        Update(normalized, normalizedPrimary);
    }

    /// <summary>
    /// Adds a caret to the collection.
    /// </summary>
    public void AddCaret(TextSelection selection, bool makePrimary = false)
    {
        var existing = CaptureSelections();
        var list = existing.IsDefaultOrEmpty
            ? new List<TextSelection>()
            : new List<TextSelection>(existing);

        list.Add(selection);

        var currentPrimary = PrimaryCaretIndex;
        var newPrimary = makePrimary || list.Count == 1
            ? list.Count - 1
            : Math.Clamp(currentPrimary, 0, list.Count - 1);

        SetCarets(list, newPrimary);
    }

    /// <summary>
    /// Removes the caret at the specified index.
    /// </summary>
    public void RemoveCaretAt(int index)
    {
        var existing = CaptureSelections();
        if (existing.IsDefaultOrEmpty || index < 0 || index >= existing.Length)
        {
            return;
        }

        var list = existing.ToList();
        list.RemoveAt(index);

        var currentPrimary = PrimaryCaretIndex;
        var newPrimary = list.Count == 0
            ? -1
            : Math.Clamp(currentPrimary, 0, list.Count - 1);

        SetCarets(list, newPrimary);
    }

    /// <summary>
    /// Extends the primary selection to a new active position.
    /// </summary>
    public void ExtendPrimarySelection(TextPosition newActive, LogicalDirection direction = LogicalDirection.Forward)
    {
        var snapshot = CaptureSelections();
        var primary = PrimaryCaretIndex;
        if (snapshot.IsDefaultOrEmpty || primary < 0)
        {
            SetSingleCaret(newActive, direction);
            return;
        }

        var selection = snapshot[primary];
        TextSelection updated = selection.Kind switch
        {
            SelectionKind.Caret => TextSelection.Stream(selection.Active, newActive, direction),
            SelectionKind.Stream => TextSelection.Stream(selection.Anchor, newActive, direction),
            SelectionKind.Column when selection.ColumnSpan is { } span => TextSelection.Column(
                span.Line,
                direction == LogicalDirection.Backward ? span.EndColumn : span.StartColumn,
                Math.Max(newActive.Column, 0),
                direction),
            _ => selection,
        };

        var builder = snapshot.ToBuilder();
        builder[primary] = NormalizeSelection(updated);

        var (normalized, normalizedPrimary) = NormalizeSelections(builder, primary);
        Update(normalized, normalizedPrimary);
    }

    /// <summary>
    /// Configures a column (rectangular) selection spanning the supplied anchors.
    /// </summary>
    public void SetColumnSelection(
        TextPosition anchor,
        TextPosition active,
        ITextLineProvider lineProvider,
        LogicalDirection? horizontalDirection = null)
    {
        ArgumentNullException.ThrowIfNull(lineProvider);

        if (lineProvider.LineCount <= 0)
        {
            SetSingleCaret(new TextPosition(0, 0));
            return;
        }

        var anchorLine = ClampLine(anchor.Line, lineProvider.LineCount);
        var activeLine = ClampLine(active.Line, lineProvider.LineCount);

        var startLine = Math.Min(anchorLine, activeLine);
        var endLine = Math.Max(anchorLine, activeLine);

        var anchorColumn = Math.Max(anchor.Column, 0);
        var activeColumn = Math.Max(active.Column, 0);

        var direction = horizontalDirection ?? (anchorColumn <= activeColumn
            ? LogicalDirection.Forward
            : LogicalDirection.Backward);

        var leftColumn = Math.Min(anchorColumn, activeColumn);
        var rightColumn = Math.Max(anchorColumn, activeColumn);

        var selections = new List<TextSelection>();
        var activeLineIndex = -1;

        for (var line = startLine; line <= endLine; line++)
        {
            var lineLength = lineProvider.GetLineLength(line);
            var clampedLeft = Math.Min(leftColumn, lineLength);
            var clampedRight = Math.Min(rightColumn, lineLength);

            var anchorColumnForLine = direction == LogicalDirection.Backward ? clampedRight : clampedLeft;
            var activeColumnForLine = direction == LogicalDirection.Backward ? clampedLeft : clampedRight;

            var selection = TextSelection.Column(
                line,
                anchorColumnForLine,
                activeColumnForLine,
                direction);

            selections.Add(selection);

            if (line == activeLine)
            {
                activeLineIndex = selections.Count - 1;
            }
        }

        var primaryIndex = selections.Count == 0
            ? -1
            : (activeLineIndex >= 0 ? activeLineIndex : selections.Count - 1);

        SetCarets(selections, primaryIndex);
    }

    private ImmutableArray<TextSelection> CaptureSelections()
    {
        lock (_gate)
        {
            return _caretSelections;
        }
    }

    private void Update(ImmutableArray<TextSelection> newSelections, int newPrimaryIndex)
    {
        ImmutableArray<TextSelection> previous;
        int previousPrimary;

        lock (_gate)
        {
            previous = _caretSelections;
            previousPrimary = _primaryIndex;

            if (previous.SequenceEqual(newSelections) && previousPrimary == newPrimaryIndex)
            {
                return;
            }

            _caretSelections = newSelections;
            _primaryIndex = newPrimaryIndex;
        }

        var previousState = CreateState(previous, previousPrimary);
        var currentState = CreateState(newSelections, newPrimaryIndex);
        CaretSelectionChanged?.Invoke(this, new CaretSelectionChangedEventArgs(previousState, currentState));
    }

    private (ImmutableArray<TextSelection> Selections, int PrimaryIndex) NormalizeSelections(IEnumerable<TextSelection> selections, int primaryIndex)
    {
        ArgumentNullException.ThrowIfNull(selections);

        var list = new List<TextSelection>();
        HashSet<TextSelection>? unique = _options.DeduplicateCarets ? new HashSet<TextSelection>() : null;
        TextSelection? requestedPrimary = null;
        var index = 0;

        foreach (var selection in selections)
        {
            var normalized = NormalizeSelection(selection);

            if (index == primaryIndex)
            {
                requestedPrimary = normalized;
            }

            if (_options.DeduplicateCarets && !unique!.Add(normalized))
            {
                index++;
                continue;
            }

            list.Add(normalized);
            index++;
        }

        if (list.Count == 0)
        {
            return (ImmutableArray<TextSelection>.Empty, -1);
        }

        var primarySelection = requestedPrimary.HasValue && list.Contains(requestedPrimary.Value)
            ? requestedPrimary.Value
            : list[Math.Clamp(primaryIndex, 0, list.Count - 1)];

        if (_options.SortCaretsByPosition && list.Count > 1)
        {
            list.Sort(static (a, b) =>
            {
                var cmp = a.Active.CompareTo(b.Active);
                if (cmp != 0)
                {
                    return cmp;
                }

                return a.Anchor.CompareTo(b.Anchor);
            });

            primaryIndex = list.FindIndex(s => s.Equals(primarySelection));
            if (primaryIndex < 0)
            {
                primaryIndex = 0;
            }
        }
        else
        {
            primaryIndex = list.FindIndex(s => s.Equals(primarySelection));
            if (primaryIndex < 0)
            {
                primaryIndex = Math.Clamp(primaryIndex, 0, list.Count - 1);
            }
        }

        list = TrimCaretList(list, ref primaryIndex);

        var builder = ImmutableArray.CreateBuilder<TextSelection>(list.Count);
        builder.AddRange(list);
        return (builder.ToImmutable(), list.Count == 0 ? -1 : primaryIndex);
    }

    private List<TextSelection> TrimCaretList(List<TextSelection> list, ref int primaryIndex)
    {
        if (list.Count <= _options.MaxCaretCount)
        {
            return list;
        }

        var window = _options.MaxCaretCount;

        if (primaryIndex < 0)
        {
            return list.GetRange(0, window);
        }

        var start = Math.Clamp(primaryIndex - window + 1, 0, Math.Max(list.Count - window, 0));
        var trimmed = list.GetRange(start, Math.Min(window, list.Count - start));
        primaryIndex -= start;
        return trimmed;
    }

    private static TextSelection NormalizeSelection(TextSelection selection)
    {
        return selection.Kind switch
        {
            SelectionKind.Caret => TextSelection.Caret(selection.Active, selection.ActiveDirection),
            SelectionKind.Stream when selection.Anchor == selection.Active =>
                TextSelection.Caret(selection.Active, selection.ActiveDirection),
            SelectionKind.Stream => new TextSelection(selection.Anchor, selection.Active, SelectionKind.Stream, selection.ActiveDirection),
            SelectionKind.Column when selection.ColumnSpan is { } span => TextSelection.Column(
                span.Line,
                selection.ActiveDirection == LogicalDirection.Backward ? span.EndColumn : span.StartColumn,
                selection.ActiveDirection == LogicalDirection.Backward ? span.StartColumn : span.EndColumn,
                selection.ActiveDirection),
            SelectionKind.Column => TextSelection.Column(
                selection.Active.Line,
                selection.Anchor.Column,
                selection.Active.Column,
                selection.ActiveDirection),
            _ => selection,
        };
    }

    private static ImmutableArray<CaretState> CreateState(ImmutableArray<TextSelection> selections, int primaryIndex)
    {
        if (selections.IsDefaultOrEmpty)
        {
            return ImmutableArray<CaretState>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<CaretState>(selections.Length);
        for (var i = 0; i < selections.Length; i++)
        {
            builder.Add(new CaretState(selections[i], i == primaryIndex));
        }

        return builder.ToImmutable();
    }

    private static int ClampLine(int line, int lineCount)
    {
        if (lineCount <= 0)
        {
            return 0;
        }

        return Math.Clamp(line, 0, lineCount - 1);
    }
}
