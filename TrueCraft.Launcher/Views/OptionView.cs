using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Iguina.Defs;
using Iguina.Entities;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Views;

public sealed class OptionView : ILauncherView
{
    private readonly LauncherGame _game;
    private readonly List<TexturePack> _texturePacks = new();

    private const string PreviewTextureId = "tcraft:texturepack:preview";

    private DropDown _resolutionDropDown;
    private Checkbox _fullscreenCheckBox;
    private Checkbox _invertMouseCheckBox;
    private ListBox _texturePackList;
    private Image _texturePackPreview;
    private Texture2D _currentPreview;
    private Button _openFolderButton;
    private Button _officialAssetsButton;
    private ProgressBar _officialAssetsProgress;
    private Paragraph _errorLabel;
    private Button _backButton;

    public OptionView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Options"));
        parent.AddChild(new HorizontalLine(_game.UI));

        parent.AddChild(new Paragraph(_game.UI, "Resolution"));
        _resolutionDropDown = new DropDown(_game.UI);
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
            resolutionIndex = _resolutionDropDown.ItemsCount - 1;
        }
        _resolutionDropDown.SelectedIndex = resolutionIndex;
        _resolutionDropDown.Events.OnValueChanged = _ =>
        {
            if (_resolutionDropDown.SelectedValue is null) return;
            UserSettings.Local.WindowResolution =
                WindowResolution.FromString(_resolutionDropDown.SelectedValue);
            UserSettings.Local.Save();
        };
        parent.AddChild(_resolutionDropDown);

        _fullscreenCheckBox = new Checkbox(_game.UI, "Fullscreen mode")
        {
            Checked = UserSettings.Local.IsFullscreen,
        };
        _fullscreenCheckBox.Events.OnValueChanged = _ =>
        {
            UserSettings.Local.IsFullscreen = _fullscreenCheckBox.Checked;
            UserSettings.Local.Save();
        };
        parent.AddChild(_fullscreenCheckBox);

        _invertMouseCheckBox = new Checkbox(_game.UI, "Inverted mouse")
        {
            Checked = UserSettings.Local.InvertedMouse,
        };
        _invertMouseCheckBox.Events.OnValueChanged = _ =>
        {
            UserSettings.Local.InvertedMouse = _invertMouseCheckBox.Checked;
            UserSettings.Local.Save();
        };
        parent.AddChild(_invertMouseCheckBox);

        parent.AddChild(new Paragraph(_game.UI, "Texture pack"));
        _texturePackList = new ListBox(_game.UI);
        _texturePackList.Size.Y.SetPixels(140);
        _texturePackList.Events.OnValueChanged = _ => OnTexturePackChanged();
        parent.AddChild(_texturePackList);

        _texturePackPreview = new Image(_game.UI, PreviewTextureId) { Anchor = Anchor.AutoCenter };
        _texturePackPreview.Size.X.SetPixels(96);
        _texturePackPreview.Size.Y.SetPixels(96);
        _texturePackPreview.Visible = false;
        parent.AddChild(_texturePackPreview);

        _openFolderButton = new Button(_game.UI, "Open texture pack folder")
        {
            Anchor = Anchor.AutoCenter,
        };
        _openFolderButton.Events.OnClick = _ => OpenTexturePackFolder();
        parent.AddChild(_openFolderButton);

        _officialAssetsProgress = new ProgressBar(_game.UI) { Visible = false };
        _officialAssetsProgress.MinValue = 0;
        _officialAssetsProgress.MaxValue = 100;
        parent.AddChild(_officialAssetsProgress);

        _officialAssetsButton = new Button(_game.UI, "Download Minecraft assets (Mojang)")
        {
            Anchor = Anchor.AutoCenter,
            Visible = false,
        };
        _officialAssetsButton.Events.OnClick = _ => StartOfficialAssetsDownload();
        parent.AddChild(_officialAssetsButton);

        _errorLabel = new Paragraph(_game.UI, string.Empty) { Visible = false };
        _errorLabel.OverrideStyles.TextFillColor = new Color(205, 92, 92, 255);
        parent.AddChild(_errorLabel);

        _backButton = new Button(_game.UI, "Back") { Anchor = Anchor.BottomCenter };
        _backButton.Events.OnClick = _ => _game.ShowView(new MainMenuView(_game));
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
        if (pack?.Image is null)
        {
            _texturePackPreview.Visible = false;
            return;
        }
        try
        {
            // pack.Image is a shared stream — reset before reading.
            if (pack.Image.CanSeek) pack.Image.Seek(0, SeekOrigin.Begin);
            var next = Texture2D.FromStream(_game.GraphicsDevice, pack.Image);
            _game.Renderer.RegisterTexture(PreviewTextureId, next);
            _currentPreview?.Dispose();
            _currentPreview = next;
            _texturePackPreview.Visible = true;
        }
        catch
        {
            // best effort — hide the preview rather than show a stale texture.
            _texturePackPreview.Visible = false;
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
            if (pack is not null)
            {
                _texturePacks.Add(pack);
                _texturePackList.AddItem(pack.Name);
            }
        }

        if (!officialPresent)
            _officialAssetsButton.Visible = true;
    }

    private void StartOfficialAssetsDownload()
    {
        // Pulls the Mojang beta 1.7.3 jar and extracts the textures into
        // Minecraft.zip. By proceeding the user accepts Mojang's asset terms
        // (https://account.mojang.com/terms#brand). The earlier modal that
        // surfaced these terms is gone — Iguina has no MessageBox equivalent
        // and an inline disclaimer in the button label communicates the same.
        _officialAssetsButton.Visible = false;
        _officialAssetsProgress.Visible = true;
        ShowError(null);

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
                    if (pack is not null)
                    {
                        _texturePacks.Add(pack);
                        _texturePackList.AddItem(pack.Name);
                    }
                });
            }
            catch (Exception ex)
            {
                _game.Invoke(() =>
                {
                    ShowError("Error retrieving assets: " + ex.Message);
                    _officialAssetsProgress.Visible = false;
                    _officialAssetsButton.Visible = true;
                });
            }
        });
    }

    private void ShowError(string message)
    {
        if (_errorLabel is null) return;
        if (string.IsNullOrEmpty(message))
        {
            _errorLabel.Visible = false;
            _errorLabel.Text = string.Empty;
            return;
        }
        _errorLabel.Text = message;
        _errorLabel.Visible = true;
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
        using var jarStream = new MemoryStream(jarBytes, writable: false);
        using var jar = new ZipArchive(jarStream, ZipArchiveMode.Read);

        if (File.Exists(outputPath))
            File.Delete(outputPath);

        using var outFile = File.Create(outputPath);
        using var zip = new ZipArchive(outFile, ZipArchiveMode.Create);

        foreach (var entry in jar.Entries)
        {
            if (!entry.FullName.StartsWith("terrain")
                && !entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                continue;
            var newEntry = zip.CreateEntry(entry.FullName);
            using var input = entry.Open();
            using var output = newEntry.Open();
            input.CopyTo(output);
        }
    }

    public void Dispose()
    {
        _game.Renderer.UnregisterTexture(PreviewTextureId);
        _currentPreview?.Dispose();
        _currentPreview = null;
    }
}
