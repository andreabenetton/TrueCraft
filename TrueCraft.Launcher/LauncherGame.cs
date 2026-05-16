using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using Iguina;
using Iguina.Defs;
using Iguina.Demo.MonoGame;
using Iguina.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TrueCraft.Core;
using TrueCraft.Launcher.Views;

namespace TrueCraft.Launcher;

/// <summary>
///     MonoGame host for the launcher UI. Hosts an Iguina <see cref="UISystem"/>
///     for the GUI; replaces the prior GeonBit.UI-based shell.
/// </summary>
public sealed class LauncherGame : Game
{
    private static LauncherGame _instance;
    private static readonly CancellationTokenSource SessionCancel = new();
    private static readonly HttpClient SessionClient = new();
    private static Thread _sessionThread;

    private readonly GraphicsDeviceManager _graphics;
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private SpriteBatch _spriteBatch;
    private MonoGameRenderer _renderer;
    private MonoGameInput _input;
    private UISystem _ui;
    private ILauncherView _currentView;
    private Song _song;

    // Iguina theme: ships at bin/Debug/IguinaTheme/ via the Iguina.Demo theme assets
    // linked in TrueCraft.Launcher.csproj.
    private const string ThemeFolder = "IguinaTheme";

    public LauncherGame()
    {
        _instance = this;
        Content.RootDirectory = "Content";
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1024,
            PreferredBackBufferHeight = 600,
            SynchronizeWithVerticalRetrace = true,
        };
        IsMouseVisible = true;
        Window.Title = "TrueCraft Launcher";
        Window.AllowUserResizing = false;
    }

    public TrueCraftUser User { get; } = new TrueCraftUser();
    public Panel WelcomePanel { get; private set; }
    public Panel InteractionPanel { get; private set; }
    public UISystem UI => _ui;

    /// <summary>
    ///     Exposed so views can register runtime-decoded textures (e.g. texture-pack
    ///     previews) by id before drawing them via an <see cref="Iguina.Entities.Image"/>.
    /// </summary>
    public MonoGameRenderer Renderer => _renderer;

    public SpriteBatch Sprites => _spriteBatch;
    public int ScreenWidth => _graphics.PreferredBackBufferWidth;
    public int ScreenHeight => _graphics.PreferredBackBufferHeight;

    public static LauncherGame Current => _instance
        ?? throw new InvalidOperationException("LauncherGame not yet constructed");

    /// <summary>
    ///     Marshal an action onto the main (game) thread. Background tasks call this
    ///     from <c>Task.Run</c> continuations to safely mutate UI state.
    /// </summary>
    public void Invoke(Action action) => _mainThreadActions.Enqueue(action);

    public void ShowView(ILauncherView view)
    {
        _currentView?.Dispose();
        InteractionPanel?.ClearChildren();
        _currentView = view;
        view.Mount(InteractionPanel);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var assetsPath = Path.Combine(AppContext.BaseDirectory, ThemeFolder);
        _renderer = new MonoGameRenderer(Content, GraphicsDevice, _spriteBatch, assetsPath);
        _input = new MonoGameInput();
        _ui = new UISystem(Path.Combine(assetsPath, "system_style.json"), _renderer, _input);

        BuildShell();
        ShowView(new LoginView(this));
        StartSessionKeepAlive();

        // Background music. Content pipeline asset; runtime depends on Beginning_2.xnb
        // which is part of the deferred MGCB .xnb gap.
        try
        {
            _song = Content.Load<Song>("Beginning_2");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_song);
        }
        catch
        {
            // No .xnb yet — silent until the content pipeline gap is closed.
        }
    }

    private void BuildShell()
    {
        // Two-column shell: welcome panel on the left, interaction panel on the right.
        WelcomePanel = new Panel(_ui) { Anchor = Anchor.CenterLeft };
        WelcomePanel.Size.X.SetPixels(450);
        WelcomePanel.Size.Y.SetPixels(540);
        WelcomePanel.Offset.X.SetPixels(20);
        _ui.Root.AddChild(WelcomePanel);

        InteractionPanel = new Panel(_ui) { Anchor = Anchor.CenterRight };
        InteractionPanel.Size.X.SetPixels(500);
        InteractionPanel.Size.Y.SetPixels(540);
        InteractionPanel.Offset.X.SetPixels(20);
        _ui.Root.AddChild(InteractionPanel);

        new WelcomeView(this).Mount(WelcomePanel);
    }

    protected override void Update(GameTime gameTime)
    {
        while (_mainThreadActions.TryDequeue(out var action))
            action();

        if (_ui is not null)
        {
            _input.StartFrame(gameTime);
            _ui.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            _input.EndFrame();
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color(20, 20, 30));
        if (_ui is not null)
        {
            _renderer.StartFrame();
            _ui.Draw();
            _renderer.EndFrame();
        }
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentView?.Dispose();
        }
        base.Dispose(disposing);
    }

    // ----- Session keep-alive (background) -----

    private void StartSessionKeepAlive()
    {
        _sessionThread = new Thread(KeepSessionAlive)
        {
            IsBackground = true,
            Priority = ThreadPriority.Lowest,
            Name = "TrueCraftLauncher.SessionKeepAlive",
        };
        _sessionThread.Start();
    }

    public static void CancelSessionKeepAlive()
    {
        SessionCancel.Cancel();
        _sessionThread?.Join(TimeSpan.FromSeconds(5));
        SessionClient.Dispose();
    }

    private static void KeepSessionAlive()
    {
        var token = SessionCancel.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                var user = Current.User;
                if (!string.IsNullOrEmpty(user?.SessionId) && user.SessionId != "-")
                {
                    var url = $"{TrueCraftUser.AuthServer}/session?name={user.Username}&session={user.SessionId}";
                    SessionClient.GetStringAsync(url).GetAwaiter().GetResult();
                }
            }
            catch
            {
                // Network errors are not fatal — retry next interval.
            }

            token.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
        }
    }
}
