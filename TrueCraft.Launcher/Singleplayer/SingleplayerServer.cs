using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrueCraft.API;
using TrueCraft.Core.World;

namespace TrueCraft.Launcher.Singleplayer
{
    public class SingleplayerServer
    {
        private static ILogger Log => App.LoggerFor<SingleplayerServer>();

        public delegate void ProgressNotification(double progress, string stage);

        public SingleplayerServer(World world)
        {
            World = world;
            // Set NodeConfiguration before resolving MultiplayerServer so any handler
            // that reads Program.NodeConfiguration during server init sees the
            // singleplayer overrides.
            var inMemoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Configuration:Singleplayer"] = "true",
                    ["Configuration:query-enabled"] = "false",
                })
                .Build();
            TrueCraft.Program.NodeConfiguration = new NodeConfiguration(inMemoryConfig)
            {
                MOTD = null,
            };
            Server = App.Services.GetRequiredService<MultiplayerServer>();
            world.BlockRepository = Server.BlockRepository;
            Server.AddWorld(world);
        }

        public MultiplayerServer Server { get; set; }
        public World World { get; set; }

        public void Initialize(ProgressNotification progressNotification = null)
        {
            Log.LogInformation("Generating world around spawn point...");
            for (var x = -5; x < 5; x++)
            {
                for (var z = -5; z < 5; z++)
                    World.GetChunk(new Coordinates2D(x, z));
                var progress = (int) ((x + 5) / 10.0 * 100);
                progressNotification?.Invoke(progress / 100.0, "Generating world...");
                if (progress % 10 == 0)
                    Log.LogInformation("{Progress}% complete", progress + 10);
            }

            Log.LogInformation("Simulating the world for a moment...");
            for (var x = -5; x < 5; x++)
            {
                for (var z = -5; z < 5; z++)
                {
                    var chunk = World.GetChunk(new Coordinates2D(x, z));
                    for (byte _x = 0; _x < Chunk.Width; _x++)
                    for (byte _z = 0; _z < Chunk.Depth; _z++)
                    for (var _y = 0; _y < chunk.GetHeight(_x, _z); _y++)
                    {
                        var coords = new Coordinates3D(x + _x, _y, z + _z);
                        var data = World.GetBlockData(coords);
                        var provider = World.BlockRepository.GetBlockProvider(data.ID);
                        provider.BlockUpdate(data, data, Server, World);
                    }
                }

                var progress = (int) ((x + 5) / 10.0 * 100);
                progressNotification?.Invoke(progress / 100.0, "Simulating world...");
                if (progress % 10 == 0)
                    Log.LogInformation("{Progress}% complete", progress + 10);
            }

            World.Save();
        }

        public void Start()
        {
            Server.Start(new IPEndPoint(IPAddress.Loopback, 0));
        }

        public void Stop()
        {
            Server.Stop();
        }
    }
}