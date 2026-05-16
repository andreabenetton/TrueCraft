using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using TrueCraft;
using TrueCraft.Launcher.Entities;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Views
{
    public sealed class SingleplayerView : ILauncherView
    {
        private readonly LauncherGame _game;

        private SelectList _worldList;
        private MenuButton _createButton;
        private MenuButton _deleteButton;
        private MenuButton _playButton;
        private MenuButton _backButton;
        private Label _progressLabel;
        private ProgressBar _progressBar;
        private SingleplayerServer _server;

        public SingleplayerView(LauncherGame game)
        {
            _game = game;
            Worlds.Local = ActivatorUtilities.CreateInstance<Worlds>(App.Services);
            Worlds.Local.Load();
        }

        public void Mount(Panel parent)
        {
            parent.AddChild(new Header("Singleplayer"));
            parent.AddChild(new HorizontalLine());

            _worldList = new SelectList(new Vector2(0, 200));
            foreach (var world in Worlds.Local.Saves)
                _worldList.AddItem(world.Name);
            _worldList.OnValueChange = _ => UpdateSelectionSensitive();
            parent.AddChild(_worldList);

            _createButton = new MenuButton("New world", anchor: Anchor.AutoInline,
                size: new Vector2(0.5f, -1));
            _createButton.OnClick = _ => _game.ShowView(
                new NewWorldView(_game, world => _game.ShowView(new SingleplayerView(_game))));
            parent.AddChild(_createButton);

            _deleteButton = new MenuButton("Delete", Anchor.AutoInline,
                new Vector2(0.5f, -1));
            _deleteButton.Enabled = false;
            _deleteButton.OnClick = _ => DeleteSelectedWorld();
            parent.AddChild(_deleteButton);

            _playButton = new MenuButton("Play", anchor: Anchor.Auto);
            _playButton.Enabled = false;
            _playButton.OnClick = _ => PlaySelectedWorld();
            parent.AddChild(_playButton);

            _progressLabel = new Label("Loading world...", Anchor.Auto) { Visible = false };
            parent.AddChild(_progressLabel);
            _progressBar = new ProgressBar(0, 100) { Visible = false };
            parent.AddChild(_progressBar);

            _backButton = new MenuButton("Back", Anchor.BottomCenter);
            _backButton.OnClick = _ => _game.ShowView(new MainMenuView(_game));
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
                MessageBox.ShowMsgBox("Delete failed", ex.Message);
            }
        }

        private void PlaySelectedWorld()
        {
            var idx = _worldList.SelectedIndex;
            if (idx < 0) return;

            _server = ActivatorUtilities.CreateInstance<SingleplayerServer>(
                App.Services, Worlds.Local.Saves[idx]);
            SetInteractive(false);
            _progressBar.Visible = true;
            _progressLabel.Visible = true;

            Task.Run(() =>
            {
                try
                {
                    _server.Initialize((value, stage) =>
                        _game.Invoke(() =>
                        {
                            _progressLabel.Text = stage;
                            _progressBar.Value = Math.Clamp((int) (value * 100), 0, 100);
                        }));
                    _server.Start();

                    _game.Invoke(() => LaunchClient());
                }
                catch (Exception ex)
                {
                    _game.Invoke(() =>
                    {
                        MessageBox.ShowMsgBox("Error loading world",
                            "It's possible that this world is corrupted.\n\n" + ex.Message);
                        _progressBar.Visible = false;
                        _progressLabel.Visible = false;
                        SetInteractive(true);
                    });
                }
            });
        }

        private void LaunchClient()
        {
            var endpoint = _server.Server.EndPoint;
            var args = $"{endpoint} {_game.User.Username} {_game.User.SessionId}";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("TrueCraft.Client", args)
                {
                    UseShellExecute = false,
                },
                EnableRaisingEvents = true,
            };
            process.Exited += (_, _) => _game.Invoke(OnClientExited);
            process.Start();
        }

        private void OnClientExited()
        {
            _progressBar.Visible = false;
            _progressLabel.Visible = false;
            try
            {
                _server.Stop();
                _server.World.Save();
            }
            catch
            {
                // Save best-effort.
            }
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

        public void Dispose() { }
    }
}
