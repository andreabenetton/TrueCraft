using System.Collections.Generic;
using System.Text.Json.Serialization;
using TrueCraft.API;

namespace TrueCraft
{
    public class AccessConfiguration : Configuration, IAccessConfiguration
    {
        public AccessConfiguration()
        {
            Blacklist = new List<string>();
            Whitelist = new List<string>();
            Oplist = new List<string>();
        }

        [JsonPropertyName("blacklist")]
        public IList<string> Blacklist { get; private set; }

        [JsonPropertyName("whitelist")]
        public IList<string> Whitelist { get; private set; }

        [JsonPropertyName("ops")]
        public IList<string> Oplist { get; private set; }
    }
}
