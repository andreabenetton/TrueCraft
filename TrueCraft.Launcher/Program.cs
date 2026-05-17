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

namespace TrueCraft.Launcher;

public static class Program
{
    // Resolved per-use so it's safe to read before/after App.Services init.
    private static ILogger Log => App.LoggerFor("TrueCraft.Launcher.Program");

    [STAThread]
    public static void Main(string[] args)
    {
        App.EnableBootstrapLogger();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("launchersettings.json", optional: false, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSerilogLogging(configuration);
        services.AddSingleton<IConfiguration>(configuration);

        // Launcher runs an embedded singleplayer server — bake the singleplayer
        // overrides into NodeOptions directly. No file source needed since
        // launchersettings.json has no Configuration section.
        services.AddOptions<NodeOptions>()
            .Configure(opts =>
            {
                opts.Singleplayer = true;
                opts.Query = false;
                opts.MOTD = null;
                // The TrueCraft.Client process does its own lighting; the
                // embedded server doesn't need to. Disabling avoids the
                // ChunkLoaded → WorldLighting.GenerateHeightMap cascade that
                // re-enters World.GetChunk for every neighbour block during
                // the second-launch chunk-load loop.
                opts.EnableLighting = false;
            })
            .ValidateDataAnnotations();
        services.AddOptions<DebugOptions>().ValidateDataAnnotations();
        services.AddOptions<ProfilerOptions>().ValidateDataAnnotations();
        services.AddOptions<AccessOptions>().ValidateDataAnnotations();

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
        services.AddSingleton<TrueCraft.Handlers.LoginHandlers>();
        // Transient so each concurrent singleplayer session gets its own
        // MultiplayerServer instance with its own port + world list.
        // The launcher's only ctor-level consumer is SingleplayerServer;
        // LoginHandlers receives the server as a method argument from the
        // server's own packet pipeline, not via DI.
        services.AddTransient<MultiplayerServer>();
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
