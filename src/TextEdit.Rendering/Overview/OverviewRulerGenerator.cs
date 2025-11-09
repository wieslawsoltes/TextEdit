using System;
using System.Collections.Generic;
using TextEdit.Rendering.Graph;

namespace TextEdit.Rendering.Overview;

/// <summary>
/// Renders an overview ruler with normalized markers.
/// </summary>
public sealed class OverviewRulerGenerator
{
    private readonly IReadOnlyList<OverviewMarker> _markers;
    private readonly int _lineCount;
    private readonly Func<OverviewMarker.MarkerKind, object> _penFactory;

    public OverviewRulerGenerator(
        IReadOnlyList<OverviewMarker> markers,
        int lineCount,
        Func<OverviewMarker.MarkerKind, object> penFactory)
    {
        _markers = markers ?? throw new ArgumentNullException(nameof(markers));
        _lineCount = lineCount;
        _penFactory = penFactory ?? throw new ArgumentNullException(nameof(penFactory));
    }

    public void AddToGraph(RenderGraphBuilder builder, double x, double y, double height)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (_lineCount <= 0 || height <= 0)
        {
            return;
        }

        foreach (var marker in _markers)
        {
            var ratio = Math.Clamp((double)marker.LineIndex / Math.Max(1, _lineCount - 1), 0, 1);
            var markerY = y + (ratio * height);
            var pen = _penFactory(marker.Kind);
            builder.AddOperation(RenderLayerKind.Diagnostics, marker.LineIndex, (ctx, state) =>
            {
                ctx.DrawLine(x, markerY, x, markerY + marker.Thickness, pen);
                state.Set("overview.lastMarker", marker.LineIndex);
            });
        }
    }
}
