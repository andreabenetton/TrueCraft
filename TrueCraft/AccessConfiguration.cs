using System;
using Microsoft.Extensions.Configuration;
using TrueCraft.API;

namespace TrueCraft
{
    public class AccessConfiguration : BaseConfiguration, IAccessConfiguration
    {
        public AccessConfiguration() : base("nodesettings.json")
        {
            IConfiguration section = ConfigurationHolder.GetSection("Access");
            Blacklist = section.GetValue("blacklist", Array.Empty<string>());
            Whitelist = section.GetValue("whitelist", Array.Empty<string>());
            Oplist = section.GetValue("ops", Array.Empty<string>());
        }

        public string[] Blacklist { get; }
        public string[] Whitelist { get; }
        public string[] Oplist { get; }
    }
}
