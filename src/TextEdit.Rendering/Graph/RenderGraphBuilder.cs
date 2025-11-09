using System;
using System.Collections.Generic;
using System.Linq;

namespace TextEdit.Rendering.Graph;

/// <summary>
/// Builds a render graph composed of ordered operations across logical layers.
/// </summary>
public sealed class RenderGraphBuilder
{
    private readonly List<RenderOperation> _operations = new();
    private readonly RenderState _state = new();

    /// <summary>
    /// Adds a rendering operation to the graph.
    /// </summary>
    public RenderGraphBuilder AddOperation(RenderOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        _operations.Add(new RenderOperation(operation.Layer, operation.ZIndex, operation.Execute));
        return this;
    }

    /// <summary>
    /// Adds a rendering operation using the provided delegate.
    /// </summary>
    public RenderGraphBuilder AddOperation(
        RenderLayerKind layer,
        double zIndex,
        Action<IRenderContext, RenderState> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _operations.Add(new RenderOperation(layer, zIndex, (ctx, state) => action(ctx, state)));
        return this;
    }

    /// <summary>
    /// Builds the render graph ready for execution.
    /// </summary>
    public RenderGraph Build()
    {
        var frozenState = _state.Clone();
        var operations = _operations
            .OrderBy(static op => op.Layer)
            .ThenBy(static op => op.ZIndex)
            .Select(op => new RenderOperation(op.Layer, op.ZIndex, op.Execute))
            .ToArray();

        return new RenderGraph(operations, frozenState);
    }
}
