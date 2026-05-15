using Microsoft.Extensions.Configuration;
using TrueCraft.API;

namespace TrueCraft
{
    public class DebugConfiguration : BaseConfiguration
    {
        public DebugConfiguration(IConfiguration configuration) : base(configuration)
        {
            Initialize();
        }

        private void Initialize()
        {
            IConfiguration section = ConfigurationHolder.GetSection("debug");
            DeleteWorldOnStartup = section.GetValue("deleteWorldOnStartup", false);
            DeletePlayersOnStartup = section.GetValue("deletePlayersOnStartup", false);
            Profiler = new ProfilerConfiguration(ConfigurationHolder);
        }

        public bool DeleteWorldOnStartup { get; set; }
        public bool DeletePlayersOnStartup { get; set; }
        public ProfilerConfiguration Profiler { get; set; }
    }
}
