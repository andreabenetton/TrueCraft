using Iguina.Entities;

namespace TrueCraft.Launcher.Views;

public sealed class OptionView : ILauncherView
{
    private readonly LauncherGame _game;

    public OptionView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Options"));
        parent.AddChild(new HorizontalLine(_game.UI));
        parent.AddChild(new Paragraph(_game.UI, "TODO: migrate OptionView to Iguina"));
    }

    public void Dispose() { }
}
