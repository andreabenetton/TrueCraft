using Microsoft.Extensions.Configuration;
using Serilog;
using TrueCraft.API;

namespace TrueCraft.Launcher
{
    /// <summary>
    ///     Strongly-typed launcher configuration, read from <c>launchersettings.json</c>
    ///     next to the executable. Wires Serilog's global <see cref="Log.Logger"/> from
    ///     the <c>Serilog</c> section of that file via
    ///     <see cref="Serilog.Settings.Configuration"/>.
    /// </summary>
    public sealed class LauncherConfiguration : BaseConfiguration
    {
        public const string SettingsFileName = "launchersettings.json";

        public LauncherConfiguration() : base(SettingsFileName)
        {
            Initialize();
        }

        public LauncherConfiguration(IConfiguration configuration) : base(configuration)
        {
            Initialize();
        }

        private void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(ConfigurationHolder)
                .CreateLogger();
        }
    }
}
