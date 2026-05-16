using Iguina.Defs;
using Iguina.Entities;

namespace TrueCraft.Launcher.Views;

public sealed class MainMenuView : ILauncherView
{
    private readonly LauncherGame _game;

    public MainMenuView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, $"Welcome, {_game.User.Username}"));
        parent.AddChild(new HorizontalLine(_game.UI));
        parent.AddChild(new RowsSpacer(_game.UI));

        var singleplayer = new Button(_game.UI, "Singleplayer") { Anchor = Anchor.AutoCenter };
        singleplayer.Events.OnClick = _ => _game.ShowView(new SingleplayerView(_game));
        parent.AddChild(singleplayer);

        var multiplayer = new Button(_game.UI, "Multiplayer") { Anchor = Anchor.AutoCenter };
        multiplayer.Events.OnClick = _ => _game.ShowView(new MultiplayerView(_game));
        parent.AddChild(multiplayer);

        var options = new Button(_game.UI, "Options") { Anchor = Anchor.AutoCenter };
        options.Events.OnClick = _ => _game.ShowView(new OptionView(_game));
        parent.AddChild(options);

        parent.AddChild(new RowsSpacer(_game.UI));

        var signOut = new Button(_game.UI, "Sign out") { Anchor = Anchor.AutoCenter };
        signOut.Events.OnClick = _ =>
        {
            _game.User.Username = null;
            _game.User.SessionId = null;
            _game.ShowView(new LoginView(_game));
        };
        parent.AddChild(signOut);

        var quit = new Button(_game.UI, "Quit") { Anchor = Anchor.AutoCenter };
        quit.Events.OnClick = _ => _game.Exit();
        parent.AddChild(quit);
    }

    public void Dispose() { }
}
