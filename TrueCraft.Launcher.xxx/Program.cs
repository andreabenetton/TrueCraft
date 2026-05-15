using System;

namespace TrueCraft.Launcher
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new LauncherGame())
                game.Run();
        }
    }
}
