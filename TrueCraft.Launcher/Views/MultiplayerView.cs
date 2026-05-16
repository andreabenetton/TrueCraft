using Iguina.Entities;

namespace TrueCraft.Launcher.Views;

public sealed class MultiplayerView : ILauncherView
{
    private readonly LauncherGame _game;

    public MultiplayerView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Multiplayer"));
        parent.AddChild(new HorizontalLine(_game.UI));
        parent.AddChild(new Paragraph(_game.UI, "TODO: migrate MultiplayerView to Iguina"));
    }

    public void Dispose() { }
}
