namespace TextEdit.Core.Input;

/// <summary>
/// Receives editor gestures produced by the host platform adapter.
/// </summary>
public interface IEditorGestureSink
{
    /// <summary>
    /// Handles an incoming gesture.
    /// </summary>
    void HandleGesture(EditorGesture gesture);
}
