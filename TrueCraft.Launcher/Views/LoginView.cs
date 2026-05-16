using Iguina.Entities;

namespace TrueCraft.Launcher.Views;

/// <summary>
///     Placeholder Iguina LoginView during the GeonBit→Iguina migration.
///     Real form (username/password/validators, BeginLogin/HTTP, LoginOffline)
///     to be ported in a follow-up commit.
/// </summary>
public sealed class LoginView : ILauncherView
{
    private readonly LauncherGame _game;

    public LoginView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Sign in"));
        parent.AddChild(new HorizontalLine(_game.UI));
        parent.AddChild(new Paragraph(_game.UI, "TODO: migrate LoginView to Iguina"));
    }

    public void Dispose() { }
}
