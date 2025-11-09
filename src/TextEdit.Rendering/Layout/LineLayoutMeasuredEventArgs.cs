using System;

namespace TextEdit.Rendering.Layout;

/// <summary>
/// Event arguments emitted when a line layout measurement completes.
/// </summary>
public sealed class LineLayoutMeasuredEventArgs : EventArgs
{
    public LineLayoutMeasuredEventArgs(LineLayoutInfo info)
    {
        Info = info ?? throw new ArgumentNullException(nameof(info));
    }

    /// <summary>
    /// Gets the measured line information.
    /// </summary>
    public LineLayoutInfo Info { get; }
}
