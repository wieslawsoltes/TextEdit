using System;
using System.Collections.Immutable;

namespace TextEdit.Core.Caret;

/// <summary>
/// Event payload describing a caret/selection change.
/// </summary>
public sealed class CaretSelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CaretSelectionChangedEventArgs"/> class.
    /// </summary>
    public CaretSelectionChangedEventArgs(
        ImmutableArray<CaretState> previousState,
        ImmutableArray<CaretState> currentState)
    {
        PreviousState = previousState;
        CurrentState = currentState;
    }

    /// <summary>
    /// Gets the previous caret snapshot.
    /// </summary>
    public ImmutableArray<CaretState> PreviousState { get; }

    /// <summary>
    /// Gets the current caret snapshot.
    /// </summary>
    public ImmutableArray<CaretState> CurrentState { get; }
}
