using System.Collections.ObjectModel;

namespace TextEdit.Extensions;

/// <summary>
/// Stub manifest representation used to validate extension project wiring.
/// </summary>
public sealed class ExtensionManifest
{
    /// <summary>
    /// Gets the manifest display name.
    /// </summary>
    public string Name { get; init; } = "Placeholder Extension";

    /// <summary>
    /// Gets the supported capability identifiers.
    /// </summary>
    public ReadOnlyCollection<string> Capabilities { get; init; } =
        new(new List<string> { "core" });
}
