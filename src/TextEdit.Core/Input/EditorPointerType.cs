namespace TextEdit.Core.Input;

/// <summary>
/// Identifies the source device for a pointer gesture.
/// </summary>
public enum EditorPointerType
{
    Unknown = 0,
    Mouse,
    Touch,
    Pen,
    Eraser,
    Stylus,
    Touchpad,
}
