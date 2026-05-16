using Iguina.Entities;

namespace TrueCraft.Launcher.Views;

public sealed class MainMenuView : ILauncherView
{
    private readonly LauncherGame _game;

    public MainMenuView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, $"Welcome, {_game.User.Username}"));
        parent.AddChild(new HorizontalLine(_game.UI));
        parent.AddChild(new Paragraph(_game.UI, "TODO: migrate MainMenuView to Iguina"));
    }

    public void Dispose() { }
}
