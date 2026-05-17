using System;
using System.IO;
using Iguina;
using Iguina.Defs;
using Iguina.Demo.MonoGame;
using Iguina.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProgressBarProbe;

/// <summary>
///     Minimal MonoGame host that renders a single Iguina <see cref="ProgressBar"/>
///     against the LowRes theme so its styling can be iterated on without
///     dragging the rest of the launcher (world generation, music, login,
///     etc.) into the loop.
///
///     Value is hard-pinned to 50 % so a single screenshot tells us
///     unambiguously whether the fill reaches the right spot. To inspect the
///     full range, switch the constant <see cref="ProbeValue"/> at the top
///     of the class to a negative sentinel (e.g. -1) and the bar will instead
///     slowly cycle 0 → 100 → 0 over <see cref="ProbePeriodSeconds"/>.
/// </summary>
public sealed class Probe : Game
{
    private const int ProbeValue = 100;
    private const float ProbePeriodSeconds = 8f;
    private const string ThemeFolder = "IguinaTheme";

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private MonoGameRenderer _renderer;
    private MonoGameInput _input;
    private UISystem _ui;
    private ProgressBar _bar;
    private Paragraph _label;
    private float _elapsed;

    public Probe()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 600,
            PreferredBackBufferHeight = 220,
            SynchronizeWithVerticalRetrace = true,
        };
        IsMouseVisible = true;
        Window.Title = "ProgressBar Probe";
        Window.AllowUserResizing = false;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var assetsPath = Path.Combine(AppContext.BaseDirectory, ThemeFolder);
        _renderer = new MonoGameRenderer(Content, GraphicsDevice, _spriteBatch, assetsPath);
        // Theme bundles a Pixel_Regular.fnt; load it so paragraphs render.
        var fontDir = Path.Combine(assetsPath, "Fonts");
        if (Directory.Exists(fontDir))
        {
            foreach (var ttf in Directory.EnumerateFiles(fontDir, "*.ttf"))
                _renderer.RegisterFont(Path.GetFileNameWithoutExtension(ttf), ttf);
            foreach (var fnt in Directory.EnumerateFiles(fontDir, "*.fnt"))
                _renderer.RegisterBMFont(Path.GetFileNameWithoutExtension(fnt), fnt);
        }
        _input = new MonoGameInput();
        _ui = new UISystem(Path.Combine(assetsPath, "system_style.json"), _renderer, _input);

        var panel = new Panel(_ui) { Anchor = Anchor.Center };
        panel.Size.X.SetPixels(540);
        panel.Size.Y.SetPixels(160);
        _ui.Root.AddChild(panel);

        _label = new Paragraph(_ui, "ProgressBar probe");
        panel.AddChild(_label);
        panel.AddChild(new HorizontalLine(_ui));

        _bar = new ProgressBar(_ui)
        {
            MinValue = 0,
            MaxValue = 100,
            Value = ProbeValue >= 0 ? ProbeValue : 0,
        };
        panel.AddChild(_bar);
    }

    protected override void Update(GameTime gameTime)
    {
        _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (ProbeValue < 0)
        {
            // 0 -> 100 -> 0 triangle wave so each second of the run gives us
            // a fresh value to screenshot.
            var t = _elapsed / ProbePeriodSeconds % 1f;
            var v = (int)(t < 0.5f ? t * 2f * 100f : (1f - (t - 0.5f) * 2f) * 100f);
            _bar.Value = Math.Clamp(v, 0, 100);
            _label.Text = $"ProgressBar probe — Value={_bar.Value}";
        }
        else
        {
            _label.Text = $"ProgressBar probe — fixed Value={_bar.Value}";
        }
        _input.StartFrame(gameTime);
        _ui.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        _input.EndFrame();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color(20, 20, 30));
        _renderer.StartFrame();
        _ui.Draw();
        _renderer.EndFrame();
        base.Draw(gameTime);
    }
}

public static class Program
{
    [STAThread]
    public static void Main()
    {
        using var probe = new Probe();
        probe.Run();
    }
}
