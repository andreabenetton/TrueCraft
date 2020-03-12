using Newtonsoft.Json;
using TrueCraft.API;

namespace TrueCraft
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerConfiguration : Configuration
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class DebugConfiguration
        {
            [JsonObject(MemberSerialization.OptIn)]
            public class ProfilerConfiguration
            {
                public ProfilerConfiguration()
                {
                    Buckets = "";
                }

                [JsonProperty(PropertyName = "buckets")]
                public string Buckets { get; set; }

                [JsonProperty(PropertyName = "lag")]
                public bool Lag { get; set; }
            }

            public DebugConfiguration()
            {
                DeleteWorldOnStartup = false;
                DeletePlayersOnStartup = false;
            }

            [JsonProperty(PropertyName = "deleteWorldOnStartup")]
            public bool DeleteWorldOnStartup { get; set; }

            [JsonProperty(PropertyName = "deletePlayersOnStartup")]
            public bool DeletePlayersOnStartup { get; set; }

            [JsonProperty(PropertyName = "profiler")]
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

        [JsonProperty(PropertyName = "motd")]
        public string MOTD { get; set; }

        [JsonProperty(PropertyName = "bind-port")]
        public int ServerPort {get; set; }

        [JsonProperty(PropertyName = "bind-endpoint")]
        public string ServerAddress { get; set; }

        [JsonProperty(PropertyName = "debug")]
        public DebugConfiguration Debug { get; set; }

        [JsonProperty(PropertyName = "save-interval")]
        public int WorldSaveInterval { get; set; }

        public bool Singleplayer { get; set; }

        [JsonProperty(PropertyName = "query-enabled")]
        public bool Query { get; set; }

        [JsonProperty(PropertyName = "query-port")]
        public int QueryPort { get; set; }

        [JsonProperty(PropertyName = "enable-lighting")]
        public bool EnableLighting { get; set; }

        [JsonProperty(PropertyName = "enable-event-loading")]
        public bool EnableEventLoading { get; set; }

        [JsonProperty(PropertyName = "disable-events")]
        public string[] DisabledEvents { get; set; }
    }
}