namespace TextEdit.Core.Documents;

/// <summary>
/// Describes the type of change applied to a document.
/// </summary>
public enum DocumentChangeKind
{
    Insert,
    Delete,
    Undo,
    Redo,
}
