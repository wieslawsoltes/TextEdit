using System;
using System.Collections.Generic;

namespace TextEdit.Core.Input;

/// <summary>
/// Multiplexes gestures to a set of observers.
/// </summary>
public sealed class EditorGestureDispatcher : IEditorGestureSink
{
    private readonly object _gate = new();
    private readonly List<IEditorGestureSink> _observers = new();

    /// <summary>
    /// Adds a downstream observer.
    /// </summary>
    public void Subscribe(IEditorGestureSink observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        lock (_gate)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
    }

    /// <summary>
    /// Removes a downstream observer.
    /// </summary>
    public void Unsubscribe(IEditorGestureSink observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        lock (_gate)
        {
            _observers.Remove(observer);
        }
    }

    /// <inheritdoc />
    public void HandleGesture(EditorGesture gesture)
    {
        ArgumentNullException.ThrowIfNull(gesture);

        IEditorGestureSink[] snapshot;
        lock (_gate)
        {
            snapshot = _observers.ToArray();
        }

        foreach (var observer in snapshot)
        {
            observer.HandleGesture(gesture);
        }
    }
}
