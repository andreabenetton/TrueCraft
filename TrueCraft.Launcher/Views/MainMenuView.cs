using Xwt;

namespace TrueCraft.Launcher.Views
{
    public class MainMenuView : VBox
    {
        public MainMenuView(LauncherWindow window)
        {
            Window = window;
            MinWidth = 250;

            WelcomeText = new Label("Welcome, " + Window.User.Username)
            {
                TextAlignment = Alignment.Center
            };
            SingleplayerButton = new Button("Singleplayer");
            MultiplayerButton = new Button("Multiplayer");
            OptionsButton = new Button("Options");
            QuitButton = new Button("Quit Game");

            SingleplayerButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.SingleplayerView);
            };
            MultiplayerButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.MultiplayerView);
            };
            OptionsButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                window.InteractionBox.PackEnd(Window.OptionView);
            };
            QuitButton.Clicked += (sender, e) => Application.Exit();

            PackStart(WelcomeText);
            PackStart(SingleplayerButton);
            PackStart(MultiplayerButton);
            PackStart(OptionsButton);
            PackEnd(QuitButton);
        }

        public LauncherWindow Window { get; set; }

        public Label WelcomeText { get; set; }
        public Button SingleplayerButton { get; set; }
        public Button MultiplayerButton { get; set; }
        public Button OptionsButton { get; set; }
        public Button QuitButton { get; set; }
    }
}