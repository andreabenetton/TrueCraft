using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrueCraft.API.Logic;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Profiling;
using TrueCraft;
using TrueCraft.Options;

namespace TrueCraft.Launcher
{
    public static class Program
    {
        // Resolved per-use so it's safe to read before/after App.Services init.
        private static ILogger Log => App.LoggerFor("TrueCraft.Launcher.Program");

        [STAThread]
        public static void Main(string[] args)
        {
            var launcherConfig = new LauncherConfiguration();

            var services = new ServiceCollection();
            services.AddSerilogLogging(launcherConfig.Configuration);
            services.AddSingleton(launcherConfig);
            services.AddSingleton<IConfiguration>(launcherConfig.Configuration);

            // Launcher runs an embedded singleplayer server — bake the singleplayer
            // overrides into NodeOptions directly. No file source needed since
            // launchersettings.json has no Configuration section.
            services.AddOptions<NodeOptions>().Configure(opts =>
            {
                opts.Singleplayer = true;
                opts.Query = false;
                opts.MOTD = null;
            });
            services.AddOptions<DebugOptions>();
            services.AddOptions<ProfilerOptions>();
            services.AddOptions<AccessOptions>();

            services.AddSingleton<Profiler>();
            services.AddSingleton<IBlockRepository>(_ =>
            {
                var repo = new BlockRepository();
                repo.DiscoverBlockProviders();
                return repo;
            });
            services.AddSingleton<IItemRepository>(_ =>
            {
                var repo = new ItemRepository();
                repo.DiscoverItemProviders();
                return repo;
            });
            services.AddSingleton<ICraftingRepository>(_ =>
            {
                var repo = new CraftingRepository();
                repo.DiscoverRecipes();
                return repo;
            });
            services.AddSingleton<MultiplayerServer>();
            App.Services = services.BuildServiceProvider();

            Log.LogInformation("TrueCraft.Launcher starting");

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
                Log.LogCritical(ex, "Launcher terminated unexpectedly");
                throw;
            }
            finally
            {
                // Serilog-only API; no MEL equivalent.
                Serilog.Log.CloseAndFlush();
            }
        }
    }
}
