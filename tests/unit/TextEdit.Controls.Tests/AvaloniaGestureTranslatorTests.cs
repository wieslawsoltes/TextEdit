using Avalonia.Input;
using TextEdit.Controls.Input;
using TextEdit.Core.Input;

namespace TextEdit.Controls.Tests;

public sealed class AvaloniaGestureTranslatorTests
{
    [Theory]
    [InlineData(Key.A, EditorKey.A)]
    [InlineData(Key.Z, EditorKey.Z)]
    [InlineData(Key.D5, EditorKey.D5)]
    [InlineData(Key.F12, EditorKey.F12)]
    [InlineData(Key.Left, EditorKey.Left)]
    [InlineData(Key.OemMinus, EditorKey.Minus)]
    public void ToEditorKeyMapsExpectedValues(Key platformKey, EditorKey expected)
    {
        var result = AvaloniaGestureTranslator.ToEditorKey(platformKey);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToEditorModifiersAggregatesFlags()
    {
        var modifiers = KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Function;
        var result = AvaloniaGestureTranslator.ToEditorModifiers(modifiers);

        Assert.True(result.HasFlag(EditorKeyModifiers.Control));
        Assert.True(result.HasFlag(EditorKeyModifiers.Shift));
        Assert.True(result.HasFlag(EditorKeyModifiers.Function));
        Assert.False(result.HasFlag(EditorKeyModifiers.Alt));
    }

    [Fact]
    public void ToEditorPhysicalKeyUsesSymbol()
    {
        var result = AvaloniaGestureTranslator.ToEditorPhysicalKey(PhysicalKey.KeyA);
        Assert.Equal("KeyA", result.Code);
    }
}
