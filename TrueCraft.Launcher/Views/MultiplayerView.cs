using System.Linq;
using Iguina.Defs;
using Iguina.Entities;
using TrueCraft.Core;
using TrueCraft.Launcher.Sessions;

namespace TrueCraft.Launcher.Views;

public sealed class MultiplayerView : ILauncherView
{
    private readonly LauncherGame _game;
    private TextInput _serverIPInput;
    private ListBox _serverList;
    private Button _addServerButton;
    private Button _removeServerButton;
    private Button _connectButton;
    private Button _backButton;
    private Panel _addServerPanel;
    private TextInput _newServerName;
    private TextInput _newServerAddress;
    private Button _commitNewServer;
    private Button _cancelNewServer;

    public MultiplayerView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Multiplayer"));
        parent.AddChild(new HorizontalLine(_game.UI));

        parent.AddChild(new Paragraph(_game.UI, "Server address"));
        _serverIPInput = new TextInput(_game.UI)
        {
            Value = UserSettings.Local?.LastIP ?? string.Empty,
            PlaceholderText = "host:port",
        };
        parent.AddChild(_serverIPInput);

        parent.AddChild(new Paragraph(_game.UI, "Saved servers"));
        _serverList = new ListBox(_game.UI);
        _serverList.Size.Y.SetPixels(160);
        if (UserSettings.Local?.FavoriteServers is not null)
            foreach (var s in UserSettings.Local.FavoriteServers)
                _serverList.AddItem(s.Name);
        _serverList.Events.OnValueChanged = _ =>
        {
            var has = _serverList.SelectedIndex >= 0;
            _removeServerButton.Enabled = has;
            if (has && UserSettings.Local?.FavoriteServers is not null
                   && _serverList.SelectedIndex < UserSettings.Local.FavoriteServers.Length)
                _serverIPInput.Value =
                    UserSettings.Local.FavoriteServers[_serverList.SelectedIndex].Address;
        };
        parent.AddChild(_serverList);

        _addServerButton = new Button(_game.UI, "Add server") { Anchor = Anchor.AutoInlineLTR };
        _addServerButton.Size.X.SetPercents(50);
        _addServerButton.Events.OnClick = _ => SetAddServerVisible(true);
        parent.AddChild(_addServerButton);

        _removeServerButton = new Button(_game.UI, "Remove")
        {
            Anchor = Anchor.AutoInlineLTR,
            Enabled = false,
        };
        _removeServerButton.Size.X.SetPercents(50);
        _removeServerButton.Events.OnClick = _ => RemoveSelectedServer();
        parent.AddChild(_removeServerButton);

        _addServerPanel = new Panel(_game.UI) { Visible = false };
        _addServerPanel.Size.Y.SetPixels(200);
        _addServerPanel.AddChild(new Paragraph(_game.UI, "Add new server"));
        _newServerName = new TextInput(_game.UI) { PlaceholderText = "Name" };
        _addServerPanel.AddChild(_newServerName);
        _newServerAddress = new TextInput(_game.UI) { PlaceholderText = "Address" };
        _addServerPanel.AddChild(_newServerAddress);

        _commitNewServer = new Button(_game.UI, "Add") { Anchor = Anchor.AutoInlineLTR };
        _commitNewServer.Size.X.SetPercents(50);
        _commitNewServer.Events.OnClick = _ => CommitAddServer();
        _addServerPanel.AddChild(_commitNewServer);

        _cancelNewServer = new Button(_game.UI, "Cancel") { Anchor = Anchor.AutoInlineLTR };
        _cancelNewServer.Size.X.SetPercents(50);
        _cancelNewServer.Events.OnClick = _ => SetAddServerVisible(false);
        _addServerPanel.AddChild(_cancelNewServer);
        parent.AddChild(_addServerPanel);

        _connectButton = new Button(_game.UI, "Connect") { Anchor = Anchor.AutoCenter };
        _connectButton.Events.OnClick = _ => Connect();
        parent.AddChild(_connectButton);

        _backButton = new Button(_game.UI, "Back") { Anchor = Anchor.BottomCenter };
        _backButton.Events.OnClick = _ => _game.ShowView(new MainMenuView(_game));
        parent.AddChild(_backButton);
    }

    private void SetAddServerVisible(bool visible)
    {
        _addServerPanel.Visible = visible;
        if (visible)
        {
            _newServerName.Value = string.Empty;
            _newServerAddress.Value = string.Empty;
        }
    }

    private void CommitAddServer()
    {
        var name = _newServerName.Value?.Trim();
        var address = _newServerAddress.Value?.Trim();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address))
            return;
        var server = new FavoriteServer { Name = name, Address = address };
        UserSettings.Local.FavoriteServers =
            (UserSettings.Local.FavoriteServers ?? [])
            .Concat([server]).ToArray();
        UserSettings.Local.Save();
        _serverList.AddItem(server.Name);
        SetAddServerVisible(false);
    }

    private void RemoveSelectedServer()
    {
        var idx = _serverList.SelectedIndex;
        if (idx < 0 || UserSettings.Local?.FavoriteServers is null) return;
        var server = UserSettings.Local.FavoriteServers[idx];
        _serverList.RemoveItem(idx);
        UserSettings.Local.FavoriteServers = UserSettings.Local.FavoriteServers
            .Where(s => s.Name != server.Name || s.Address != server.Address).ToArray();
        UserSettings.Local.Save();
    }

    private void Connect()
    {
        var ip = _serverIPInput.Value;
        if (string.IsNullOrWhiteSpace(ip))
            return;

        UserSettings.Local.LastIP = ip;
        UserSettings.Local.Save();

        var args = $"{ip} {_game.User.Username} {_game.User.SessionId}";
        try
        {
            var process = _game.StartClient(args);
            // Multiplayer sessions have no in-launcher server (server=null);
            // worldPath is null so the registry's WorldPath dedupe doesn't
            // fire — two clients to the same remote server are intentionally
            // allowed.
            var session = new GameSession(
                label: $"Server: {ip}",
                server: null,
                client: process,
                worldPath: null);
            _game.Sessions.TryAdd(session, out _);
            process.Start();
        }
        catch
        {
            // ignore — non-fatal; client may not be built.
        }
    }

    public void Dispose() { }
}
