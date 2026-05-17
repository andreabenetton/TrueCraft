using System;
using Iguina.Defs;
using Iguina.Entities;
using TrueCraft.Launcher.Sessions;

namespace TrueCraft.Launcher.Views;

/// <summary>
///     Shows every <see cref="GameSession"/> currently in
///     <see cref="LauncherGame.Sessions"/> with per-row Stop buttons.
///     Subscribes to the registry's Added/Removed events so the list
///     reflects sessions added or ended from other views, or by client
///     processes exiting on their own.
/// </summary>
public sealed class ActiveGamesView : ILauncherView
{
    private readonly LauncherGame _game;
    private Panel _listPanel;
    private Action<GameSession> _addedHandler;
    private Action<GameSession> _removedHandler;

    public ActiveGamesView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Active games"));
        parent.AddChild(new HorizontalLine(_game.UI));

        _listPanel = new Panel(_game.UI);
        _listPanel.Size.Y.SetPixels(380);
        parent.AddChild(_listPanel);

        var back = new Button(_game.UI, "Back") { Anchor = Anchor.BottomCenter };
        back.Events.OnClick = _ => _game.ShowView(new MainMenuView(_game));
        parent.AddChild(back);

        // Subscribe before the initial render to avoid a window where a
        // session ends between Render and subscribe. Stored as fields so
        // Dispose can unsubscribe.
        _addedHandler = _ => Render();
        _removedHandler = _ => Render();
        _game.Sessions.Added += _addedHandler;
        _game.Sessions.Removed += _removedHandler;

        Render();
    }

    private void Render()
    {
        if (_listPanel is null) return;
        _listPanel.ClearChildren();

        var sessions = _game.Sessions.All;
        if (sessions.Count == 0)
        {
            _listPanel.AddChild(new Paragraph(_game.UI, "No active games."));
            return;
        }

        foreach (var session in sessions)
        {
            var label = $"{session.Label}  (started {session.StartedAt:HH:mm:ss})";
            _listPanel.AddChild(new Paragraph(_game.UI, label));

            var stop = new Button(_game.UI, "Stop") { Anchor = Anchor.AutoCenter };
            // Capture session locally so each row's click hits its own.
            var captured = session;
            stop.Events.OnClick = _ => captured.Stop();
            _listPanel.AddChild(stop);
        }
    }

    public void Dispose()
    {
        if (_addedHandler is not null)
            _game.Sessions.Added -= _addedHandler;
        if (_removedHandler is not null)
            _game.Sessions.Removed -= _removedHandler;
    }
}
