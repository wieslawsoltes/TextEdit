using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents IME composition lifecycle events.
/// </summary>
public sealed record CompositionEditorGesture(
    EditorGestureKind Kind,
    DateTimeOffset Timestamp,
    string? Text,
    string? PreeditText,
    EditorKeyModifiers Modifiers)
    : EditorGesture(Kind, Timestamp);
