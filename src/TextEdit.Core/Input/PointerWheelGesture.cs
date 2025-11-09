using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents a pointer wheel delta (mouse wheel, touchpad scroll, etc.).
/// </summary>
public sealed record PointerWheelGesture(
    DateTimeOffset Timestamp,
    long PointerId,
    EditorPointerType PointerType,
    EditorPointerButtons Buttons,
    EditorKeyModifiers Modifiers,
    double DeltaX,
    double DeltaY,
    EditorPointerWheelDeltaMode DeltaMode,
    bool IsPrecise)
    : EditorGesture(EditorGestureKind.PointerWheelChanged, Timestamp);
