using System;
using TrueCraft.Core;

namespace TrueCraft.Launcher
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            UserSettings.Local = UserSettings.Load();
            using (var game = new LauncherGame())
            {
                game.Run();
            }
            LauncherGame.CancelSessionKeepAlive();
        }
    }
}
