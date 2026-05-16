using System.Reflection;
using Iguina.Entities;

namespace TrueCraft.Launcher.Views;

/// <summary>
///     Static welcome panel on the left of the launcher shell. Iguina-based.
/// </summary>
public sealed class WelcomeView : ILauncherView
{
    private readonly LauncherGame _game;

    public WelcomeView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "TrueCraft"));
        parent.AddChild(new HorizontalLine(_game.UI));

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "dev";
        parent.AddChild(new Paragraph(_game.UI, $"Launcher build {version}"));

        parent.AddChild(new RowsSpacer(_game.UI));
        parent.AddChild(new Paragraph(_game.UI,
            "An open-source server-grade implementation of Minecraft beta 1.7.3, " +
            "written in modern C#. The launcher signs you in, lets you create or " +
            "join worlds, and hands off to the game client."));
    }

    public void Dispose() { }
}
