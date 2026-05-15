using System;
using System.Diagnostics;
using System.Reflection;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using TrueCraft.Launcher.Entities;

namespace TrueCraft.Launcher.Views
{
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
            parent.AddChild(new Header("TrueCraft"));
            parent.AddChild(new HorizontalLine());

            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "dev";
            parent.AddChild(new Paragraph($"Launcher build {version}")
            {
                Anchor = Anchor.AutoCenter,
            });

            parent.AddChild(new LineSpace());
            parent.AddChild(new Paragraph(
                "An open-source server-grade implementation of Minecraft beta 1.7.3, " +
                "written in modern C#. The launcher signs you in, lets you create or " +
                "join worlds, and hands off to the game client.")
            {
                Anchor = Anchor.Auto,
            });

            parent.AddChild(new LineSpace());
            var updates = new MenuButton("View updates", anchor: Anchor.AutoCenter);
            updates.OnClick = _ => OpenInBrowser("https://truecraft.io/updates");
            parent.AddChild(updates);

            var source = new MenuButton("Source on GitHub", anchor: Anchor.AutoCenter);
            source.OnClick = _ => OpenInBrowser("https://github.com/ddevault/TrueCraft");
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
}
