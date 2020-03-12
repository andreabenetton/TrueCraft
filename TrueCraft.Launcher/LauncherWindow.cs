using System.Reflection;
using TrueCraft.Core;
using TrueCraft.Launcher.Views;
using Xwt;
using Xwt.Drawing;

namespace TrueCraft.Launcher
{
    public class LauncherWindow : Window
    {
        public LauncherWindow()
        {
            Title = "TrueCraft Launcher";
            Width = 1200;
            Height = 576;
            User = new TrueCraftUser();

            MainContainer = new HBox();
            WebScrollView = new ScrollView();
            WebView = new WebView("https://truecraft.io/updates");
            LoginView = new LoginView(this);
            OptionView = new OptionView(this);
            MultiplayerView = new MultiplayerView(this);
            SingleplayerView = new SingleplayerView(this);
            InteractionBox = new VBox();

            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("TrueCraft.Launcher.Content.truecraft_logo.png"))
            {
                TrueCraftLogoImage = new ImageView(Image.FromStream(stream).WithBoxSize(350, 75));
            }

            WebScrollView.Content = WebView;
            MainContainer.PackStart(WebScrollView, true);
            InteractionBox.PackStart(TrueCraftLogoImage);
            InteractionBox.PackEnd(LoginView);
            MainContainer.PackEnd(InteractionBox);

            Content = MainContainer;
        }

        public TrueCraftUser User { get; set; }

        public HBox MainContainer { get; set; }
        public ScrollView WebScrollView { get; set; }
        public WebView WebView { get; set; }

        public LoginView LoginView { get; set; }
        public MainMenuView MainMenuView { get; set; }
        public OptionView OptionView { get; set; }
        public MultiplayerView MultiplayerView { get; set; }
        public SingleplayerView SingleplayerView { get; set; }
        public VBox InteractionBox { get; set; }
        public ImageView TrueCraftLogoImage { get; set; }

        private void ClientExited()
        {
            Show();
            ShowInTaskbar = true;
        }
    }
}