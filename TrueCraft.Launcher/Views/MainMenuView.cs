using System;
using Iguina.Defs;
using Iguina.Entities;
using TrueCraft.Launcher.Sessions;

namespace TrueCraft.Launcher.Views;

public sealed class MainMenuView : ILauncherView
{
    private readonly LauncherGame _game;
    private Button _activeGamesButton;
    private Button _quitButton;
    private Paragraph _quitNote;
    private Action<GameSession> _registryHandler;

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

        _activeGamesButton = new Button(_game.UI, ActiveGamesLabel()) { Anchor = Anchor.AutoCenter };
        _activeGamesButton.Events.OnClick = _ => _game.ShowView(new ActiveGamesView(_game));
        parent.AddChild(_activeGamesButton);

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

        _quitButton = new Button(_game.UI, "Quit") { Anchor = Anchor.AutoCenter };
        _quitButton.Events.OnClick = _ => _game.Exit();
        parent.AddChild(_quitButton);

        _quitNote = new Paragraph(_game.UI, "Stop active games to quit.") { Visible = false };
        parent.AddChild(_quitNote);

        // Keep the active-game button label and the Quit guard in sync with
        // the registry while this view is mounted. Single handler covers
        // both Added and Removed since both just need a refresh.
        _registryHandler = _ => RefreshSessionState();
        _game.Sessions.Added += _registryHandler;
        _game.Sessions.Removed += _registryHandler;
        RefreshSessionState();
    }

    private void RefreshSessionState()
    {
        var count = _game.Sessions.Count;
        if (_activeGamesButton is not null)
            _activeGamesButton.Paragraph.Text = ActiveGamesLabel(count);
        if (_quitButton is not null)
            _quitButton.Enabled = count == 0;
        if (_quitNote is not null)
            _quitNote.Visible = count > 0;
    }

    private string ActiveGamesLabel() => ActiveGamesLabel(_game.Sessions.Count);

    private static string ActiveGamesLabel(int count) =>
        count > 0 ? $"Active games ({count})" : "Active games";

    public void Dispose()
    {
        if (_registryHandler is not null)
        {
            _game.Sessions.Added -= _registryHandler;
            _game.Sessions.Removed -= _registryHandler;
        }
    }
}
