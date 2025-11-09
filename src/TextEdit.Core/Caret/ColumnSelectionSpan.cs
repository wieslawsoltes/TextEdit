using System;

namespace TextEdit.Core.Caret;

/// <summary>
/// Represents a single row slice participating in a column selection.
/// </summary>
public readonly record struct ColumnSelectionSpan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnSelectionSpan"/> struct.
    /// </summary>
    /// <param name="line">Zero-based line index.</param>
    /// <param name="startColumn">Inclusive start column.</param>
    /// <param name="endColumn">Exclusive end column.</param>
    public ColumnSelectionSpan(int line, int startColumn, int endColumn)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(line);
        ArgumentOutOfRangeException.ThrowIfNegative(startColumn);
        ArgumentOutOfRangeException.ThrowIfNegative(endColumn);

        Line = line;
        if (endColumn < startColumn)
        {
            (startColumn, endColumn) = (endColumn, startColumn);
        }

        StartColumn = startColumn;
        EndColumn = endColumn;
    }

    /// <summary>
    /// Gets the zero-based line index associated with this span.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the inclusive start column.
    /// </summary>
    public int StartColumn { get; }

    /// <summary>
    /// Gets the exclusive end column.
    /// </summary>
    public int EndColumn { get; }

    /// <summary>
    /// Gets the span length in columns.
    /// </summary>
    public int Length => Math.Max(EndColumn - StartColumn, 0);

    /// <summary>
    /// Gets a value indicating whether the span is empty.
    /// </summary>
    public bool IsEmpty => StartColumn == EndColumn;

    /// <summary>
    /// Creates a new span using the same line but different column bounds.
    /// </summary>
    public ColumnSelectionSpan WithBounds(int startColumn, int endColumn) => new(Line, startColumn, endColumn);
}
