using Microsoft.Xna.Framework.Input;

namespace TrueCraft.Client.Input;

public readonly struct GamePadButtonEventArgs
{
    public GamePadButtonEventArgs(Buttons button)
    {
        Button = button;
    }

    public Buttons Button { get; }
}
