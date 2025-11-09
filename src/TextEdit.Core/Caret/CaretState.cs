using System;

namespace TextEdit.Core.Caret;

/// <summary>
/// Immutable snapshot describing a single caret entry managed by <see cref="CaretSelectionManager"/>.
/// </summary>
public readonly record struct CaretState(TextSelection Selection, bool IsPrimary)
{
    /// <summary>
    /// Gets the caret position.
    /// </summary>
    public TextPosition Position => Selection.Active;

    /// <summary>
    /// Gets the selection kind.
    /// </summary>
    public SelectionKind SelectionKind => Selection.Kind;

    /// <summary>
    /// Gets the column span when the selection is columnar.
    /// </summary>
    public ColumnSelectionSpan? ColumnSpan => Selection.ColumnSpan;

    /// <inheritdoc/>
    public override string ToString()
        => $"{(IsPrimary ? "*" : " ")} {Selection}";
}
