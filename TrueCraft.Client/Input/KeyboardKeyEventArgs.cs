using Microsoft.Xna.Framework.Input;

namespace TrueCraft.Client.Input;

/// <summary>
///     Provides the event data for keyboard key events.
/// </summary>
public readonly struct KeyboardKeyEventArgs
{
    public KeyboardKeyEventArgs(Keys key, bool isPressed)
    {
        Key = key;
        IsPressed = isPressed;
    }

    public Keys Key { get; }
    public bool IsPressed { get; }
}
