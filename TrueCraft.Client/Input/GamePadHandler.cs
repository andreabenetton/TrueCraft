using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TrueCraft.Client.Input;

public class GamePadHandler : GameComponent
{
    private static readonly Buttons[] AllButtons = (Buttons[]) Enum.GetValues(typeof(Buttons));

    public GamePadHandler(Game game) : base(game)
    {
        PlayerIndex = PlayerIndex.One;
    }

    public GamePadState State { get; set; }
    public PlayerIndex PlayerIndex { get; set; }

    public event EventHandler<GamePadButtonEventArgs> ButtonDown;
    public event EventHandler<GamePadButtonEventArgs> ButtonUp;

    public override void Initialize()
    {
        State = GamePad.GetState(PlayerIndex);

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var newState = GamePad.GetState(PlayerIndex);
        Process(newState, State);
        State = newState;

        base.Update(gameTime);
    }

    private void Process(GamePadState newState, GamePadState oldState)
    {
        if (!newState.IsConnected)
            return;
        if (newState.Buttons != oldState.Buttons)
        {
            foreach (var button in AllButtons)
            {
                var wasDown = oldState.IsButtonDown(button);
                var isDown = newState.IsButtonDown(button);
                if (isDown && !wasDown)
                    ButtonDown?.Invoke(this, new GamePadButtonEventArgs(button));
                else if (!isDown && wasDown)
                    ButtonUp?.Invoke(this, new GamePadButtonEventArgs(button));
            }
        }
    }
}
