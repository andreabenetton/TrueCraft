using System;
using System.Diagnostics;
using System.Linq;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using TrueCraft.Core;
using TrueCraft.Launcher.Entities;

namespace TrueCraft.Launcher.Views
{
    public sealed class MultiplayerView : ILauncherView
    {
        private readonly LauncherGame _game;
        private TextInput _serverIPInput;
        private SelectList _serverList;
        private MenuButton _addServerButton;
        private MenuButton _removeServerButton;
        private MenuButton _connectButton;
        private MenuButton _backButton;
        private Panel _addServerPanel;
        private TextInput _newServerName;
        private TextInput _newServerAddress;
        private MenuButton _commitNewServer;
        private MenuButton _cancelNewServer;

        public MultiplayerView(LauncherGame game)
        {
            _game = game;
        }

        public void Mount(Panel parent)
        {
            parent.AddChild(new Header("Multiplayer"));
            parent.AddChild(new HorizontalLine());

            parent.AddChild(new Label("Server address"));
            _serverIPInput = new TextInput(false)
            {
                Value = UserSettings.Local?.LastIP ?? string.Empty,
                PlaceholderText = "host:port",
            };
            parent.AddChild(_serverIPInput);

            parent.AddChild(new Label("Saved servers"));
            _serverList = new SelectList(new Vector2(0, 160));
            if (UserSettings.Local?.FavoriteServers is not null)
                foreach (var s in UserSettings.Local.FavoriteServers)
                    _serverList.AddItem(s.Name);
            _serverList.OnValueChange = _ =>
            {
                var has = _serverList.SelectedIndex >= 0;
                _removeServerButton.Enabled = has;
                if (has && UserSettings.Local?.FavoriteServers is not null
                       && _serverList.SelectedIndex < UserSettings.Local.FavoriteServers.Length)
                    _serverIPInput.Value =
                        UserSettings.Local.FavoriteServers[_serverList.SelectedIndex].Address;
            };
            parent.AddChild(_serverList);

            _addServerButton = new MenuButton("Add server", anchor: Anchor.AutoInline,
                size: new Vector2(0.5f, -1));
            _addServerButton.OnClick = _ => SetAddServerVisible(true);
            parent.AddChild(_addServerButton);

            _removeServerButton = new MenuButton("Remove", Anchor.AutoInline,
                new Vector2(0.5f, -1));
            _removeServerButton.Enabled = false;
            _removeServerButton.OnClick = _ => RemoveSelectedServer();
            parent.AddChild(_removeServerButton);

            _addServerPanel = new Panel(new Vector2(0, 200), PanelSkin.None, Anchor.Auto)
            {
                Visible = false,
            };
            _addServerPanel.AddChild(new Label("Add new server"));
            _newServerName = new TextInput(false) { PlaceholderText = "Name" };
            _addServerPanel.AddChild(_newServerName);
            _newServerAddress = new TextInput(false) { PlaceholderText = "Address" };
            _addServerPanel.AddChild(_newServerAddress);
            _commitNewServer = new MenuButton("Add", anchor: Anchor.AutoInline,
                size: new Vector2(0.5f, -1));
            _commitNewServer.OnClick = _ => CommitAddServer();
            _addServerPanel.AddChild(_commitNewServer);
            _cancelNewServer = new MenuButton("Cancel", Anchor.AutoInline,
                new Vector2(0.5f, -1));
            _cancelNewServer.OnClick = _ => SetAddServerVisible(false);
            _addServerPanel.AddChild(_cancelNewServer);
            parent.AddChild(_addServerPanel);

            _connectButton = new MenuButton("Connect", anchor: Anchor.Auto);
            _connectButton.OnClick = _ => Connect();
            parent.AddChild(_connectButton);

            _backButton = new MenuButton("Back", Anchor.BottomCenter);
            _backButton.OnClick = _ => _game.ShowView(new MainMenuView(_game));
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
                (UserSettings.Local.FavoriteServers ?? Array.Empty<FavoriteServer>())
                .Concat(new[] { server }).ToArray();
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
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("TrueCraft.Client", args)
                {
                    UseShellExecute = false,
                },
                EnableRaisingEvents = true,
            };
            try
            {
                process.Start();
            }
            catch
            {
                // ignore — non-fatal; client may not be built.
            }
        }

        public void Dispose() { }
    }
}
