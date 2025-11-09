namespace TextEdit.Core.Input;

/// <summary>
/// Identifies the semantic type of an editor gesture raised by a platform input adapter.
/// </summary>
public enum EditorGestureKind
{
    /// <summary>
    /// Unknown or unsupported gesture.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A key press event.
    /// </summary>
    KeyDown,

    /// <summary>
    /// A key release event.
    /// </summary>
    KeyUp,

    /// <summary>
    /// Text input generated via character composition.
    /// </summary>
    TextInput,

    /// <summary>
    /// Pointer pressed (mouse down/touch contact/stylus contact).
    /// </summary>
    PointerPressed,

    /// <summary>
    /// Pointer released (mouse up/touch release/stylus lift).
    /// </summary>
    PointerReleased,

    /// <summary>
    /// Pointer moved while in range or in contact.
    /// </summary>
    PointerMoved,

    /// <summary>
    /// Pointer entered the control bounds.
    /// </summary>
    PointerEntered,

    /// <summary>
    /// Pointer exited the control bounds.
    /// </summary>
    PointerExited,

    /// <summary>
    /// Pointer capture lost due to focus change or other platform reason.
    /// </summary>
    PointerCaptureLost,

    /// <summary>
    /// Pointer wheel/scroll gesture.
    /// </summary>
    PointerWheelChanged,

    /// <summary>
    /// Input method editor composition started.
    /// </summary>
    CompositionStarted,

    /// <summary>
    /// Input method editor composition updated.
    /// </summary>
    CompositionUpdated,

    /// <summary>
    /// Input method editor composition completed.
    /// </summary>
    CompositionCompleted,

    /// <summary>
    /// Control gained logical focus.
    /// </summary>
    FocusGained,

    /// <summary>
    /// Control lost logical focus.
    /// </summary>
    FocusLost,

    /// <summary>
    /// Access key (mnemonic) invoked for accessibility scenarios.
    /// </summary>
    AccessKeyInvoked,
}
