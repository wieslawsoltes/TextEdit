using System;

namespace TextEdit.Core.Caret;

/// <summary>
/// Represents a logical position within a text buffer using zero-based line and column coordinates.
/// </summary>
public readonly record struct TextPosition : IComparable<TextPosition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextPosition"/> struct.
    /// </summary>
    /// <param name="line">Zero-based line index.</param>
    /// <param name="column">Zero-based column index.</param>
    public TextPosition(int line, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(line);
        ArgumentOutOfRangeException.ThrowIfNegative(column);

        Line = line;
        Column = column;
    }

    /// <summary>
    /// Gets the zero-based line index.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the zero-based column index.
    /// </summary>
    public int Column { get; }

    /// <inheritdoc/>
    public int CompareTo(TextPosition other)
    {
        var lineComparison = Line.CompareTo(other.Line);
        return lineComparison != 0 ? lineComparison : Column.CompareTo(other.Column);
    }

    /// <summary>
    /// Returns the earlier of two positions.
    /// </summary>
    public static TextPosition Min(in TextPosition left, in TextPosition right)
        => left.CompareTo(right) <= 0 ? left : right;

    /// <summary>
    /// Returns the later of two positions.
    /// </summary>
    public static TextPosition Max(in TextPosition left, in TextPosition right)
        => left.CompareTo(right) >= 0 ? left : right;

    /// <summary>
    /// Compares two positions for ordering.
    /// </summary>
    public static bool operator <(TextPosition left, TextPosition right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Compares two positions for ordering.
    /// </summary>
    public static bool operator >(TextPosition left, TextPosition right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Compares two positions for ordering.
    /// </summary>
    public static bool operator <=(TextPosition left, TextPosition right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Compares two positions for ordering.
    /// </summary>
    public static bool operator >=(TextPosition left, TextPosition right) => left.CompareTo(right) >= 0;

    /// <inheritdoc/>
    public override string ToString() => $"({Line},{Column})";
}
