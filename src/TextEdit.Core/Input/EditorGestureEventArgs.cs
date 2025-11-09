using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Event arguments carrying a single editor gesture.
/// </summary>
public sealed class EditorGestureEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditorGestureEventArgs"/> class.
    /// </summary>
    public EditorGestureEventArgs(EditorGesture gesture)
    {
        Gesture = gesture;
    }

    /// <summary>
    /// Gets the gesture carried by the event.
    /// </summary>
    public EditorGesture Gesture { get; }
}
