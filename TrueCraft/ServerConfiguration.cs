using System.Text.Json.Serialization;
using TrueCraft.API;

namespace TrueCraft
{
    public class ServerConfiguration : Configuration
    {
        public class DebugConfiguration
        {
            public class ProfilerConfiguration
            {
                public ProfilerConfiguration()
                {
                    Buckets = "";
                }

                [JsonPropertyName("buckets")]
                public string Buckets { get; set; }

                [JsonPropertyName("lag")]
                public bool Lag { get; set; }
            }

            public DebugConfiguration()
            {
                DeleteWorldOnStartup = false;
                DeletePlayersOnStartup = false;
            }

            [JsonPropertyName("deleteWorldOnStartup")]
            public bool DeleteWorldOnStartup { get; set; }

            [JsonPropertyName("deletePlayersOnStartup")]
            public bool DeletePlayersOnStartup { get; set; }

            [JsonPropertyName("profiler")]
            public ProfilerConfiguration Profiler { get; set; }
        }

        public ServerConfiguration()
        {
            MOTD = "Welcome to TrueCraft!";
            Debug = new DebugConfiguration();
            ServerPort = 25565;
            ServerAddress = "0.0.0.0";
            WorldSaveInterval = 30;
            Singleplayer = false;
            Query = true;
            QueryPort = 25566;
            EnableLighting = true;
            EnableEventLoading = true;
            DisabledEvents = new string[0];
        }

        [JsonPropertyName("motd")]
        public string MOTD { get; set; }

        [JsonPropertyName("bind-port")]
        public int ServerPort { get; set; }

        [JsonPropertyName("bind-endpoint")]
        public string ServerAddress { get; set; }

        [JsonPropertyName("debug")]
        public DebugConfiguration Debug { get; set; }

        [JsonPropertyName("save-interval")]
        public int WorldSaveInterval { get; set; }

        [JsonIgnore]
        public bool Singleplayer { get; set; }

        [JsonPropertyName("query-enabled")]
        public bool Query { get; set; }

        [JsonPropertyName("query-port")]
        public int QueryPort { get; set; }

        [JsonPropertyName("enable-lighting")]
        public bool EnableLighting { get; set; }

        [JsonPropertyName("enable-event-loading")]
        public bool EnableEventLoading { get; set; }

        [JsonPropertyName("disable-events")]
        public string[] DisabledEvents { get; set; }
    }
}
