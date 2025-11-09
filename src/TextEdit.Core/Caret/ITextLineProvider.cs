namespace TextEdit.Core.Caret;

/// <summary>
/// Provides line metrics required for caret and selection normalization.
/// </summary>
public interface ITextLineProvider
{
    /// <summary>
    /// Gets the number of lines available.
    /// </summary>
    int LineCount { get; }

    /// <summary>
    /// Gets the length (in columns) of the specified line.
    /// </summary>
    int GetLineLength(int lineIndex);
}
