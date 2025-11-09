using System.Collections.Generic;

namespace TextEdit.Rendering.Graph;

/// <summary>
/// Stores shared state between render operations.
/// </summary>
public sealed class RenderState
{
    private readonly Dictionary<string, object?> _values = new();

    public void Set<T>(string key, T value) => _values[key] = value;

    public bool TryGet<T>(string key, out T? value)
    {
        if (_values.TryGetValue(key, out var stored) && stored is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    internal RenderState Clone()
    {
        var clone = new RenderState();
        foreach (var pair in _values)
        {
            clone._values[pair.Key] = pair.Value;
        }

        return clone;
    }
}
