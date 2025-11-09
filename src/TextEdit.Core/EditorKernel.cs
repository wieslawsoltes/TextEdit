using System.Diagnostics.CodeAnalysis;
using TextEdit.Core.Buffers;
using TextEdit.Core.Documents;

namespace TextEdit.Core;

/// <summary>
/// Entry point abstraction for coordinating core editor services.
/// Populated with scaffolding members in subsequent milestones.
/// </summary>
public sealed class EditorKernel
{
    /// <summary>
    /// Version placeholder for downstream components during scaffolding.
    /// </summary>
    public const string Version = "0.1.0-dev";

    /// <summary>
    /// Creates a new <see cref="PieceTreeTextBuffer"/> instance.
    /// </summary>
    [SuppressMessage("Performance", "CA1822", Justification = "Kernel will expose instance state in subsequent milestones.")]
    public PieceTreeTextBuffer CreateTextBuffer(string? initialText = null)
        => new(initialText);

    /// <summary>
    /// Creates a new <see cref="Document"/> instance.
    /// </summary>
    [SuppressMessage("Performance", "CA1822", Justification = "Kernel will expose instance state in subsequent milestones.")]
    public Document CreateDocument(string? initialText = null, Uri? uri = null, DocumentId? id = null)
        => new(initialText, uri, id);
}
