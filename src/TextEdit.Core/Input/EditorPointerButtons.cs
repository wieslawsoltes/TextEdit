using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Describes pointer device buttons that are currently pressed.
/// </summary>
[Flags]
public enum EditorPointerButtons
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Middle = 1 << 2,
    XButton1 = 1 << 3,
    XButton2 = 1 << 4,
    PenBarrel = 1 << 5,
    PenEraser = 1 << 6,
}
