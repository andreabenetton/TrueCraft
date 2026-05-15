using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            var services = new ServiceCollection();
            services.AddSerilogLogging(launcherConfig.Configuration);
            services.AddSingleton(launcherConfig);
            services.AddSingleton<IConfiguration>(launcherConfig.Configuration);
            App.Services = services.BuildServiceProvider();

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
