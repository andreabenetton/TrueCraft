using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils;
using Microsoft.Xna.Framework;
using TrueCraft.Core.World;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Panels
{
    public class SinglePlayerPanel : BackgroundManagingPanel
    {
        private LauncherGame _game;

        private SelectList _worldListView;
        private Button _createWorldButton;
        private Button _deleteWorldButton;
        private Button _playButton;
        private Button _backButton;

        private NewWorldPanel _createWorldBox;
        private ProgressBar _progressBar;
        private SingleplayerServer _server;

        public EventCallback OnBack = null;

        public SinglePlayerPanel(LauncherGame game) : base(game)
        {
            _game = game;

            Size = new Vector2(730, 700);
            Worlds.Local = new Worlds();
            Worlds.Local.Load();
            
            _worldListView = new SelectList(new Vector2(0, 280));
            _createWorldButton = new Button("New world", ButtonSkin.Default, Anchor.Auto);
            _deleteWorldButton = new Button("Delete", ButtonSkin.Default, Anchor.AutoInline);
            _playButton = new Button("Play", ButtonSkin.Default, Anchor.Auto);
            _playButton.Enabled = _deleteWorldButton.Enabled = false;
            _backButton = new Button("Back", ButtonSkin.Default, Anchor.AutoInline);

            _createWorldBox = new NewWorldPanel(game);
            _createWorldBox.Visible = false;

            PopulateWorldList();

            _progressBar = new ProgressBar {Visible = false, Caption = {Text = "Loading world..."}};

            _backButton.OnClick += sender =>
            {
                OnBack?.Invoke(this);
            }; 
            _createWorldButton.OnClick += sender =>
            {
                _createWorldBox.Visible = true;
                Visible = false;
            };
            _createWorldBox.OnCancel += sender =>
            {
                _createWorldBox.Visible = false;
                Visible = true;
            };
            _createWorldBox.OnCommit += NewWorldCommit_Clicked;
            _worldListView.OnValueChange += sender =>
            {
                _playButton.Enabled = _deleteWorldButton.Enabled = _worldListView.HasSelectedValue;
            };
            _playButton.OnClick += PlayButton_Clicked;
            _deleteWorldButton.OnClick += sender =>
            {
                var world = Worlds.Local.Saves[_worldListView.SelectedIndex-1];
                _worldListView.RemoveItem(_worldListView.SelectedIndex);
                Worlds.Local.Saves = Worlds.Local.Saves.Where(s => s != world).ToArray();
                Directory.Delete(world.BaseDirectory, true);
            };

            UserInterface.Active.AddEntity(_createWorldBox);

            AddChild(new Header("Singleplayer"));
            AddChild(new HorizontalLine());
            AddChild(_worldListView);
            AddChild(_createWorldButton);
            AddChild(_deleteWorldButton);
            AddChild(new HorizontalLine());
            AddChild(_playButton);
            AddChild(_backButton);
            AddChild(_progressBar);
            

        }

        public void PlayButton_Clicked(Entity sender)
        {
            _server = new SingleplayerServer(Worlds.Local.Saves[_worldListView.SelectedIndex-1]);
            _playButton.Enabled = _backButton.Enabled = _createWorldButton.Enabled =
                _createWorldBox.Visible = _worldListView.Enabled = false;
            _progressBar.Visible = true;
            Task.Factory.StartNew(() =>
            {
                _server.Initialize((value, stage) =>
                    {
                        _progressBar.Caption.Text = stage;
                        _progressBar.Value = (int) value;
                    });
                _server.Start();

                _playButton.Enabled = _backButton.Enabled =
                    _createWorldButton.Enabled = _worldListView.Enabled = true;
                var launchParams = $"{_server.Server.EndPoint} {_game.User.Username} {_game.User.SessionId}";
                var process = new Process();
                process.StartInfo = new ProcessStartInfo("TrueCraft.Client.exe", launchParams);
                process.EnableRaisingEvents = true;
                process.Exited += (s, a) => 
                {
                    _progressBar.Visible = false;
                    _server.Stop();
                    _server.World.Save();
                };
                process.Start();

            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    MessageBox.ShowMsgBox("Error loading world", "It's possible that this world is corrupted.");
                    _progressBar.Visible = false;
                    _playButton.Enabled = _backButton.Enabled = _createWorldButton.Enabled =
                        _worldListView.Enabled = true;
                }
            });
        }

        private void NewWorldCommit_Clicked(Entity sender)
        {
            _createWorldBox.Visible = false;
            Visible = true;

            PopulateWorldList();
        }

        private void PopulateWorldList()
        {
            _worldListView.ClearItems();
            _worldListView.LockedItems[0] = true;
            _worldListView.AddItem($"{"{{RED}}"}{"World Name",-18} {"Date",-14}");
            World[] worlds = Worlds.Local.Saves.ToArray();
            foreach (var w in worlds)
            {
                _worldListView.AddItem(
                    $"{w.Name,-18} {w.LastModified.ToString("g", CultureInfo.CurrentCulture),-14}");
            }
        }
    }
}
