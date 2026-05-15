using System;
using Serilog;
using TrueCraft.Core;

namespace TrueCraft.Launcher
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Constructing LauncherConfiguration wires Serilog from launchersettings.json.
            _ = new LauncherConfiguration();
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

