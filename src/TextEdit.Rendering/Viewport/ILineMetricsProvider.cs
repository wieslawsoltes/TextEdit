namespace TextEdit.Rendering.Viewport;

/// <summary>
/// Provides line height information required for viewport calculations.
/// </summary>
public interface ILineMetricsProvider
{
    /// <summary>
    /// Gets the number of lines available for rendering.
    /// </summary>
    int LineCount { get; }

    /// <summary>
    /// Returns the height of a specific line in device-independent pixels.
    /// </summary>
    /// <param name="lineIndex">Zero-based line index.</param>
    double GetLineHeight(int lineIndex);
}
