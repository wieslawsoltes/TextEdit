namespace TextEdit.Core.Input;

/// <summary>
/// Detailed pointer position metadata.
/// </summary>
public readonly record struct EditorPointerPoint(
    EditorPoint Position,
    float Pressure,
    float Twist,
    float XTilt,
    float YTilt)
{
    /// <summary>
    /// Gets the position in device-independent pixels.
    /// </summary>
    public EditorPoint Position { get; } = Position;

    /// <summary>
    /// Gets the normalised pressure in the range [0,1].
    /// </summary>
    public float Pressure { get; } = Pressure;

    /// <summary>
    /// Gets the twist (barrel rotation) in degrees.
    /// </summary>
    public float Twist { get; } = Twist;

    /// <summary>
    /// Gets the X-axis tilt in degrees.
    /// </summary>
    public float XTilt { get; } = XTilt;

    /// <summary>
    /// Gets the Y-axis tilt in degrees.
    /// </summary>
    public float YTilt { get; } = YTilt;
}
