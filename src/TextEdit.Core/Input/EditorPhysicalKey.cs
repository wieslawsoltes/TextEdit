using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Encapsulates the physical key location reported by the platform keyboard layout.
/// </summary>
public readonly record struct EditorPhysicalKey(string Code)
{
    /// <summary>
    /// Gets a value indicating whether the key is populated.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Code);

    /// <summary>
    /// Creates an <see cref="EditorPhysicalKey"/> from a platform supplied identifier,
    /// normalising <see langword="null"/> to an empty key.
    /// </summary>
    public static EditorPhysicalKey FromPlatform(string? code)
        => new(code ?? string.Empty);

    /// <inheritdoc/>
    public override string ToString() => Code ?? string.Empty;
}
