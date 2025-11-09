namespace TextEdit.Core.Caret;

/// <summary>
/// Represents the logical affinity of a caret or selection, used for bidirectional text scenarios.
/// </summary>
public enum LogicalDirection
{
    /// <summary>
    /// Indicates forward affinity (left-to-right in LTR scripts).
    /// </summary>
    Forward,

    /// <summary>
    /// Indicates backward affinity (right-to-left in LTR scripts).
    /// </summary>
    Backward,
}
