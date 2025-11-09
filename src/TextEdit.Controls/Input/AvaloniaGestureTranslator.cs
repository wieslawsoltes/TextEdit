using System;
using Avalonia.Input;
using TextEdit.Core.Input;

namespace TextEdit.Controls.Input;

internal static class AvaloniaGestureTranslator
{
    public static EditorKeyModifiers ToEditorModifiers(KeyModifiers modifiers)
    {
        var result = EditorKeyModifiers.None;

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            result |= EditorKeyModifiers.Shift;
        }

        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            result |= EditorKeyModifiers.Control;
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            result |= EditorKeyModifiers.Alt;
        }

        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            result |= EditorKeyModifiers.Meta;
        }

        return result;
    }

    public static EditorPointerType ToEditorPointerType(PointerType pointerType)
    {
        return pointerType switch
        {
            PointerType.Mouse => EditorPointerType.Mouse,
            PointerType.Touch => EditorPointerType.Touch,
            PointerType.Pen => EditorPointerType.Pen,
            _ => EditorPointerType.Unknown,
        };
    }

    public static EditorKey ToEditorKey(Key key) => key switch
    {
        Key.Back => EditorKey.Backspace,
        Key.Tab => EditorKey.Tab,
        Key.Enter or Key.Return => EditorKey.Enter,
        Key.Escape => EditorKey.Escape,
        Key.Space => EditorKey.Space,

        Key.Left => EditorKey.Left,
        Key.Up => EditorKey.Up,
        Key.Right => EditorKey.Right,
        Key.Down => EditorKey.Down,
        Key.Home => EditorKey.Home,
        Key.End => EditorKey.End,
        Key.PageUp => EditorKey.PageUp,
        Key.PageDown => EditorKey.PageDown,
        Key.Insert => EditorKey.Insert,
        Key.Delete => EditorKey.Delete,

        Key.CapsLock => EditorKey.CapsLock,
        Key.NumLock => EditorKey.NumLock,
        Key.Scroll => EditorKey.ScrollLock,
        Key.PrintScreen => EditorKey.PrintScreen,
        Key.Pause => EditorKey.Pause,

        Key.Apps => EditorKey.ContextMenu,

        Key.D0 => EditorKey.D0,
        Key.D1 => EditorKey.D1,
        Key.D2 => EditorKey.D2,
        Key.D3 => EditorKey.D3,
        Key.D4 => EditorKey.D4,
        Key.D5 => EditorKey.D5,
        Key.D6 => EditorKey.D6,
        Key.D7 => EditorKey.D7,
        Key.D8 => EditorKey.D8,
        Key.D9 => EditorKey.D9,

        Key.A => EditorKey.A,
        Key.B => EditorKey.B,
        Key.C => EditorKey.C,
        Key.D => EditorKey.D,
        Key.E => EditorKey.E,
        Key.F => EditorKey.F,
        Key.G => EditorKey.G,
        Key.H => EditorKey.H,
        Key.I => EditorKey.I,
        Key.J => EditorKey.J,
        Key.K => EditorKey.K,
        Key.L => EditorKey.L,
        Key.M => EditorKey.M,
        Key.N => EditorKey.N,
        Key.O => EditorKey.O,
        Key.P => EditorKey.P,
        Key.Q => EditorKey.Q,
        Key.R => EditorKey.R,
        Key.S => EditorKey.S,
        Key.T => EditorKey.T,
        Key.U => EditorKey.U,
        Key.V => EditorKey.V,
        Key.W => EditorKey.W,
        Key.X => EditorKey.X,
        Key.Y => EditorKey.Y,
        Key.Z => EditorKey.Z,

        Key.F1 => EditorKey.F1,
        Key.F2 => EditorKey.F2,
        Key.F3 => EditorKey.F3,
        Key.F4 => EditorKey.F4,
        Key.F5 => EditorKey.F5,
        Key.F6 => EditorKey.F6,
        Key.F7 => EditorKey.F7,
        Key.F8 => EditorKey.F8,
        Key.F9 => EditorKey.F9,
        Key.F10 => EditorKey.F10,
        Key.F11 => EditorKey.F11,
        Key.F12 => EditorKey.F12,
        Key.F13 => EditorKey.F13,
        Key.F14 => EditorKey.F14,
        Key.F15 => EditorKey.F15,
        Key.F16 => EditorKey.F16,
        Key.F17 => EditorKey.F17,
        Key.F18 => EditorKey.F18,
        Key.F19 => EditorKey.F19,
        Key.F20 => EditorKey.F20,
        Key.F21 => EditorKey.F21,
        Key.F22 => EditorKey.F22,
        Key.F23 => EditorKey.F23,
        Key.F24 => EditorKey.F24,

        Key.OemMinus => EditorKey.Minus,
        Key.Subtract => EditorKey.NumPadSubtract,
        Key.OemPlus => EditorKey.Equals,
        Key.Add => EditorKey.NumPadAdd,
        Key.OemComma => EditorKey.Comma,
        Key.OemPeriod => EditorKey.Period,
        Key.Oem1 => EditorKey.Semicolon,
        Key.Oem2 => EditorKey.Slash,
        Key.Oem3 => EditorKey.GraveAccent,
        Key.Oem4 => EditorKey.LeftBracket,
        Key.Oem5 => EditorKey.Backslash,
        Key.Oem6 => EditorKey.RightBracket,
        Key.Oem7 => EditorKey.Apostrophe,

        Key.NumPad0 => EditorKey.NumPad0,
        Key.NumPad1 => EditorKey.NumPad1,
        Key.NumPad2 => EditorKey.NumPad2,
        Key.NumPad3 => EditorKey.NumPad3,
        Key.NumPad4 => EditorKey.NumPad4,
        Key.NumPad5 => EditorKey.NumPad5,
        Key.NumPad6 => EditorKey.NumPad6,
        Key.NumPad7 => EditorKey.NumPad7,
        Key.NumPad8 => EditorKey.NumPad8,
        Key.NumPad9 => EditorKey.NumPad9,
        Key.Decimal => EditorKey.NumPadDecimal,
        Key.Divide => EditorKey.NumPadDivide,
        Key.Multiply => EditorKey.NumPadMultiply,
        Key.Separator => EditorKey.NumPadEnter,

        Key.VolumeDown => EditorKey.VolumeDown,
        Key.VolumeUp => EditorKey.VolumeUp,
        Key.VolumeMute => EditorKey.VolumeMute,
        Key.MediaPlayPause => EditorKey.MediaPlayPause,
        Key.MediaStop => EditorKey.MediaStop,
        Key.MediaNextTrack => EditorKey.MediaNextTrack,
        Key.MediaPreviousTrack => EditorKey.MediaPreviousTrack,

        Key.BrowserBack => EditorKey.BrowserBack,
        Key.BrowserForward => EditorKey.BrowserForward,
        Key.BrowserRefresh => EditorKey.BrowserRefresh,
        Key.BrowserStop => EditorKey.BrowserStop,
        Key.BrowserSearch => EditorKey.BrowserSearch,
        Key.BrowserFavorites => EditorKey.BrowserFavorites,
        Key.BrowserHome => EditorKey.BrowserHome,

        Key.LaunchMail => EditorKey.LaunchMail,
        Key.SelectMedia => EditorKey.LaunchMediaSelect,
        Key.LaunchApplication1 => EditorKey.LaunchApplication1,
        Key.LaunchApplication2 => EditorKey.LaunchApplication2,

        _ => EditorKey.Unknown,
    };

    public static EditorPointerType ToEditorPointerType(PointerType pointerType, PointerPointProperties properties)
    {
        return pointerType switch
        {
            PointerType.Mouse => EditorPointerType.Mouse,
            PointerType.Touch => EditorPointerType.Touch,
            PointerType.Pen => properties.IsEraser ? EditorPointerType.Eraser : EditorPointerType.Pen,
            _ => EditorPointerType.Unknown,
        };
    }

    public static EditorPointerButtons ToEditorButtons(PointerPointProperties properties)
    {
        var buttons = EditorPointerButtons.None;

        if (properties.IsLeftButtonPressed)
        {
            buttons |= EditorPointerButtons.Left;
        }

        if (properties.IsRightButtonPressed)
        {
            buttons |= EditorPointerButtons.Right;
        }

        if (properties.IsMiddleButtonPressed)
        {
            buttons |= EditorPointerButtons.Middle;
        }

        if (properties.IsXButton1Pressed)
        {
            buttons |= EditorPointerButtons.XButton1;
        }

        if (properties.IsXButton2Pressed)
        {
            buttons |= EditorPointerButtons.XButton2;
        }

        if (properties.IsBarrelButtonPressed)
        {
            buttons |= EditorPointerButtons.PenBarrel;
        }

        if (properties.IsEraser)
        {
            buttons |= EditorPointerButtons.PenEraser;
        }

        return buttons;
    }

    public static EditorPointerButtons ToChangedButtons(PointerPointProperties properties)
    {
        return properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonPressed or PointerUpdateKind.LeftButtonReleased =>
                EditorPointerButtons.Left,
            PointerUpdateKind.RightButtonPressed or PointerUpdateKind.RightButtonReleased =>
                EditorPointerButtons.Right,
            PointerUpdateKind.MiddleButtonPressed or PointerUpdateKind.MiddleButtonReleased =>
                EditorPointerButtons.Middle,
            PointerUpdateKind.XButton1Pressed or PointerUpdateKind.XButton1Released =>
                EditorPointerButtons.XButton1,
            PointerUpdateKind.XButton2Pressed or PointerUpdateKind.XButton2Released =>
                EditorPointerButtons.XButton2,
            _ => EditorPointerButtons.None,
        };
    }

    public static EditorPointerUpdateKind ToEditorPointerUpdateKind(PointerUpdateKind updateKind)
    {
        return updateKind switch
        {
            PointerUpdateKind.LeftButtonPressed => EditorPointerUpdateKind.LeftButtonPressed,
            PointerUpdateKind.LeftButtonReleased => EditorPointerUpdateKind.LeftButtonReleased,
            PointerUpdateKind.RightButtonPressed => EditorPointerUpdateKind.RightButtonPressed,
            PointerUpdateKind.RightButtonReleased => EditorPointerUpdateKind.RightButtonReleased,
            PointerUpdateKind.MiddleButtonPressed => EditorPointerUpdateKind.MiddleButtonPressed,
            PointerUpdateKind.MiddleButtonReleased => EditorPointerUpdateKind.MiddleButtonReleased,
            PointerUpdateKind.XButton1Pressed => EditorPointerUpdateKind.XButton1Pressed,
            PointerUpdateKind.XButton1Released => EditorPointerUpdateKind.XButton1Released,
            PointerUpdateKind.XButton2Pressed => EditorPointerUpdateKind.XButton2Pressed,
            PointerUpdateKind.XButton2Released => EditorPointerUpdateKind.XButton2Released,
            _ => EditorPointerUpdateKind.Other,
        };
    }

    public static EditorPointerPoint ToEditorPointerPoint(PointerPoint point)
    {
        var properties = point.Properties;
        return new EditorPointerPoint(
            new EditorPoint(point.Position.X, point.Position.Y),
            properties.Pressure,
            properties.Twist,
            properties.XTilt,
            properties.YTilt);
    }

    public static EditorPhysicalKey ToEditorPhysicalKey(PhysicalKey physicalKey)
    {
        return EditorPhysicalKey.FromPlatform(physicalKey == PhysicalKey.None ? null : physicalKey.ToString());
    }
}
