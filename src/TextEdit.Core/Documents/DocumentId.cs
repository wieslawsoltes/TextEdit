using System;

namespace TextEdit.Core.Documents;

/// <summary>
/// Represents a stable identifier for a document within the editor graph.
/// </summary>
public readonly record struct DocumentId(Guid Value)
{
    /// <summary>
    /// Creates a new document identifier.
    /// </summary>
    public static DocumentId CreateNew() => new(Guid.NewGuid());

    /// <summary>
    /// Returns the string representation of the identifier.
    /// </summary>
    /// <returns>A 32-character, hyphen-less GUID string.</returns>
    public override string ToString() => Value.ToString("N");

    /// <summary>
    /// Returns the underlying <see cref="Guid"/>.
    /// </summary>
    public Guid ToGuid() => Value;
}
