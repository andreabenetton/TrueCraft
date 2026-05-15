using Microsoft.Extensions.Configuration;
using TrueCraft.API;

namespace TrueCraft.Launcher
{
    public class LauncherConfiguration : BaseConfiguration
    {
        public LauncherConfiguration(string fileName = "launchersettings.json") : base(fileName)
        {
            Initialize();
        }

        public LauncherConfiguration(IConfiguration configuration) : base(configuration)
        {
            Initialize();
        }

        private void Initialize()
        {

        }
    }
}
