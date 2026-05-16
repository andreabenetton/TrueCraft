using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Server;
using TrueCraft.API.World;
using TrueCraft.Commands;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Profiling;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.World;
using TrueCraft.Options;

namespace TrueCraft
{
    public class Program
    {
        // Resolved per-use so the property is safe to read before/after App.Services init
        // (Program's static field initializers run before Main, before App.Services is set).
        private static ILogger Log => App.LoggerFor<Program>();
        private static Profiler Profiler => App.Services.GetRequiredService<Profiler>();
        private static MultiplayerServer Server => App.Services.GetRequiredService<MultiplayerServer>();
        private static CommandManager CommandManager => App.Services.GetRequiredService<CommandManager>();
        private static NodeOptions Node => App.Services.GetRequiredService<IOptions<NodeOptions>>().Value;

        // Signaled by Ctrl-C / SIGINT to release the awaitable shutdown hold in Main.
        private static readonly TaskCompletionSource ShutdownSignal =
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public static async Task Main(string[] args)
        {
            App.EnableBootstrapLogger();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("nodesettings.json", optional: false, reloadOnChange: false)
                .Build();

            var services = new ServiceCollection();
            services.AddSerilogLogging(configuration);
            services.AddSingleton<IConfiguration>(configuration);

            services.AddOptions<NodeOptions>()
                .Bind(configuration.GetSection(NodeOptions.SectionName))
                .ValidateDataAnnotations();
            services.AddOptions<DebugOptions>()
                .Bind(configuration.GetSection(DebugOptions.SectionName))
                .ValidateDataAnnotations();
            services.AddOptions<ProfilerOptions>()
                .Bind(configuration.GetSection(ProfilerOptions.SectionName))
                .ValidateDataAnnotations();
            services.AddOptions<AccessOptions>()
                .Bind(configuration.GetSection(AccessOptions.SectionName))
                .ValidateDataAnnotations();

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
            services.AddSingleton<CommandManager>();
            App.Services = services.BuildServiceProvider();

            // Force eager validation: any DataAnnotations violation in nodesettings.json
            // throws OptionsValidationException here rather than silently misbehaving later.
            _ = App.Services.GetRequiredService<IOptions<NodeOptions>>().Value;
            _ = App.Services.GetRequiredService<IOptions<AccessOptions>>().Value;
            var debug = App.Services.GetRequiredService<IOptions<DebugOptions>>().Value;
            var profilerOpts = App.Services.GetRequiredService<IOptions<ProfilerOptions>>().Value;

            var buckets = profilerOpts.Buckets?.Split(',');
            if (buckets != null)
            {
                foreach (var bucket in buckets)
                    Profiler.EnableBucket(bucket.Trim());
            }

            if (debug.DeleteWorldOnStartup && Directory.Exists("world"))
                Directory.Delete("world", true);
            if (debug.DeletePlayersOnStartup && Directory.Exists("players"))
                Directory.Delete("players", true);

            IWorld world;
            try
            {
                world = await World.LoadWorldAsync("world");
                Server.AddWorld(world);
            }
            catch
            {
                world = new World("default", new StandardGenerator());
                world.BlockRepository = Server.BlockRepository;
                await world.SaveAsync("world");
                Server.AddWorld(world);
                Log.LogInformation("Generating world around spawn point...");
                for (int x = -5; x < 5; x++)
                {
                    for (int z = -5; z < 5; z++)
                        world.GetChunk(new Coordinates2D(x, z));
                    int progress = (int)(((x + 5) / 10.0) * 100);
                    if (progress % 10 == 0)
                        Log.LogInformation("{Progress}% complete", progress + 10);
                }
                Log.LogInformation("Simulating the world for a moment...");
                for (int x = -5; x < 5; x++)
                {
                    for (int z = -5; z < 5; z++)
                    {
                        var chunk = world.GetChunk(new Coordinates2D(x, z));
                        for (byte _x = 0; _x < Chunk.Width; _x++)
                        {
                            for (byte _z = 0; _z < Chunk.Depth; _z++)
                            {
                                for (int _y = 0; _y < chunk.GetHeight(_x, _z); _y++)
                                {
                                    var coords = new Coordinates3D(x + _x, _y, z + _z);
                                    var data = world.GetBlockData(coords);
                                    var provider = world.BlockRepository.GetBlockProvider(data.ID);
                                    provider.BlockUpdate(data, data, Server, world);
                                }
                            }
                        }
                    }
                    int progress = (int)(((x + 5) / 10.0) * 100);
                    if (progress % 10 == 0)
                        Log.LogInformation("{Progress}% complete", progress + 10);
                }
                Log.LogInformation("Lighting the world (this will take a moment)...");
                foreach (var lighter in Server.WorldLighters)
                {
                    while (lighter.TryLightNext()) ;
                }
            }
            await world.SaveAsync();
            Server.ChatMessageReceived += HandleChatMessageReceived;
            Server.Start(new IPEndPoint(IPAddress.Parse(Node.ServerAddress), Node.ServerPort));
            Console.CancelKeyPress += HandleCancelKeyPress;
            Server.Scheduler.ScheduleEvent("world.save", null,
                TimeSpan.FromSeconds(Node.WorldSaveInterval),
                (Func<IMultiplayerServer, Task>)SaveWorldsAsync);

            // Park here until SIGINT (HandleCancelKeyPress) signals shutdown. Replaces the previous
            // `while (true) Thread.Yield();` spin so the main thread doesn't burn a core idling.
            await ShutdownSignal.Task.ConfigureAwait(false);
        }

        static async Task SaveWorldsAsync(IMultiplayerServer server)
        {
            Log.LogInformation("Saving world...");
            try
            {
                foreach (var w in Server.Worlds)
                    await w.SaveAsync().ConfigureAwait(false);
                Log.LogInformation("Done.");
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "World save failed");
            }
            server.Scheduler.ScheduleEvent("world.save", null,
                TimeSpan.FromSeconds(Node.WorldSaveInterval),
                (Func<IMultiplayerServer, Task>)SaveWorldsAsync);
        }

        static void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // suppress immediate process termination so we can shut down cleanly
            Server.Stop();
            ShutdownSignal.TrySetResult();
        }

        static void HandleChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            var message = e.Message;

            if (!message.StartsWith("/") || message.StartsWith("//"))
                SendChatMessage(e.Client.Username, message);
            else
                e.PreventDefault = ProcessChatCommand(e);
        }

        private static void SendChatMessage(string username, string message)
        {
            if (message.StartsWith("//"))
                message = message.Substring(1);

            Server.SendMessage("<{0}> {1}", username, message);
        }

        /// <summary>
        /// Parse sent message as chat command
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true if the command was successfully executed</returns>
        private static bool ProcessChatCommand(ChatMessageEventArgs e)
        {
            var commandWithoutSlash = e.Message.TrimStart('/');
            var messageArray = commandWithoutSlash
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (messageArray.Length <= 0) return false; // command not found

            var alias = messageArray[0];
            var trimmedMessageArray = new string[messageArray.Length - 1];
            if (trimmedMessageArray.Length != 0)
                Array.Copy(messageArray, 1, trimmedMessageArray, 0, messageArray.Length - 1);

            CommandManager.HandleCommand(e.Client, alias, trimmedMessageArray);

            return true;
        }
    }
}
