using System;

namespace TextEdit.Core.Input;

/// <summary>
/// Represents a point in device-independent pixels.
/// </summary>
public readonly record struct EditorPoint(double X, double Y)
{
    /// <summary>
    /// Creates a point with all coordinates set to zero.
    /// </summary>
    public static EditorPoint Zero => new(0, 0);

    /// <summary>
    /// Deconstructs the point into <paramref name="x"/> and <paramref name="y"/> coordinates.
    /// </summary>
    public void Deconstruct(out double x, out double y)
    {
        x = X;
        y = Y;
    }

    /// <summary>
    /// Returns the Euclidean distance between this point and <paramref name="other"/>.
    /// </summary>
    public double DistanceTo(in EditorPoint other)
    {
        var dx = other.X - X;
        var dy = other.Y - Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    /// <inheritdoc/>
    public override string ToString() => $"({X:0.###}, {Y:0.###})";
}
