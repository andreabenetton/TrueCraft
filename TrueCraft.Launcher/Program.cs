using System;
using Serilog;
using TrueCraft.Core;
using TrueCraft;

namespace TrueCraft.Launcher
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var launcherConfig = new LauncherConfiguration();
            _ = new LoggerService<LauncherConfiguration>(launcherConfig.Configuration);
            Log.Information("TrueCraft.Launcher starting");

            try
            {
                UserSettings.Local = UserSettings.Load();
                using (var game = new LauncherGame())
                {
                    game.Run();
                }
                LauncherGame.CancelSessionKeepAlive();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Launcher terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

