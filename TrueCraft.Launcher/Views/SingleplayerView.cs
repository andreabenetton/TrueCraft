using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iguina.Defs;
using Iguina.Entities;
using Microsoft.Extensions.DependencyInjection;
using TrueCraft.Launcher.Sessions;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Views;

public sealed class SingleplayerView : ILauncherView
{
    private readonly LauncherGame _game;

    private ListBox _worldList;
    private Button _createButton;
    private Button _deleteButton;
    private Button _playButton;
    private Button _backButton;
    private Paragraph _progressLabel;
    private Paragraph _errorLabel;
    private ProgressBar _progressBar;

    public SingleplayerView(LauncherGame game)
    {
        _game = game;
        Worlds.Local ??= ActivatorUtilities.CreateInstance<Worlds>(App.Services);
        Worlds.Local.Load();
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Singleplayer"));
        parent.AddChild(new HorizontalLine(_game.UI));

        _worldList = new ListBox(_game.UI);
        _worldList.Size.Y.SetPixels(200);
        foreach (var world in Worlds.Local.Saves)
            _worldList.AddItem(world.Name);
        _worldList.Events.OnValueChanged = _ => UpdateSelectionSensitive();
        parent.AddChild(_worldList);

        _createButton = new Button(_game.UI, "New world") { Anchor = Anchor.AutoInlineLTR };
        _createButton.Size.X.SetPercents(50);
        _createButton.Events.OnClick = _ => _game.ShowView(
            new NewWorldView(_game, _ => _game.ShowView(new SingleplayerView(_game))));
        parent.AddChild(_createButton);

        _deleteButton = new Button(_game.UI, "Delete")
        {
            Anchor = Anchor.AutoInlineLTR,
            Enabled = false,
        };
        _deleteButton.Size.X.SetPercents(50);
        _deleteButton.Events.OnClick = _ => DeleteSelectedWorld();
        parent.AddChild(_deleteButton);

        _playButton = new Button(_game.UI, "Play")
        {
            Anchor = Anchor.AutoCenter,
            Enabled = false,
        };
        _playButton.Events.OnClick = _ => PlaySelectedWorld();
        parent.AddChild(_playButton);

        _progressLabel = new Paragraph(_game.UI, "Loading world...") { Visible = false };
        parent.AddChild(_progressLabel);

        _progressBar = new ProgressBar(_game.UI) { Visible = false };
        _progressBar.MinValue = 0;
        _progressBar.MaxValue = 100;
        parent.AddChild(_progressBar);

        _errorLabel = new Paragraph(_game.UI, string.Empty) { Visible = false };
        _errorLabel.OverrideStyles.TextFillColor = new Color(205, 92, 92, 255);
        parent.AddChild(_errorLabel);

        _backButton = new Button(_game.UI, "Back") { Anchor = Anchor.BottomCenter };
        _backButton.Events.OnClick = _ => _game.ShowView(new MainMenuView(_game));
        parent.AddChild(_backButton);
    }

    private void UpdateSelectionSensitive()
    {
        var has = _worldList.SelectedIndex >= 0;
        _playButton.Enabled = has;
        _deleteButton.Enabled = has;
    }

    private void DeleteSelectedWorld()
    {
        var idx = _worldList.SelectedIndex;
        if (idx < 0) return;
        var world = Worlds.Local.Saves[idx];
        _worldList.RemoveItem(idx);
        Worlds.Local.Saves = Worlds.Local.Saves.Where(s => s != world).ToArray();
        try
        {
            Directory.Delete(world.BaseDirectory, true);
        }
        catch (Exception ex)
        {
            ShowError($"Delete failed: {ex.Message}");
        }
    }

    private void PlaySelectedWorld()
    {
        var idx = _worldList.SelectedIndex;
        if (idx < 0) return;

        var world = Worlds.Local.Saves[idx];

        // Refuse to spin up a second server for a world the registry already
        // owns — two MultiplayerServers writing to the same world dir would
        // corrupt the save.
        if (_game.Sessions.TryFindByWorldPath(world.BaseDirectory, out _))
        {
            ShowError($"World '{world.Name}' is already running.");
            return;
        }

        // SingleplayerServer is constructor-injected with a transient
        // MultiplayerServer (see Program.cs), so each PlaySelectedWorld
        // call gets a fresh server on its own random port.
        var server = ActivatorUtilities.CreateInstance<SingleplayerServer>(
            App.Services, world);
        SetInteractive(false);
        ShowError(null);
        _progressBar.Visible = true;
        _progressLabel.Visible = true;

        Task.Run(() =>
        {
            try
            {
                server.Initialize((value, stage) =>
                    _game.Invoke(() =>
                    {
                        _progressLabel.Text = stage;
                        _progressBar.Value = Math.Clamp((int)(value * 100), 0, 100);
                    }));
                server.Start();

                _game.Invoke(() => RegisterAndLaunch(server, world));
            }
            catch (Exception ex)
            {
                _game.Invoke(() =>
                {
                    ShowError("Error loading world: " + ex.Message);
                    _progressBar.Visible = false;
                    _progressLabel.Visible = false;
                    SetInteractive(true);
                });
            }
        });
    }

    private void RegisterAndLaunch(SingleplayerServer server, TrueCraft.Core.World.World world)
    {
        var endpoint = server.Server.EndPoint;
        var args = $"{endpoint} {_game.User.Username} {_game.User.SessionId}";
        var process = _game.StartClient(args);
        var session = new GameSession(
            label: $"World: {world.Name}",
            server: server,
            client: process,
            worldPath: world.BaseDirectory);

        if (!_game.Sessions.TryAdd(session, out var error))
        {
            // Race: another view added the same world in between our
            // TryFindByWorldPath check and now. Roll back the server.
            try { server.Stop(); } catch { }
            ShowError(error);
            _progressBar.Visible = false;
            _progressLabel.Visible = false;
            SetInteractive(true);
            return;
        }

        process.Start();

        // Server is up, client is launched, session is in the registry —
        // the launcher UI is free again. The session continues to live
        // independently; ActiveGamesView exposes per-session controls.
        _progressBar.Visible = false;
        _progressLabel.Visible = false;
        SetInteractive(true);
    }

    private void SetInteractive(bool enabled)
    {
        _playButton.Enabled = enabled && _worldList.SelectedIndex >= 0;
        _deleteButton.Enabled = enabled && _worldList.SelectedIndex >= 0;
        _createButton.Enabled = enabled;
        _backButton.Enabled = enabled;
        _worldList.Enabled = enabled;
    }

    private void ShowError(string message)
    {
        if (_errorLabel is null)
            return;
        if (string.IsNullOrEmpty(message))
        {
            _errorLabel.Visible = false;
            _errorLabel.Text = string.Empty;
            return;
        }
        _errorLabel.Text = message;
        _errorLabel.Visible = true;
    }

    public void Dispose() { }
}
