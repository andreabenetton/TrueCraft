using System;
using Microsoft.Extensions.Configuration;
using TrueCraft.API;

namespace TrueCraft
{
    public class NodeConfiguration : BaseConfiguration
    {
        public NodeConfiguration(string fileName = "nodesettings.json") : base(fileName)
        {
            Initialize();
        }

        public NodeConfiguration(IConfiguration configuration) : base(configuration)
        {
            Initialize();
        }

        private void Initialize()
        {
            IConfiguration section = ConfigurationHolder.GetSection("Configuration");
            MOTD = section.GetValue("motd", "Welcome to TrueCraft!");

            Debug = new DebugConfiguration(ConfigurationHolder);
            ServerPort = section.GetValue("bind-port", 25565);
            ServerAddress = section.GetValue("bind-endpoint", "0.0.0.0");
            WorldSaveInterval = section.GetValue("save-interval", 30);
            Singleplayer = section.GetValue("Singleplayer", false);
            Query = section.GetValue("query-enabled", true);
            QueryPort = section.GetValue("query-port", 25566);
            EnableLighting = section.GetValue("enable-lighting", true);
            EnableEventLoading = section.GetValue("enable-event-loading", true);
            DisabledEvents = section.GetValue("disable-events", Array.Empty<string>());
        }

        public string MOTD { get; set; }
        public int ServerPort { get; private set; }
        public string ServerAddress { get; private set; }
        public DebugConfiguration Debug { get; set; }
        public int WorldSaveInterval { get; private set; }
        public bool Singleplayer { get; set; }
        public bool Query { get; set; }
        public int QueryPort { get; set; }
        public bool EnableLighting { get; set; }
        public bool EnableEventLoading { get; set; }
        public string[] DisabledEvents { get; set; }
    }
}
