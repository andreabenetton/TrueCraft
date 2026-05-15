using GeonBit.UI;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TrueCraft.Core;
using TrueCraft.Launcher.Entities;
using TrueCraft.Launcher.Panels;

namespace TrueCraft.Launcher
{
    /// <summary>
    /// This is the main 'Game' instance for your game.
    /// </summary>
    public class LauncherGame : Game
    {
        private readonly ILogger<LauncherGame> _logger;

        // graphics and spritebatch
        private GraphicsDeviceManager _graphics;

        private SpriteBatch _spriteBatch;

        private Song _song;

        private int _screenWidth;
        private int _screenHeight;


        public TrueCraftUser User;

        /// <summary>
        /// Create the game instance.
        /// </summary>
        public LauncherGame()
        {
            UserSettings.Local = new UserSettings();
            UserSettings.Local.Load();

            LauncherConfiguration launcherConfiguration = new LauncherConfiguration();
            Services.AddService(launcherConfiguration);
            _logger = new LoggerService<LauncherGame>(launcherConfiguration.Configuration);
            Services.AddService(typeof(ILogger),_logger);

            User = new TrueCraftUser(){ Username = "Guest" };

            // init graphics device manager and set content root
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.IsBorderless = true;

            foreach (var displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (displayMode.Width > 1000)
                    WindowResolution.Populate(displayMode.Width, displayMode.Height);
            }
        }

        public int ScreenWidth => _screenWidth;
        public int ScreenHeight => _screenHeight;
        public SpriteBatch Sprites => _spriteBatch;

        /// <summary>
        /// Initialize the main application.
        /// </summary>
        protected override void Initialize()
        {         
            // create and init the UI manager
            UserInterface.Initialize(Content, BuiltinThemes.hd);
            UserInterface.Active.UseRenderTarget = true;

            // draw cursor outside the render target
            UserInterface.Active.IncludeCursorInRenderTarget = false;

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // make the window fullscreen (but still with border and top control bar)
            _screenWidth = _graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _screenHeight = _graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            _graphics.PreferredBackBufferWidth = _screenWidth;
            _graphics.PreferredBackBufferHeight = _screenHeight;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            MenuButton.Initialize(this);

            // init ui and examples
            LoginPanel loginPanel = new LoginPanel(this);
            UserInterface.Active.AddEntity(loginPanel);
            MainMenuPanel menuPanel = new MainMenuPanel(this);
            UserInterface.Active.AddEntity(menuPanel);
            SinglePlayerPanel singlePlayerPanel = new SinglePlayerPanel(this);
            UserInterface.Active.AddEntity(singlePlayerPanel);
            OptionPanel optionPanel = new OptionPanel(this);
            UserInterface.Active.AddEntity(optionPanel);
            menuPanel.Visible = true;
            singlePlayerPanel.Visible = false;
            optionPanel.Visible = false;
            loginPanel.Visible = false;

            loginPanel.OnLogin = _ =>
            {
                menuPanel.ActivatePlayButton();
                loginPanel.Visible = false;
                menuPanel.Visible = true;
            };
            menuPanel.OnQuitSelected = _ => { Exit(); };
            menuPanel.OnLoginSelected = _ =>
            {
                menuPanel.Visible = false;
                loginPanel.Visible = true;
            };
            menuPanel.OnSinglePlayerSelected = _ =>
            {
                menuPanel.Visible = false;
                singlePlayerPanel.Visible = true;
            };
            menuPanel.OnOptionsSelected = _ =>
            {
                menuPanel.Visible = false;
                optionPanel.Visible = true;
            };
            singlePlayerPanel.OnBack = _ =>
            {
                menuPanel.Visible = true;
                singlePlayerPanel.Visible = false;
            };
            optionPanel.OnBack = _ =>
            {
                menuPanel.Visible = true;
                optionPanel.Visible = false;
            };

            UserInterface.Active.OnClick = entity => { _logger.LogDebug("Click: " + entity.GetType().Name); };
            UserInterface.Active.OnRightClick = entity => { _logger.LogDebug("RightClick: " + entity.GetType().Name); };
            UserInterface.Active.OnMouseDown = entity => { _logger.LogDebug("MouseDown: " + entity.GetType().Name); };
            UserInterface.Active.OnRightMouseDown = entity => { _logger.LogDebug("RightMouseDown: " + entity.GetType().Name); };
            UserInterface.Active.OnMouseEnter = entity => { _logger.LogDebug("MouseEnter: " + entity.GetType().Name); };
            UserInterface.Active.OnMouseLeave = entity => { _logger.LogDebug("MouseLeave: " + entity.GetType().Name); };
            UserInterface.Active.OnMouseReleased = entity => { _logger.LogDebug("MouseReleased: " + entity.GetType().Name); };
            UserInterface.Active.OnMouseWheelScroll = entity => { _logger.LogDebug("Scroll: " + entity.GetType().Name); };
            UserInterface.Active.OnStartDrag = entity => { _logger.LogDebug("StartDrag: " + entity.GetType().Name); };
            UserInterface.Active.OnStopDrag = entity => { _logger.LogDebug("StopDrag: " + entity.GetType().Name); };
            UserInterface.Active.OnFocusChange = entity => { _logger.LogDebug("FocusChange: " + entity.GetType().Name); };
            UserInterface.Active.OnValueChange = entity => { _logger.LogDebug("ValueChanged: " + entity.GetType().Name); };


            // start music
            if (_song!=null)
                MediaPlayer.Play(_song);

            // call base initialize
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _song = Content.Load<Song>("Beginning_2");
            BackgroundManagingPanel.LoadPanelsBackgroundTexture();
            MenuButton.LoadButtonsTexture();

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // make sure window is focused
            if (!IsActive)
                return;

            // exit on escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // update UI
            UserInterface.Active.Update(gameTime);

            // show currently active entity (for testing)
            //targetEntityShow.Text = "Target Entity: " + (UserInterface.Active.TargetEntity != null ? UserInterface.Active.TargetEntity.GetType().Name : "null");

            // call base update
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // clear buffer
            GraphicsDevice.Clear(Color.CornflowerBlue);


            // draw ui
            UserInterface.Active.Draw(_spriteBatch);

            BackgroundManagingPanel.DrawPanelsBackground();


            // finalize ui rendering
            UserInterface.Active.DrawMainRenderTarget(_spriteBatch);

            // call base draw function
            base.Draw(gameTime);
        }



    }
}
