using System;
using System.Collections.Generic;
using TextEdit.Rendering.Graph;

namespace TextEdit.Rendering.Minimap;

/// <summary>
/// Generates minimap render operations from a document source.
/// </summary>
public sealed class MinimapGenerator
{
    private readonly IMinimapSource _source;
    private readonly Func<int, object> _lineBrushFactory;

    public MinimapGenerator(IMinimapSource source, Func<int, object> lineBrushFactory)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _lineBrushFactory = lineBrushFactory ?? throw new ArgumentNullException(nameof(lineBrushFactory));
    }

    /// <summary>
    /// Adds minimap render operations to the supplied builder.
    /// </summary>
    public void AddToGraph(RenderGraphBuilder builder, double x, double y, double lineHeight, double width)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var operations = new List<RenderOperation>();

        for (var i = 0; i < _source.LineCount; i++)
        {
            var top = y + (i * lineHeight);
            var brush = _lineBrushFactory(i);
            var lineIndex = i;
            operations.Add(new RenderOperation(
                RenderLayerKind.Background,
                -100,
                (ctx, state) =>
                {
                    ctx.DrawRectangle(x, top, width, lineHeight, brush);
                    state.Set("minimap.lastLine", lineIndex);
                }));
        }

        foreach (var op in operations)
        {
            builder.AddOperation(op);
        }
    }
}
