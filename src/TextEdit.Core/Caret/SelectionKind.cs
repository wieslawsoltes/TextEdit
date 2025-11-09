namespace TextEdit.Core.Caret;

/// <summary>
/// Describes the shape of a selection associated with a caret.
/// </summary>
public enum SelectionKind
{
    /// <summary>
    /// No selection, caret only.
    /// </summary>
    Caret,

    /// <summary>
    /// Stream (linear) selection between two points.
    /// </summary>
    Stream,

    /// <summary>
    /// Columnar (rectangular) selection spanning multiple lines.
    /// </summary>
    Column,
}
