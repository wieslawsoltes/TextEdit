using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents a keyboard key press or release.
/// </summary>
public sealed record KeyEditorGesture(
    EditorGestureKind Kind,
    DateTimeOffset Timestamp,
    EditorKey Key,
    EditorPhysicalKey PhysicalKey,
    EditorKeyModifiers Modifiers,
    bool IsRepeat,
    bool IsSystemKey)
    : EditorGesture(Kind, Timestamp)
{
    /// <summary>
    /// Gets a value indicating whether the gesture corresponds to a key-down action.
    /// </summary>
    public bool IsKeyDown => Kind == EditorGestureKind.KeyDown;

    /// <summary>
    /// Gets a value indicating whether the gesture corresponds to a key-up action.
    /// </summary>
    public bool IsKeyUp => Kind == EditorGestureKind.KeyUp;
}
