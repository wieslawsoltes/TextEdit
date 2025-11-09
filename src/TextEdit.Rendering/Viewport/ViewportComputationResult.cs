namespace TextEdit.Rendering.Viewport;

/// <summary>
/// Contains the computed visible line range and offsets for a viewport update.
/// </summary>
public sealed class ViewportComputationResult
{
    /// <summary>
    /// Gets the normalized scroll offset used for this computation.
    /// </summary>
    public double ScrollOffset { get; init; }

    /// <summary>
    /// Gets the viewport height.
    /// </summary>
    public double ViewportHeight { get; init; }

    /// <summary>
    /// Gets the total document height.
    /// </summary>
    public double TotalHeight { get; init; }

    /// <summary>
    /// Gets the first visible line index.
    /// </summary>
    public int FirstVisibleLine { get; init; }

    /// <summary>
    /// Gets the offset (in pixels) of the scroll position within <see cref="FirstVisibleLine"/>.
    /// </summary>
    public double FirstLineOffset { get; init; }

    /// <summary>
    /// Gets the last visible line index.
    /// </summary>
    public int LastVisibleLine { get; init; }

    /// <summary>
    /// Gets the pixel coordinate of the bottom edge of the last visible line.
    /// </summary>
    public double LastLineBottom { get; init; }

    /// <summary>
    /// Gets the number of visible lines.
    /// </summary>
    public int VisibleLineCount => LastVisibleLine >= FirstVisibleLine ? (LastVisibleLine - FirstVisibleLine + 1) : 0;

    /// <summary>
    /// Gets the first line index within the overscan range.
    /// </summary>
    public int OverscanStartLine { get; init; }

    /// <summary>
    /// Gets the last line index within the overscan range.
    /// </summary>
    public int OverscanEndLine { get; init; }

    /// <summary>
    /// Gets the number of lines included in the overscan window.
    /// </summary>
    public int OverscanLineCount => OverscanEndLine >= OverscanStartLine ? (OverscanEndLine - OverscanStartLine + 1) : 0;

    /// <summary>
    /// Gets the pixel offset for the overscan start.
    /// </summary>
    public double OverscanStartOffset { get; init; }

    /// <summary>
    /// Gets the pixel offset for the overscan end.
    /// </summary>
    public double OverscanEndOffset { get; init; }
}
