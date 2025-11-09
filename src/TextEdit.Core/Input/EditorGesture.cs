using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Base type for gestures emitted by platform input adapters.
/// </summary>
public abstract record EditorGesture(EditorGestureKind Kind, DateTimeOffset Timestamp)
{
    /// <summary>
    /// Gets the gesture kind.
    /// </summary>
    public EditorGestureKind Kind { get; } = Kind;

    /// <summary>
    /// Gets the timestamp when the gesture was produced.
    /// </summary>
    public DateTimeOffset Timestamp { get; } = Timestamp;
}
