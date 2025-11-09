using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents an access key invocation gesture (for accessibility / mnemonics).
/// </summary>
public sealed record AccessKeyEditorGesture(
    DateTimeOffset Timestamp,
    string AccessKey,
    EditorKeyModifiers Modifiers)
    : EditorGesture(EditorGestureKind.AccessKeyInvoked, Timestamp)
{
    /// <summary>
    /// Gets a value indicating whether the access key is populated.
    /// </summary>
    public bool HasAccessKey => !string.IsNullOrWhiteSpace(AccessKey);
}
