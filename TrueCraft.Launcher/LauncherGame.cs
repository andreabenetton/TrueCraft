using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Core;
using TrueCraft.Launcher.Entities;
using TrueCraft.Launcher.Panels;
using TrueCraft.Launcher.Views;

namespace TrueCraft.Launcher
{
    /// <summary>
    ///     MonoGame host for the launcher UI. Replaces the old Xwt
    ///     <c>LauncherWindow</c> + <c>Application.Run()</c> loop.
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
        private ILauncherView _currentView;

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

        public SpriteBatch Sprites => _spriteBatch;
        public int ScreenWidth => _graphics.PreferredBackBufferWidth;
        public int ScreenHeight => _graphics.PreferredBackBufferHeight;

        public static LauncherGame Current => _instance
            ?? throw new InvalidOperationException("LauncherGame not yet constructed");

        /// <summary>
        ///     Marshal an action onto the main (game) thread. Background tasks call this
        ///     from <c>Task.Run</c> continuations to safely mutate GeonBit.UI state.
        /// </summary>
        public void Invoke(Action action) => _mainThreadActions.Enqueue(action);

        public void ShowView(ILauncherView view)
        {
            _currentView?.Dispose();
            if (InteractionPanel != null)
            {
                while (InteractionPanel.Children.Count > 0)
                    InteractionPanel.RemoveChild(InteractionPanel.Children[0]);
            }
            _currentView = view;
            view.Mount(InteractionPanel);
            // The view just constructed MenuButtons; load their themed skin sprites.
            MenuButton.LoadButtonsTexture();
        }

        protected override void Initialize()
        {
            UserInterface.Initialize(Content, BuiltinThemes.lowres);
            UserInterface.Active.UseRenderTarget = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            MenuButton.Initialize(this);
            BuildShell();
            BackgroundManagingPanel.LoadPanelsBackgroundTexture();
            ShowView(new LoginView(this));
            StartSessionKeepAlive();
        }

        private void BuildShell()
        {
            // Two-column shell: welcome panel on the left, interaction panel on the right.
            // Both are BackgroundManagingPanels so the tiled options_background renders behind them.
            WelcomePanel = new BackgroundManagingPanel(this, new Vector2(450, 540), PanelSkin.Default,
                Anchor.CenterLeft, new Vector2(20, 0));
            InteractionPanel = new BackgroundManagingPanel(this, new Vector2(500, 540), PanelSkin.Default,
                Anchor.CenterRight, new Vector2(20, 0));
            UserInterface.Active.AddEntity(WelcomePanel);
            UserInterface.Active.AddEntity(InteractionPanel);
            new WelcomeView(this).Mount(WelcomePanel);
        }

        protected override void Update(GameTime gameTime)
        {
            while (_mainThreadActions.TryDequeue(out var action))
                action();
            UserInterface.Active.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Render the UI to its internal target, clear the backbuffer, tile the panel
            // backgrounds onto the screen, blit the UI on top, draw cursor.
            UserInterface.Active.Draw(_spriteBatch);
            GraphicsDevice.Clear(new Color(20, 20, 30));
            BackgroundManagingPanel.DrawPanelsBackground();
            UserInterface.Active.DrawMainRenderTarget(_spriteBatch);
            UserInterface.Active.DrawCursor(_spriteBatch);
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
                        var url = string.Format(TrueCraftUser.AuthServer + "/session?name={0}&session={1}",
                            user.Username, user.SessionId);
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
}
