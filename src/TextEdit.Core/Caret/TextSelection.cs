using System;

namespace TextEdit.Core.Caret;

/// <summary>
/// Represents a caret along with its selection metadata.
/// </summary>
public readonly record struct TextSelection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextSelection"/> struct.
    /// </summary>
    /// <remarks>
    /// Prefer using the static factory methods (<see cref="Caret"/>, <see cref="Stream(TextPosition, TextPosition, LogicalDirection?)"/>,
    /// <see cref="Column(int, int, int, LogicalDirection)"/>) to ensure invariants are applied consistently.
    /// </remarks>
    public TextSelection(
        TextPosition anchor,
        TextPosition active,
        SelectionKind kind,
        LogicalDirection activeDirection,
        ColumnSelectionSpan? columnSpan = null)
    {
        ColumnSelectionSpan? resolvedSpan = null;

        if (kind == SelectionKind.Column)
        {
            if (columnSpan is null)
            {
                throw new ArgumentException("Column selections require a column span.", nameof(columnSpan));
            }

            resolvedSpan = columnSpan.Value;

            if (resolvedSpan.Value.Line != anchor.Line)
            {
                throw new ArgumentException("Column selection anchor must reside on the same line as the column span.", nameof(anchor));
            }

            if (resolvedSpan.Value.Line != active.Line)
            {
                active = new TextPosition(resolvedSpan.Value.Line, active.Column);
            }
        }
        else if (columnSpan is not null)
        {
            throw new ArgumentException("Column span is only valid for column selections.", nameof(columnSpan));
        }

        if (kind == SelectionKind.Caret)
        {
            anchor = active;
        }
        else if (kind == SelectionKind.Stream && anchor == active)
        {
            kind = SelectionKind.Caret;
            anchor = active;
        }

        Anchor = anchor;
        Active = active;
        Kind = kind;
        ActiveDirection = activeDirection;
        ColumnSpan = resolvedSpan;
    }

    /// <summary>
    /// Gets the anchor endpoint of the selection.
    /// </summary>
    public TextPosition Anchor { get; }

    /// <summary>
    /// Gets the active endpoint (caret position).
    /// </summary>
    public TextPosition Active { get; }

    /// <summary>
    /// Gets the shape of the selection.
    /// </summary>
    public SelectionKind Kind { get; }

    /// <summary>
    /// Gets the logical direction associated with the active endpoint.
    /// </summary>
    public LogicalDirection ActiveDirection { get; }

    /// <summary>
    /// Gets the column-span metadata when <see cref="Kind"/> is <see cref="SelectionKind.Column"/>.
    /// </summary>
    public ColumnSelectionSpan? ColumnSpan { get; }

    /// <summary>
    /// Gets a value indicating whether the selection represents a caret only.
    /// </summary>
    public bool IsCaret => Kind == SelectionKind.Caret;

    /// <summary>
    /// Gets a value indicating whether the selection represents an empty span.
    /// </summary>
    public bool IsEmpty => Kind switch
    {
        SelectionKind.Caret => true,
        SelectionKind.Stream => Anchor == Active,
        SelectionKind.Column => ColumnSpan?.IsEmpty ?? true,
        _ => true,
    };

    /// <summary>
    /// Gets the logically earlier endpoint.
    /// </summary>
    public TextPosition Start => Kind == SelectionKind.Column
        ? new TextPosition(ColumnSpan!.Value.Line, ColumnSpan.Value.StartColumn)
        : TextPosition.Min(Anchor, Active);

    /// <summary>
    /// Gets the logically later endpoint.
    /// </summary>
    public TextPosition End => Kind == SelectionKind.Column
        ? new TextPosition(ColumnSpan!.Value.Line, ColumnSpan.Value.EndColumn)
        : TextPosition.Max(Anchor, Active);

    /// <summary>
    /// Gets the caret position.
    /// </summary>
    public TextPosition Position => Active;

    /// <summary>
    /// Creates a caret-only selection at the supplied position.
    /// </summary>
    public static TextSelection Caret(TextPosition position, LogicalDirection direction = LogicalDirection.Forward)
        => new(position, position, SelectionKind.Caret, direction);

    /// <summary>
    /// Creates a stream selection between the supplied endpoints.
    /// </summary>
    public static TextSelection Stream(TextPosition anchor, TextPosition active, LogicalDirection? direction = null)
    {
        var resolvedKind = anchor == active ? SelectionKind.Caret : SelectionKind.Stream;
        var resolvedDirection = direction ?? (anchor.CompareTo(active) <= 0
            ? LogicalDirection.Forward
            : LogicalDirection.Backward);

        anchor = resolvedKind == SelectionKind.Caret ? active : anchor;

        return new TextSelection(anchor, active, resolvedKind, resolvedDirection);
    }

    /// <summary>
    /// Creates a column selection on the specified line between inclusive column bounds.
    /// </summary>
    public static TextSelection Column(int line, int columnA, int columnB, LogicalDirection direction)
    {
        var span = new ColumnSelectionSpan(line, columnA, columnB);
        var anchor = direction == LogicalDirection.Backward
            ? new TextPosition(line, span.EndColumn)
            : new TextPosition(line, span.StartColumn);

        var active = direction == LogicalDirection.Backward
            ? new TextPosition(line, span.StartColumn)
            : new TextPosition(line, span.EndColumn);

        return new TextSelection(anchor, active, SelectionKind.Column, direction, span);
    }

    /// <summary>
    /// Returns a new selection with an updated active endpoint.
    /// </summary>
    public TextSelection WithActive(TextPosition active, LogicalDirection direction)
    {
        return Kind switch
        {
            SelectionKind.Caret => Stream(Anchor, active, direction),
            SelectionKind.Stream => new TextSelection(Anchor, active, SelectionKind.Stream, direction),
            SelectionKind.Column when ColumnSpan is { } span => Column(
                span.Line,
                ActiveDirection == LogicalDirection.Backward ? span.EndColumn : span.StartColumn,
                active.Column,
                direction),
            _ => this,
        };
    }

    /// <summary>
    /// Returns a new selection with an updated anchor endpoint.
    /// </summary>
    public TextSelection WithAnchor(TextPosition anchor, LogicalDirection direction)
    {
        return Kind switch
        {
            SelectionKind.Caret => Stream(anchor, Active, direction),
            SelectionKind.Stream => new TextSelection(anchor, Active, SelectionKind.Stream, direction),
            SelectionKind.Column when ColumnSpan is { } span => Column(
                span.Line,
                anchor.Column,
                ActiveDirection == LogicalDirection.Backward ? span.StartColumn : span.EndColumn,
                direction),
            _ => this,
        };
    }

    /// <summary>
    /// Deconstructs the selection into its core components.
    /// </summary>
    public void Deconstruct(
        out TextPosition anchor,
        out TextPosition active,
        out SelectionKind kind,
        out LogicalDirection direction,
        out ColumnSelectionSpan? columnSpan)
    {
        anchor = Anchor;
        active = Active;
        kind = Kind;
        direction = ActiveDirection;
        columnSpan = ColumnSpan;
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"{Kind} Anchor={Anchor} Active={Active} Direction={ActiveDirection}";
}
