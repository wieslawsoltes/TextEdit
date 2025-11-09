using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents a pointer action such as press, move, or release.
/// </summary>
public sealed record PointerEditorGesture(
    EditorGestureKind Kind,
    DateTimeOffset Timestamp,
    long PointerId,
    EditorPointerType PointerType,
    EditorPointerButtons Buttons,
    EditorPointerButtons ChangedButtons,
    EditorPointerPoint Point,
    EditorKeyModifiers Modifiers,
    EditorPointerUpdateKind UpdateKind,
    bool IsPrimary)
    : EditorGesture(Kind, Timestamp);
