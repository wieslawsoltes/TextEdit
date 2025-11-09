using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents modifier keys that can accompany keyboard and pointer gestures.
/// </summary>
[Flags]
public enum EditorKeyModifiers
{
    /// <summary>
    /// No modifiers pressed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Shift modifier.
    /// </summary>
    Shift = 1 << 0,

    /// <summary>
    /// Control modifier (Ctrl on Windows/Linux, Command on macOS when mapped as control).
    /// </summary>
    Control = 1 << 1,

    /// <summary>
    /// Alt/Option modifier.
    /// </summary>
    Alt = 1 << 2,

    /// <summary>
    /// Platform meta modifier (Command on macOS, Windows key on Windows).
    /// </summary>
    Meta = 1 << 3,

    /// <summary>
    /// Platform super modifier (distinct from Meta when provided by the platform).
    /// </summary>
    Super = 1 << 4,

    /// <summary>
    /// Function modifier (Fn on modern keyboards).
    /// </summary>
    Function = 1 << 5,

    /// <summary>
    /// Indicates caps lock is active.
    /// </summary>
    CapsLock = 1 << 6,

    /// <summary>
    /// Indicates num lock is active.
    /// </summary>
    NumLock = 1 << 7,
}
