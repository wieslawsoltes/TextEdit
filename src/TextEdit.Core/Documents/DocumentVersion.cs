using System;

namespace TextEdit.Core.Documents;

/// <summary>
/// Represents a monotonically increasing document version.
/// </summary>
public readonly record struct DocumentVersion(int Sequence, DateTime Timestamp)
{
    /// <summary>
    /// Creates the initial document version.
    /// </summary>
    public static DocumentVersion CreateInitial(DateTime? timestamp = null)
        => new(0, Normalize(timestamp));

    /// <summary>
    /// Produces the next version in sequence.
    /// </summary>
    public DocumentVersion Next(DateTime? timestamp = null)
        => new(Sequence + 1, Normalize(timestamp));

    /// <inheritdoc/>
    public override string ToString() => $"{Sequence}@{Timestamp:O}";

    private static DateTime Normalize(DateTime? timestamp)
        => DateTime.SpecifyKind(timestamp ?? DateTime.UtcNow, DateTimeKind.Utc);
}
