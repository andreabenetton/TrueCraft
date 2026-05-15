using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using TrueCraft.Launcher.Entities;

namespace TrueCraft.Launcher.Panels
{
    public class MainMenuPanel: BackgroundManagingPanel
    {
        private int panelWidth = 520;

        private LauncherGame _game;
        private Label _welcomeText;
        private Button _loginButton;
        private Button _singlePlayerButton;
        private Button _multiPlayerButton;
        private Button _optionsButton;
        private Button _quitButton;

        public EventCallback OnLoginSelected = null;
        public EventCallback OnSinglePlayerSelected = null;
        public EventCallback OnMultiPlayerSelected = null;
        public EventCallback OnOptionsSelected = null;
        public EventCallback OnQuitSelected = null;

        public MainMenuPanel(LauncherGame game) : base(game)
        {
            Size = new Vector2(panelWidth, panelWidth);

            _game = game;
            
            _welcomeText = new Label("Welcome, " + _game.User.Username)
            {
                AlignToCenter = true
            };

            _loginButton = new MenuButton("Login");
            _singlePlayerButton = new MenuButton("Single Player");
            _multiPlayerButton = new MenuButton("Multi Player");
            _optionsButton = new MenuButton("Options", Anchor.Auto, new Vector2(225,60));
            _quitButton = new MenuButton("Quit Game", Anchor.AutoInline, new Vector2(225, 60), new Vector2(10,0));
            _singlePlayerButton.Enabled = _multiPlayerButton.Enabled = false;

            _loginButton.OnClick += _ =>
            {
                OnLoginSelected?.Invoke(this);
            };
            _singlePlayerButton.OnClick += _ =>
            {
                OnSinglePlayerSelected?.Invoke(this);
            };
            _multiPlayerButton.OnClick += _ =>
            {
                OnMultiPlayerSelected?.Invoke(this);
            };
            _optionsButton.OnClick += _ =>
            {
                OnOptionsSelected?.Invoke(this);
            };
            _quitButton.OnClick += _ =>
            {
                OnQuitSelected?.Invoke(this);
            };

            // Add controls
            AddChild(new Header("Menu"));
            AddChild(new HorizontalLine());
            AddChild(_welcomeText);
            AddChild(_loginButton);
            AddChild(_singlePlayerButton);
            AddChild(_multiPlayerButton);
            AddChild(_optionsButton);
            AddChild(_quitButton);
        }

        public void ActivatePlayButton()
        {
            _singlePlayerButton.Enabled = _multiPlayerButton.Enabled =  true;
            _welcomeText.Text = "Welcome, " + _game.User.Username;
        }
    }
}
