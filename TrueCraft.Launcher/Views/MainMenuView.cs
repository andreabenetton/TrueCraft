using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using TrueCraft.Launcher.Entities;

namespace TrueCraft.Launcher.Views
{
    public sealed class MainMenuView : ILauncherView
    {
        private readonly LauncherGame _game;

        public MainMenuView(LauncherGame game)
        {
            _game = game;
        }

        public void Mount(Panel parent)
        {
            parent.AddChild(new Header($"Welcome, {_game.User.Username}"));
            parent.AddChild(new HorizontalLine());
            parent.AddChild(new LineSpace());

            var singleplayer = new MenuButton("Singleplayer", anchor: Anchor.AutoCenter);
            singleplayer.OnClick = _ => _game.ShowView(new SingleplayerView(_game));
            parent.AddChild(singleplayer);

            var multiplayer = new MenuButton("Multiplayer", anchor: Anchor.AutoCenter);
            multiplayer.OnClick = _ => _game.ShowView(new MultiplayerView(_game));
            parent.AddChild(multiplayer);

            var options = new MenuButton("Options", Anchor.AutoCenter);
            options.OnClick = _ => _game.ShowView(new OptionView(_game));
            parent.AddChild(options);

            parent.AddChild(new LineSpace());

            var signOut = new MenuButton("Sign out", Anchor.AutoCenter);
            signOut.OnClick = _ =>
            {
                _game.User.Username = null;
                _game.User.SessionId = null;
                _game.ShowView(new LoginView(_game));
            };
            parent.AddChild(signOut);

            var quit = new MenuButton("Quit", Anchor.AutoCenter);
            quit.OnClick = _ => _game.Exit();
            parent.AddChild(quit);
        }

        public void Dispose() { }
    }
}
