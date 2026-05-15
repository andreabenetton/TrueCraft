using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Panels
{
    public class OptionPanel : BackgroundManagingPanel
    {
        private LauncherGame _game;

        private readonly TexturePack _lastTexturePack;
        private readonly List<TexturePack> _texturePacks;

        private Header _optionLabel;
        private Label _resolutionLabel;
        private DropDown _resolutionComboBox;
        private CheckBox _fullscreenCheckBox;
        private CheckBox _invertMouseCheckBox;
        private Label _texturePackLabel;
        private SelectList _texturePackListView;
        private Image _texturePackPreview;
        private Button _officialAssetsButton;
        private ProgressBar _officialAssetsProgress;
        private Button _openFolderButton;
        private Button _backButton;

        public EventCallback OnBack = null;

        public OptionPanel(LauncherGame game) : base(game)
        {
            Size = new Vector2(730, 730);

            _texturePacks = new List<TexturePack>();
            _lastTexturePack = null;
            _game = game;

            _optionLabel = new Header("Options");

            _resolutionLabel = new Label("Select a resolution...");
            _resolutionComboBox = new DropDown();
            var settingResolutionIndex = -1;
            var currentResolutionIndex = -1;
            for (var i = 0; i < WindowResolution.Defaults.Length; i++)
            {
                _resolutionComboBox.AddItem(WindowResolution.Defaults[i].ToString());

                if (settingResolutionIndex == -1)
                    settingResolutionIndex =
                        WindowResolution.Defaults[i].Width == UserSettings.Local.WindowResolution.Width &&
                        WindowResolution.Defaults[i].Height == UserSettings.Local.WindowResolution.Height
                            ? i
                            : -1;
                if (currentResolutionIndex == -1)
                    currentResolutionIndex =
                        WindowResolution.Defaults[i].Width == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                        WindowResolution.Defaults[i].Height == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
                            ? i
                            : -1;
            }

            _resolutionComboBox.SelectedIndex = (settingResolutionIndex!=-1? settingResolutionIndex: currentResolutionIndex);

            _fullscreenCheckBox = new CheckBox("Fullscreen mode")
            {
                Checked = UserSettings.Local.IsFullscreen
            };

            _invertMouseCheckBox = new CheckBox("Inverted mouse")
            {
                Checked = UserSettings.Local.InvertedMouse
            };

            _texturePackLabel = new Label("Select a texture pack...");

            _texturePackListView = new SelectList(new Vector2(400, 200));
            LoadTexturePacks();
            _texturePackPreview = new Image(Texture2D.FromStream(_game.GraphicsDevice, _texturePacks[0].Image),new Vector2(200, 200));
            _texturePackPreview.Anchor = Anchor.AutoInline;

            _openFolderButton = new Button("Open texture pack folder");
            _backButton = new Button("Back");

            _resolutionComboBox.OnValueChange += sender =>
            {
                UserSettings.Local.WindowResolution =
                    WindowResolution.FromString(_resolutionComboBox.SelectedValue);
                UserSettings.Local.Save();
            };

            _fullscreenCheckBox.OnClick += sender =>
            {
                UserSettings.Local.IsFullscreen = !UserSettings.Local.IsFullscreen;
                UserSettings.Local.Save();
            };

            _invertMouseCheckBox.OnClick += sender =>
            {
                UserSettings.Local.InvertedMouse = !UserSettings.Local.InvertedMouse;
                UserSettings.Local.Save();
            };

            _texturePackListView.OnValueChange += sender =>
            {
                var texturePack = _texturePacks[_texturePackListView.SelectedIndex];
                if (_lastTexturePack != texturePack)
                {
                    UserSettings.Local.SelectedTexturePack = texturePack.Name;
                    UserSettings.Local.Save();
                    _texturePackPreview.Texture = Texture2D.FromStream(_game.GraphicsDevice, texturePack.Image);
                }
            };

            _openFolderButton.OnClick += sender =>
            {
                var dir = new DirectoryInfo(Paths.TexturePacks);
                Process.Start(dir.FullName);
            };

            _backButton.OnClick += sender =>
            {
                OnBack?.Invoke(this);
            };

            _officialAssetsButton = new Button("Download Minecraft assets") { Visible = false };
            _officialAssetsButton.OnClick += OfficialAssetsButton_Clicked;
            _officialAssetsProgress = new ProgressBar { Visible = false, Value = 0 };

            AddChild(_optionLabel);
            AddChild(_resolutionLabel);
            AddChild(_resolutionComboBox);
            AddChild(_fullscreenCheckBox);
            AddChild(_invertMouseCheckBox);
            AddChild(_texturePackLabel);
            AddChild(_texturePackListView);
            AddChild(_texturePackPreview);
            AddChild(_officialAssetsProgress);
            AddChild(_officialAssetsButton);
            AddChild(_openFolderButton);
            AddChild(_backButton);
        }

		private void OfficialAssetsButton_Clicked(Entity sender)
		{
			MessageBox.ShowMsgBox("Download Mojang assets",
				"This will download the official Minecraft assets from Mojang.\n\n" +
				"By proceeding you agree to the Mojang asset guidelines:\n\n" +
				"https://account.mojang.com/terms#brand\n\n" +
				"Proceed?", new[] {
		new MessageBox.MsgBoxOption("Yes", () =>
		{
			_officialAssetsButton.Visible = false;
			_officialAssetsProgress.Visible = true;

			Task.Run(async () =>
			{
				try
				{
					using (HttpClient client = new HttpClient())
					{
						var response = await client.GetAsync("http://s3.amazonaws.com/Minecraft.Download/versions/b1.7.3/b1.7.3.jar");
						response.EnsureSuccessStatusCode();

						using (var ms = new MemoryStream())
						{
							await response.Content.CopyToAsync(ms);
							ms.Seek(0, SeekOrigin.Begin);

							var jar = ZipFile.Read(ms);
							var zip = new ZipFile();
							zip.AddEntry("pack.txt", "Minecraft textures");

							string[] dirs =
							{
								"terrain", "gui", "armor", "art",
								"environment", "item", "misc", "mob"
							};

							foreach (var entry in jar.Entries)
								foreach (var c in dirs)
									if (entry.FileName.StartsWith(c + "/"))
										CopyBetweenZips(entry.FileName, jar, zip);

							CopyBetweenZips("pack.png", jar, zip);
							CopyBetweenZips("terrain.png", jar, zip);
							CopyBetweenZips("particles.png", jar, zip);

							zip.Save(Path.Combine(Paths.TexturePacks, "Minecraft.zip"));

							_officialAssetsProgress.Visible = false;
							var texturePack = TexturePack.FromArchive(
								Path.Combine(Paths.TexturePacks, "Minecraft.zip"));
							_texturePacks.Add(texturePack);
							AddTexturePackRow(texturePack);
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.ShowMsgBox("Error retrieving assets", ex.ToString());
					_officialAssetsProgress.Visible = false;
					_officialAssetsButton.Visible = true;
				}
			});

			return true;
		}),
		new MessageBox.MsgBoxOption("No", () => { return false; })
			});
		}

		public static void CopyBetweenZips(string name, ZipFile source, ZipFile destination)
        {
            using (var stream = source.Entries.SingleOrDefault(f => f.FileName == name)?.OpenReader())
            {
                var ms = new MemoryStream();
                CopyStream(stream, ms);
                ms.Seek(0, SeekOrigin.Begin);
                destination.AddEntry(name, ms);
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[16 * 1024];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, read);
        }

        private void LoadTexturePacks()
        {
            // We load the default texture pack specially.
            _texturePacks.Add(TexturePack.Default);
            AddTexturePackRow(TexturePack.Default);

            // Make sure to create the texture pack directory if there is none.
            if (!Directory.Exists(Paths.TexturePacks))
                Directory.CreateDirectory(Paths.TexturePacks);

            var zips = Directory.EnumerateFiles(Paths.TexturePacks);
            var officialPresent = false;
            foreach (var zip in zips)
            {
                if (!zip.EndsWith(".zip"))
                    continue;
                if (Path.GetFileName(zip) == "Minecraft.zip")
                    officialPresent = true;

                var texturePack = TexturePack.FromArchive(zip);
                if (texturePack != null)
                {
                    _texturePacks.Add(texturePack);
                    AddTexturePackRow(texturePack);
                }
            }

            if (!officialPresent)
                _officialAssetsButton.Visible = true;
        }

        private void AddTexturePackRow(TexturePack pack)
        {

            _texturePackListView.AddItem(pack.Name + "\r\n" + pack.Description);
        }

        protected void DrawBackground(LauncherGame game)
        {

        }
    }
}
