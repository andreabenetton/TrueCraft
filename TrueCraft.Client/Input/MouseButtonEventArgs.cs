namespace TrueCraft.Client.Input;

/// <summary>
///     Provides the event data for mouse button events.
/// </summary>
public readonly struct MouseButtonEventArgs
{
    public MouseButtonEventArgs(int x, int y, MouseButton button, bool isPressed)
    {
        X = x;
        Y = y;
        Button = button;
        IsPressed = isPressed;
    }

    public int X { get; }
    public int Y { get; }
    public MouseButton Button { get; }
    public bool IsPressed { get; }
}
