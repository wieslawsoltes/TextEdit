using System;

namespace TextEdit.Rendering.Graph;

/// <summary>
/// Represents an immutable rendering graph ready for execution.
/// </summary>
public sealed class RenderGraph
{
    private readonly RenderOperation[] _operations;
    private readonly RenderState _state;

    internal RenderGraph(RenderOperation[] operations, RenderState state)
    {
        _operations = operations ?? throw new ArgumentNullException(nameof(operations));
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    /// Executes the render graph against the provided context.
    /// </summary>
    public void Execute(IRenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        foreach (var operation in _operations)
        {
            operation.Execute(context, _state);
        }
    }

    /// <summary>
    /// Gets the frozen state associated with this graph.
    /// </summary>
    public RenderState State => _state;
}
