using System.Collections.Generic;
using Newtonsoft.Json;
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

        [JsonProperty(PropertyName = "blacklist")]
        public IList<string> Blacklist { get; private set; }

        [JsonProperty(PropertyName = "whitelist")]
        public IList<string> Whitelist { get; private set; }
        
        [JsonProperty(PropertyName = "ops")]
        public IList<string> Oplist { get; private set; }
    }
}
