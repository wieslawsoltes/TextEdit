namespace TextEdit.Core.Input;

/// <summary>
/// Specifies the action indicated by a pointer update.
/// </summary>
public enum EditorPointerUpdateKind
{
    Other = 0,
    LeftButtonPressed,
    LeftButtonReleased,
    RightButtonPressed,
    RightButtonReleased,
    MiddleButtonPressed,
    MiddleButtonReleased,
    XButton1Pressed,
    XButton1Released,
    XButton2Pressed,
    XButton2Released,
    PenBarrelButtonPressed,
    PenBarrelButtonReleased,
    PenEraserPressed,
    PenEraserReleased,
}
