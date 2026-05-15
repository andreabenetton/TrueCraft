using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Views
{
    public sealed class OptionView : ILauncherView
    {
        private readonly LauncherGame _game;
        private readonly List<TexturePack> _texturePacks = new();

        private DropDown _resolutionDropDown;
        private CheckBox _fullscreenCheckBox;
        private CheckBox _invertMouseCheckBox;
        private SelectList _texturePackList;
        private Image _texturePackPreview;
        private Button _openFolderButton;
        private Button _officialAssetsButton;
        private ProgressBar _officialAssetsProgress;
        private Button _backButton;

        public OptionView(LauncherGame game)
        {
            _game = game;
        }

        public void Mount(Panel parent)
        {
            parent.AddChild(new Header("Options"));
            parent.AddChild(new HorizontalLine());

            parent.AddChild(new Label("Resolution"));
            _resolutionDropDown = new DropDown(new Vector2(0, 220));
            var resolutionIndex = -1;
            for (var i = 0; i < WindowResolution.Defaults.Length; i++)
            {
                _resolutionDropDown.AddItem(WindowResolution.Defaults[i].ToString());
                if (resolutionIndex == -1
                    && WindowResolution.Defaults[i].Width == UserSettings.Local.WindowResolution.Width
                    && WindowResolution.Defaults[i].Height == UserSettings.Local.WindowResolution.Height)
                    resolutionIndex = i;
            }
            if (resolutionIndex == -1)
            {
                _resolutionDropDown.AddItem(UserSettings.Local.WindowResolution.ToString());
                resolutionIndex = _resolutionDropDown.Count - 1;
            }
            _resolutionDropDown.SelectedIndex = resolutionIndex;
            _resolutionDropDown.OnValueChange = _ =>
            {
                UserSettings.Local.WindowResolution =
                    WindowResolution.FromString(_resolutionDropDown.SelectedValue);
                UserSettings.Local.Save();
            };
            parent.AddChild(_resolutionDropDown);

            _fullscreenCheckBox = new CheckBox("Fullscreen mode", Anchor.Auto)
            {
                Checked = UserSettings.Local.IsFullscreen,
            };
            _fullscreenCheckBox.OnValueChange = _ =>
            {
                UserSettings.Local.IsFullscreen = _fullscreenCheckBox.Checked;
                UserSettings.Local.Save();
            };
            parent.AddChild(_fullscreenCheckBox);

            _invertMouseCheckBox = new CheckBox("Inverted mouse", Anchor.Auto)
            {
                Checked = UserSettings.Local.InvertedMouse,
            };
            _invertMouseCheckBox.OnValueChange = _ =>
            {
                UserSettings.Local.InvertedMouse = _invertMouseCheckBox.Checked;
                UserSettings.Local.Save();
            };
            parent.AddChild(_invertMouseCheckBox);

            parent.AddChild(new Label("Texture pack"));
            _texturePackList = new SelectList(new Vector2(0, 140));
            _texturePackList.OnValueChange = _ => OnTexturePackChanged();
            parent.AddChild(_texturePackList);

            _texturePackPreview = new Image((Texture2D)null, new Vector2(96, 96), ImageDrawMode.Stretch, Anchor.AutoCenter);
            parent.AddChild(_texturePackPreview);

            _openFolderButton = new Button("Open texture pack folder", ButtonSkin.Alternative, Anchor.Auto);
            _openFolderButton.OnClick = _ => OpenTexturePackFolder();
            parent.AddChild(_openFolderButton);

            _officialAssetsProgress = new ProgressBar(0, 100) { Visible = false };
            parent.AddChild(_officialAssetsProgress);
            _officialAssetsButton = new Button("Download Minecraft assets", ButtonSkin.Alternative, Anchor.Auto)
            {
                Visible = false,
            };
            _officialAssetsButton.OnClick = _ => DownloadOfficialAssets();
            parent.AddChild(_officialAssetsButton);

            _backButton = new Button("Back", ButtonSkin.Alternative, Anchor.BottomCenter);
            _backButton.OnClick = _ => _game.ShowView(new MainMenuView(_game));
            parent.AddChild(_backButton);

            LoadTexturePacks();
        }

        private void OnTexturePackChanged()
        {
            var idx = _texturePackList.SelectedIndex;
            if (idx < 0 || idx >= _texturePacks.Count) return;
            var pack = _texturePacks[idx];
            UserSettings.Local.SelectedTexturePack = pack.Name;
            UserSettings.Local.Save();
            UpdatePreview(pack);
        }

        private void UpdatePreview(TexturePack pack)
        {
            if (_texturePackPreview == null || pack?.Image == null) return;
            try
            {
                // pack.Image is a Stream over pack.png; reset to start in case the
                // previous reader advanced it.
                if (pack.Image.CanSeek) pack.Image.Seek(0, SeekOrigin.Begin);
                _texturePackPreview.Texture = Texture2D.FromStream(_game.GraphicsDevice, pack.Image);
            }
            catch
            {
                // best effort — leave the previous preview in place.
            }
        }

        private void OpenTexturePackFolder()
        {
            try
            {
                Directory.CreateDirectory(Paths.TexturePacks);
                Process.Start(new ProcessStartInfo(Paths.TexturePacks) { UseShellExecute = true });
            }
            catch
            {
                // best-effort
            }
        }

        private void LoadTexturePacks()
        {
            _texturePacks.Add(TexturePack.Default);
            _texturePackList.AddItem(TexturePack.Default.Name);

            if (!Directory.Exists(Paths.TexturePacks))
                Directory.CreateDirectory(Paths.TexturePacks);

            var officialPresent = false;
            foreach (var zip in Directory.EnumerateFiles(Paths.TexturePacks))
            {
                if (!zip.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (Path.GetFileName(zip) == "Minecraft.zip")
                    officialPresent = true;

                var pack = TexturePack.FromArchive(zip);
                if (pack != null)
                {
                    _texturePacks.Add(pack);
                    _texturePackList.AddItem(pack.Name);
                }
            }

            if (!officialPresent)
                _officialAssetsButton.Visible = true;
        }

        private void DownloadOfficialAssets()
        {
            MessageBox.ShowYesNoMsgBox(
                "Download Mojang assets",
                "Download the official Minecraft assets from Mojang?\n\n" +
                "By proceeding you agree to the Mojang asset guidelines:\n" +
                "https://account.mojang.com/terms#brand",
                onYes: () => { StartOfficialAssetsDownload(); return true; },
                onNo: () => true,
                yesText: "Download",
                noText: "Cancel");
        }

        private void StartOfficialAssetsDownload()
        {
            _officialAssetsButton.Visible = false;
            _officialAssetsProgress.Visible = true;

            Task.Run(() =>
            {
                try
                {
                    var jarBytes = DownloadJar();
                    var outputPath = Path.Combine(Paths.TexturePacks, "Minecraft.zip");
                    Directory.CreateDirectory(Paths.TexturePacks);
                    BuildTexturePackZip(jarBytes, outputPath);

                    _game.Invoke(() =>
                    {
                        _officialAssetsProgress.Visible = false;
                        var pack = TexturePack.FromArchive(outputPath);
                        _texturePacks.Add(pack);
                        _texturePackList.AddItem(pack.Name);
                    });
                }
                catch (Exception ex)
                {
                    _game.Invoke(() =>
                    {
                        MessageBox.ShowMsgBox("Error retrieving assets", ex.Message);
                        _officialAssetsProgress.Visible = false;
                        _officialAssetsButton.Visible = true;
                    });
                }
            });
        }

        private static byte[] DownloadJar()
        {
            using var httpClient = new HttpClient();
            return httpClient.GetByteArrayAsync(
                "http://s3.amazonaws.com/Minecraft.Download/versions/b1.7.3/b1.7.3.jar")
                .GetAwaiter().GetResult();
        }

        private static void BuildTexturePackZip(byte[] jarBytes, string outputPath)
        {
            // Read source jar (just a ZIP) and write a new zip with only the texture assets.
            using var jarStream = new MemoryStream(jarBytes, writable: false);
            using var jar = new ZipArchive(jarStream, ZipArchiveMode.Read);

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            using var outFile = File.Create(outputPath);
            using var zip = new ZipArchive(outFile, ZipArchiveMode.Create);

            var packEntry = zip.CreateEntry("pack.txt");
            using (var w = new StreamWriter(packEntry.Open()))
                w.Write("Minecraft textures");

            string[] dirs = { "terrain", "gui", "armor", "art", "environment", "item", "misc", "mob" };
            foreach (var src in jar.Entries)
            {
                var keep = false;
                foreach (var d in dirs)
                    if (src.FullName.StartsWith(d + "/", StringComparison.Ordinal)) { keep = true; break; }
                if (!keep) continue;
                CopyEntry(src, zip);
            }
            CopyEntryByName("pack.png", jar, zip);
            CopyEntryByName("terrain.png", jar, zip);
            CopyEntryByName("particles.png", jar, zip);
        }

        private static void CopyEntryByName(string name, ZipArchive source, ZipArchive destination)
        {
            var entry = source.GetEntry(name);
            if (entry != null) CopyEntry(entry, destination);
        }

        private static void CopyEntry(ZipArchiveEntry source, ZipArchive destination)
        {
            var dest = destination.CreateEntry(source.FullName);
            using var srcStream = source.Open();
            using var destStream = dest.Open();
            srcStream.CopyTo(destStream);
        }

        public void Dispose() { }
    }
}
