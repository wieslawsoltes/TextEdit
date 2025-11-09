namespace TextEdit.Rendering.Viewport;

/// <summary>
/// Represents the state of the viewport when computing visible lines.
/// </summary>
public readonly record struct ViewportState
{
    public ViewportState(double scrollOffset, double viewportHeight, double overscanMargin = 0)
    {
        ScrollOffset = scrollOffset < 0 ? 0 : scrollOffset;
        ViewportHeight = viewportHeight < 0 ? 0 : viewportHeight;
        OverscanMargin = overscanMargin < 0 ? 0 : overscanMargin;
    }

    /// <summary>
    /// Gets the vertical scroll offset in device-independent pixels.
    /// </summary>
    public double ScrollOffset { get; }

    /// <summary>
    /// Gets the visible viewport height in device-independent pixels.
    /// </summary>
    public double ViewportHeight { get; }

    /// <summary>
    /// Gets the additional margin applied above and below the viewport for prefetching lines.
    /// </summary>
    public double OverscanMargin { get; }
}
