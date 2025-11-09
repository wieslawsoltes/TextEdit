using System.Threading;
using System.Threading.Tasks;

namespace TextEdit.Rendering.Layout;

/// <summary>
/// Provides asynchronous line layout measurements.
/// </summary>
public interface ILineLayoutProvider
{
    /// <summary>
    /// Gets the total number of lines that can be measured.
    /// </summary>
    int LineCount { get; }

    /// <summary>
    /// Measures the layout of the specified line.
    /// </summary>
    ValueTask<LineLayoutInfo> MeasureAsync(int lineIndex, CancellationToken cancellationToken);
}
