using System;

namespace TextEdit.Core.Documents;

/// <summary>
/// Represents a single editorial change applied to a document.
/// </summary>
public readonly record struct DocumentChange(
    DocumentChangeKind Kind,
    int Position,
    int Length,
    string? Text)
{
    /// <summary>
    /// Creates an insertion change.
    /// </summary>
    public static DocumentChange Insert(int position, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new DocumentChange(DocumentChangeKind.Insert, position, text.Length, text);
    }

    /// <summary>
    /// Creates a deletion change.
    /// </summary>
    public static DocumentChange Delete(int position, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new DocumentChange(DocumentChangeKind.Delete, position, text.Length, text);
    }

    /// <summary>
    /// Creates an undo marker.
    /// </summary>
    public static DocumentChange Undo() => new(DocumentChangeKind.Undo, -1, 0, null);

    /// <summary>
    /// Creates a redo marker.
    /// </summary>
    public static DocumentChange Redo() => new(DocumentChangeKind.Redo, -1, 0, null);
}
