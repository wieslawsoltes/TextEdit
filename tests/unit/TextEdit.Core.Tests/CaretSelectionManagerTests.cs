using System;
using System.Linq;
using TextEdit.Core.Caret;
using TextEdit.Core.Documents;

namespace TextEdit.Core.Tests;

public sealed class CaretSelectionManagerTests
{
    [Fact]
    public void SetSingleCaretCreatesPrimaryEntry()
    {
        var manager = new CaretSelectionManager();
        manager.SetSingleCaret(new TextPosition(2, 4));

        var state = manager.CaptureState();
        var caret = Assert.Single(state);

        Assert.True(caret.IsPrimary);
        Assert.True(caret.Selection.IsCaret);
        Assert.Equal(new TextPosition(2, 4), caret.Position);
        Assert.Equal(LogicalDirection.Forward, caret.Selection.ActiveDirection);
    }

    [Fact]
    public void SetCaretsDeduplicatesAndSorts()
    {
        var manager = new CaretSelectionManager(new CaretSelectionManagerOptions
        {
            MaxCaretCount = 8,
        });

        var selections = new[]
        {
            TextSelection.Stream(new TextPosition(5, 10), new TextPosition(5, 12)),
            TextSelection.Stream(new TextPosition(1, 4), new TextPosition(1, 8)),
            TextSelection.Stream(new TextPosition(5, 10), new TextPosition(5, 12)),
        };

        manager.SetCarets(selections, primaryIndex: 2);

        var state = manager.CaptureState();
        Assert.Equal(2, state.Length);

        Assert.Equal(new TextPosition(1, 8), state[0].Selection.Active);
        Assert.False(state[0].IsPrimary);

        Assert.True(state[1].IsPrimary);
        Assert.Equal(new TextPosition(5, 12), state[1].Selection.Active);
    }

    [Fact]
    public void ExtendPrimarySelectionSupportsBackwardDirection()
    {
        var manager = new CaretSelectionManager();
        manager.SetSingleCaret(new TextPosition(0, 5));

        manager.ExtendPrimarySelection(new TextPosition(0, 2), LogicalDirection.Backward);

        var state = manager.CaptureState();
        var caret = Assert.Single(state);

        Assert.Equal(SelectionKind.Stream, caret.Selection.Kind);
        Assert.Equal(new TextPosition(0, 5), caret.Selection.Anchor);
        Assert.Equal(new TextPosition(0, 2), caret.Selection.Active);
        Assert.Equal(LogicalDirection.Backward, caret.Selection.ActiveDirection);
    }

    [Fact]
    public void ColumnSelectionCreatesRectangularSlices()
    {
        var provider = new DocumentLineProvider("alpha\nbe\nlongerline");
        var manager = new CaretSelectionManager();

        manager.SetColumnSelection(
            anchor: new TextPosition(0, 1),
            active: new TextPosition(2, 5),
            lineProvider: provider);

        var state = manager.CaptureState();
        Assert.Equal(3, state.Length);

        Assert.All(state, caret => Assert.Equal(SelectionKind.Column, caret.Selection.Kind));

        Assert.Equal(new ColumnSelectionSpan(0, 1, 5), state[0].Selection.ColumnSpan);
        Assert.Equal(new ColumnSelectionSpan(1, 1, 2), state[1].Selection.ColumnSpan);
        Assert.Equal(new ColumnSelectionSpan(2, 1, 5), state[2].Selection.ColumnSpan);
        Assert.True(state[2].IsPrimary);
    }

    [Fact]
    public void ColumnSelectionHonorsBackwardDirection()
    {
        var provider = new DocumentLineProvider("abcd");
        var manager = new CaretSelectionManager();

        manager.SetColumnSelection(
            anchor: new TextPosition(0, 3),
            active: new TextPosition(0, 1),
            lineProvider: provider);

        var caret = Assert.Single(manager.CaptureState());
        Assert.Equal(LogicalDirection.Backward, caret.Selection.ActiveDirection);
        Assert.Equal(new ColumnSelectionSpan(0, 1, 3), caret.Selection.ColumnSpan);
        Assert.Equal(new TextPosition(0, 3), caret.Selection.Anchor);
        Assert.Equal(new TextPosition(0, 1), caret.Selection.Active);
    }

    [Fact]
    public void RespectsMaxCaretCountLimit()
    {
        var manager = new CaretSelectionManager(new CaretSelectionManagerOptions
        {
            MaxCaretCount = 3,
        });

        var selections = Enumerable.Range(0, 10)
            .Select(i => TextSelection.Caret(new TextPosition(i, 0)))
            .ToArray();

        manager.SetCarets(selections, primaryIndex: 9);

        var state = manager.CaptureState();
        Assert.Equal(3, state.Length);
        Assert.True(state[^1].IsPrimary);
        Assert.Equal(new TextPosition(9, 0), state[^1].Selection.Active);
    }
}
