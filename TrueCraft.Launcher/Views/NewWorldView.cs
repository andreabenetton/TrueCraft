using System;
using Iguina.Entities;
using TrueCraft.Core.World;

namespace TrueCraft.Launcher.Views;

public sealed class NewWorldView : ILauncherView
{
    private readonly LauncherGame _game;
    private readonly Action<World> _onCreated;

    public NewWorldView(LauncherGame game, Action<World> onCreated)
    {
        _game = game;
        _onCreated = onCreated;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "New world"));
        parent.AddChild(new HorizontalLine(_game.UI));
        parent.AddChild(new Paragraph(_game.UI, "TODO: migrate NewWorldView to Iguina"));
    }

    public void Dispose() { }
}
