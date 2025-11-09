namespace TextEdit.Rendering.Minimap;

/// <summary>
/// Provides lightweight document data for minimap rendering.
/// </summary>
public interface IMinimapSource
{
    int LineCount { get; }

    string GetLinePreview(int lineIndex);
}
