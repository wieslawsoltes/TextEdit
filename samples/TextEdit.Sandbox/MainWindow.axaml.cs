using System;
using Avalonia.Controls;
using TextEdit.Core;
using TextEdit.Core.Documents;
using TextEdit.Core.Input;
using TextEdit.Extensions;
using TextEdit.Rendering;

namespace TextEdit.Sandbox;

public partial class MainWindow : Window
{
    private readonly EditorGestureDispatcher _gestureDispatcher = new();
    private GestureDisplaySink? _gestureSink;

    public MainWindow()
    {
        InitializeComponent();
        var manifest = new ExtensionManifest();
        VersionTextBlock.Text =
            $"Kernel {RenderingPipeline.KernelVersion} | Default Extension: {manifest.Name}";

        var kernel = new EditorKernel();
        var document = kernel.CreateDocument("Welcome to the document subsystem.", uri: new Uri("sandbox://welcome"));
        document.Insert(document.Length, " This text comes from the core document prototype.");
        var snapshot = document.CreateSnapshot();

        DocumentInfoTextBlock.Text = $"Document {snapshot.Id} v{snapshot.Version.Sequence}";
        BufferTextBlock.Text = snapshot.GetText();

        _gestureSink = new GestureDisplaySink(GestureStatusTextBlock);
        _gestureDispatcher.Subscribe(_gestureSink);
        EditorControl.GestureSink = _gestureDispatcher;
    }

    private sealed class GestureDisplaySink : IEditorGestureSink
    {
        private readonly TextBlock _target;

        public GestureDisplaySink(TextBlock target)
        {
            _target = target;
        }

        public void HandleGesture(EditorGesture gesture)
        {
            _target.Text = gesture switch
            {
                KeyEditorGesture key => $"{key.Kind}: {key.Key} ({key.Modifiers})",
                TextInputGesture text => $"TextInput: \"{text.Text}\" (mods: {text.Modifiers})",
                PointerEditorGesture pointer => $"{pointer.Kind}: {pointer.PointerType} @ {pointer.Point.Position} buttons {pointer.Buttons}",
                PointerWheelGesture wheel => $"Wheel: Δ({wheel.DeltaX:0.##}, {wheel.DeltaY:0.##}) mods {wheel.Modifiers}",
                FocusChangedGesture focus => $"Focus {(focus.IsFocused ? "gained" : "lost")}",
                AccessKeyEditorGesture access => $"AccessKey: {access.AccessKey}",
                CompositionEditorGesture composition => $"{composition.Kind}: \"{composition.PreeditText ?? composition.Text}\"",
                _ => $"{gesture.Kind} @ {gesture.Timestamp:HH:mm:ss.fff}",
            };
        }
    }
}
