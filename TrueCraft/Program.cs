using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;
using TrueCraft.Core.TerrainGen;
using TrueCraft.API.Server;
using System.IO;
using TrueCraft.Commands;
using TrueCraft.API.World;
using System;
using TrueCraft.API;
using TrueCraft.Core.Profiling;
using TrueCraft.Options;

namespace TrueCraft
{
    public class Program
    {
        public static NodeConfiguration NodeConfiguration;

        // Resolved per-use so the property is safe to read before/after App.Services init
        // (Program's static field initializers run before Main, before App.Services is set).
        private static ILogger Log => App.LoggerFor<Program>();
        private static Profiler Profiler => App.Services.GetRequiredService<Profiler>();
        private static MultiplayerServer Server => App.Services.GetRequiredService<MultiplayerServer>();
        private static CommandManager CommandManager => App.Services.GetRequiredService<CommandManager>();

        // Signaled by Ctrl-C / SIGINT to release the awaitable shutdown hold in Main.
        private static readonly TaskCompletionSource ShutdownSignal =
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public static async Task Main(string[] args)
        {
            NodeConfiguration = new NodeConfiguration();

            var services = new ServiceCollection();
            services.AddSerilogLogging(NodeConfiguration.Configuration);
            services.AddSingleton(NodeConfiguration);
            services.AddSingleton<IConfiguration>(NodeConfiguration.Configuration);

            services.AddOptions<NodeOptions>()
                .Bind(NodeConfiguration.Configuration.GetSection(NodeOptions.SectionName));
            services.AddOptions<DebugOptions>()
                .Bind(NodeConfiguration.Configuration.GetSection(DebugOptions.SectionName));
            services.AddOptions<ProfilerOptions>()
                .Bind(NodeConfiguration.Configuration.GetSection(ProfilerOptions.SectionName));
            services.AddOptions<AccessOptions>()
                .Bind(NodeConfiguration.Configuration.GetSection(AccessOptions.SectionName));

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

            var buckets = NodeConfiguration.Debug?.Profiler?.Buckets?.Split(',');
            if (buckets != null)
            {
                foreach (var bucket in buckets)
                {
                    Profiler.EnableBucket(bucket.Trim());
                }
            }

            if (NodeConfiguration.Debug.DeleteWorldOnStartup)
            {
                if (Directory.Exists("world"))
                    Directory.Delete("world", true);
            }
            if (NodeConfiguration.Debug.DeletePlayersOnStartup)
            {
                if (Directory.Exists("players"))
                    Directory.Delete("players", true);
            }
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
            Server.Start(new IPEndPoint(IPAddress.Parse(NodeConfiguration.ServerAddress), NodeConfiguration.ServerPort));
            Console.CancelKeyPress += HandleCancelKeyPress;
            Server.Scheduler.ScheduleEvent("world.save", null,
                TimeSpan.FromSeconds(NodeConfiguration.WorldSaveInterval),
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
                TimeSpan.FromSeconds(NodeConfiguration.WorldSaveInterval),
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
