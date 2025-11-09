using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents focus changes for the editor control.
/// </summary>
public sealed record FocusChangedGesture(
    EditorGestureKind Kind,
    DateTimeOffset Timestamp,
    bool IsFocused)
    : EditorGesture(Kind, Timestamp);
