using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents text input produced by the platform text services.
/// </summary>
public sealed record TextInputGesture(
    DateTimeOffset Timestamp,
    string Text,
    string? HighlightedText,
    EditorKeyModifiers Modifiers)
    : EditorGesture(EditorGestureKind.TextInput, Timestamp)
{
    /// <summary>
    /// Gets a value indicating whether the gesture contains non-empty text.
    /// </summary>
    public bool HasText => !string.IsNullOrEmpty(Text);
}
