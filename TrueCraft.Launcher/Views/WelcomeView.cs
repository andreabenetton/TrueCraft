using System.Diagnostics;
using System.Reflection;
using Iguina.Defs;
using Iguina.Entities;

namespace TrueCraft.Launcher.Views;

/// <summary>
///     Static welcome panel on the left side of the launcher shell. Replaces the
///     old Xwt WebView that embedded the project's updates page — MonoGame has no
///     native web rendering, so the link opens in the user's default browser
///     instead.
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
        parent.AddChild(new Paragraph(_game.UI, $"Launcher build {version}")
        {
            Anchor = Anchor.AutoCenter,
        });

        parent.AddChild(new RowsSpacer(_game.UI));
        parent.AddChild(new Paragraph(_game.UI,
            "An open-source server-grade implementation of Minecraft beta 1.7.3, " +
            "written in modern C#. The launcher signs you in, lets you create or " +
            "join worlds, and hands off to the game client."));

        parent.AddChild(new RowsSpacer(_game.UI));

        var updates = new Button(_game.UI, "View updates") { Anchor = Anchor.AutoCenter };
        updates.Events.OnClick = _ => OpenInBrowser("https://truecraft.io/updates");
        parent.AddChild(updates);

        var source = new Button(_game.UI, "Source on GitHub") { Anchor = Anchor.AutoCenter };
        source.Events.OnClick = _ => OpenInBrowser("https://github.com/ddevault/TrueCraft");
        parent.AddChild(source);
    }

    public void Dispose() { }

    private static void OpenInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Browser launch failures are not fatal.
        }
    }
}
