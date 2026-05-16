using System;
using TrueCraft.API;

namespace TrueCraft.Options
{
    /// <summary>
    ///     Strongly-typed bindings for the <c>Access</c> section of <c>nodesettings.json</c>.
    ///     Implements <see cref="IAccessConfiguration"/> so the public
    ///     <c>IMultiplayerServer.AccessConfiguration</c> contract is preserved.
    /// </summary>
    public sealed class AccessOptions : IAccessConfiguration
    {
        public const string SectionName = "Access";

        public string[] Blacklist { get; set; } = [];
        public string[] Whitelist { get; set; } = [];

        [Microsoft.Extensions.Configuration.ConfigurationKeyName("ops")]
        public string[] Oplist { get; set; } = [];
    }
}
