using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TextEdit.Controls.Input;
using TextEdit.Core.Input;

namespace TextEdit.Controls;

/// <summary>
/// Avalonia control surface for the TextEdit editor, translating platform input into editor gestures.
/// </summary>
public class CodeEditorControl : Control
{
    /// <summary>
    /// Defines the <see cref="GestureSink"/> property.
    /// </summary>
    public static readonly DirectProperty<CodeEditorControl, IEditorGestureSink?> GestureSinkProperty =
        AvaloniaProperty.RegisterDirect<CodeEditorControl, IEditorGestureSink?>(
            nameof(GestureSink),
            control => control._gestureSink,
            (control, value) => control._gestureSink = value);

    private IEditorGestureSink? _gestureSink;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeEditorControl"/> class.
    /// </summary>
    public CodeEditorControl()
    {
        Focusable = true;
        IsTabStop = true;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.Ibeam);
    }

    /// <summary>
    /// Raised when a gesture is produced by this control.
    /// </summary>
    public event EventHandler<EditorGestureEventArgs>? GestureReceived;

    /// <summary>
    /// Gets or sets the gesture sink that receives translated input events.
    /// </summary>
    public IEditorGestureSink? GestureSink
    {
        get => _gestureSink;
        set => SetAndRaise(GestureSinkProperty, ref _gestureSink, value);
    }

    /// <summary>
    /// Dispatches a gesture to the sink and the <see cref="GestureReceived"/> event.
    /// </summary>
    protected virtual void DispatchGesture(EditorGesture gesture)
    {
        GestureSink?.HandleGesture(gesture);
        GestureReceived?.Invoke(this, new EditorGestureEventArgs(gesture));
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnKeyDown(e);
        if (e.Handled)
        {
            return;
        }

        var gesture = new KeyEditorGesture(
            EditorGestureKind.KeyDown,
            DateTimeOffset.UtcNow,
            AvaloniaGestureTranslator.ToEditorKey(e.Key),
            AvaloniaGestureTranslator.ToEditorPhysicalKey(e.PhysicalKey),
            AvaloniaGestureTranslator.ToEditorModifiers(e.KeyModifiers),
            false,
            false);

        DispatchGesture(gesture);
    }

    /// <inheritdoc/>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnKeyUp(e);
        if (e.Handled)
        {
            return;
        }

        var gesture = new KeyEditorGesture(
            EditorGestureKind.KeyUp,
            DateTimeOffset.UtcNow,
            AvaloniaGestureTranslator.ToEditorKey(e.Key),
            AvaloniaGestureTranslator.ToEditorPhysicalKey(e.PhysicalKey),
            AvaloniaGestureTranslator.ToEditorModifiers(e.KeyModifiers),
            false,
            false);

        DispatchGesture(gesture);
    }

    /// <inheritdoc/>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnTextInput(e);
        if (e.Handled)
        {
            return;
        }

        var gesture = new TextInputGesture(
            DateTimeOffset.UtcNow,
            e.Text ?? string.Empty,
            null,
            EditorKeyModifiers.None);

        DispatchGesture(gesture);
    }

    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerEntered(e);
        DispatchPointerGesture(e, EditorGestureKind.PointerEntered);
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerExited(e);
        DispatchPointerGesture(e, EditorGestureKind.PointerExited);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerPressed(e);
        DispatchPointerGesture(e, EditorGestureKind.PointerPressed);
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerReleased(e);
        DispatchPointerGesture(e, EditorGestureKind.PointerReleased);
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerMoved(e);
        DispatchPointerGesture(e, EditorGestureKind.PointerMoved);
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerWheelChanged(e);
        if (e.Handled)
        {
            return;
        }

        var modifiers = AvaloniaGestureTranslator.ToEditorModifiers(e.KeyModifiers);
        var pointerType = AvaloniaGestureTranslator.ToEditorPointerType(e.Pointer.Type, e.GetCurrentPoint(this).Properties);
        var buttons = AvaloniaGestureTranslator.ToEditorButtons(e.GetCurrentPoint(this).Properties);

        var gesture = new PointerWheelGesture(
            DateTimeOffset.UtcNow,
            e.Pointer.Id,
            pointerType,
            buttons,
            modifiers,
            e.Delta.X,
            e.Delta.Y,
            EditorPointerWheelDeltaMode.Pixel,
            false);

        DispatchGesture(gesture);
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerCaptureLost(e);
        DispatchPointerCaptureLost(e);
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        var gesture = new FocusChangedGesture(EditorGestureKind.FocusGained, DateTimeOffset.UtcNow, IsFocused);
        DispatchGesture(gesture);
    }

    /// <inheritdoc/>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        var gesture = new FocusChangedGesture(EditorGestureKind.FocusLost, DateTimeOffset.UtcNow, IsFocused);
        DispatchGesture(gesture);
    }

    private void DispatchPointerGesture(PointerEventArgs e, EditorGestureKind kind)
    {
        if (e.Handled)
        {
            return;
        }

        var point = e.GetCurrentPoint(this);
        var gesture = new PointerEditorGesture(
            kind,
            DateTimeOffset.UtcNow,
            e.Pointer.Id,
            AvaloniaGestureTranslator.ToEditorPointerType(e.Pointer.Type, point.Properties),
            AvaloniaGestureTranslator.ToEditorButtons(point.Properties),
            AvaloniaGestureTranslator.ToChangedButtons(point.Properties),
            AvaloniaGestureTranslator.ToEditorPointerPoint(point),
            AvaloniaGestureTranslator.ToEditorModifiers(e.KeyModifiers),
            AvaloniaGestureTranslator.ToEditorPointerUpdateKind(point.Properties.PointerUpdateKind),
            e.Pointer.IsPrimary);

        DispatchGesture(gesture);
    }

    private void DispatchPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        var pointerType = e.Pointer is null
            ? EditorPointerType.Unknown
            : AvaloniaGestureTranslator.ToEditorPointerType(e.Pointer.Type);

        var gesture = new PointerEditorGesture(
            EditorGestureKind.PointerCaptureLost,
            DateTimeOffset.UtcNow,
            e.Pointer?.Id ?? 0,
            pointerType,
            EditorPointerButtons.None,
            EditorPointerButtons.None,
            new EditorPointerPoint(new EditorPoint(0, 0), 0f, 0f, 0f, 0f),
            EditorKeyModifiers.None,
            EditorPointerUpdateKind.Other,
            e.Pointer?.IsPrimary ?? false);

        DispatchGesture(gesture);
    }
}
