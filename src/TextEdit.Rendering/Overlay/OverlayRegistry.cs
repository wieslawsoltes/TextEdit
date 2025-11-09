using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TextEdit.Rendering.Graph;

namespace TextEdit.Rendering.Overlay;

/// <summary>
/// Manages overlay/adornment registrations and emits render operations.
/// </summary>
public sealed class OverlayRegistry
{
    private readonly ConcurrentDictionary<string, OverlayInfo> _overlays = new();

    public bool Register(string id, OverlayInfo overlay) => _overlays.TryAdd(id, overlay);

    public bool Update(string id, OverlayInfo overlay)
    {
        ArgumentNullException.ThrowIfNull(overlay);
        _overlays[id] = overlay;
        return true;
    }

    public bool Unregister(string id) => _overlays.TryRemove(id, out _);

    public void Contribute(RenderGraphBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        foreach (var entry in _overlays.Values)
        {
            var overlay = entry;
            builder.AddOperation(overlay.Layer, overlay.ZIndex, overlay.Render);
        }
    }

    public IReadOnlyCollection<string> SnapshotIds() => _overlays.Keys.ToArray();
}
