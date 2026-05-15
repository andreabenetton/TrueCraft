using Microsoft.Extensions.Configuration;
using TrueCraft.API;

namespace TrueCraft
{
    public class ProfilerConfiguration : BaseConfiguration
    {
        public ProfilerConfiguration(IConfiguration configuration) : base(configuration)
        {
            Initialize();
        }

        private void Initialize()
        {
            IConfiguration section = ConfigurationHolder.GetSection("profiler");
            Buckets = section.GetValue("buckets", "");
            Lag = section.GetValue("lag", false);
        }

        public string Buckets { get; set; }
        public bool Lag { get; set; }
    }
}
