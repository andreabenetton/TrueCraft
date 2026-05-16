using System;
using Microsoft.Extensions.Configuration;

namespace TrueCraft.Options
{
    /// <summary>
    ///     Strongly-typed bindings for the <c>Configuration</c> section of
    ///     <c>nodesettings.json</c>. JSON keys that use kebab-case are mapped via
    ///     <see cref="ConfigurationKeyNameAttribute"/>.
    /// </summary>
    public sealed class NodeOptions
    {
        public const string SectionName = "Configuration";

        public string MOTD { get; set; } = "Welcome to TrueCraft!";

        [ConfigurationKeyName("bind-port")]
        public int ServerPort { get; set; } = 25565;

        [ConfigurationKeyName("bind-endpoint")]
        public string ServerAddress { get; set; } = "0.0.0.0";

        [ConfigurationKeyName("save-interval")]
        public int WorldSaveInterval { get; set; } = 30;

        public bool Singleplayer { get; set; } = false;

        [ConfigurationKeyName("query-enabled")]
        public bool Query { get; set; } = true;

        [ConfigurationKeyName("query-port")]
        public int QueryPort { get; set; } = 25566;

        [ConfigurationKeyName("enable-lighting")]
        public bool EnableLighting { get; set; } = true;

        [ConfigurationKeyName("enable-event-loading")]
        public bool EnableEventLoading { get; set; } = true;

        [ConfigurationKeyName("disable-events")]
        public string[] DisabledEvents { get; set; } = Array.Empty<string>();
    }
}
