using Iguina.Entities;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Views;

public sealed class SingleplayerView : ILauncherView
{
    private readonly LauncherGame _game;

    public SingleplayerView(LauncherGame game)
    {
        _game = game;
        // World list still loads behind the scenes; UI to mount it pending Iguina port.
        Worlds.Local ??= Microsoft.Extensions.DependencyInjection.ActivatorUtilities
            .CreateInstance<Worlds>(TrueCraft.App.Services);
        Worlds.Local.Load();
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Singleplayer"));
        parent.AddChild(new HorizontalLine(_game.UI));
        parent.AddChild(new Paragraph(_game.UI, "TODO: migrate SingleplayerView to Iguina"));
    }

    public void Dispose() { }
}
