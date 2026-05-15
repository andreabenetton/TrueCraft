using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace TrueCraft.Launcher
{
    /// <summary>
    ///     Reads <c>launchersettings.json</c> from the application directory and
    ///     configures the global <see cref="Log.Logger"/> from its <c>Serilog</c>
    ///     section. Designed to run once at startup, before <see cref="LauncherGame"/>
    ///     is constructed, so every component logs through the same sinks.
    /// </summary>
    internal static class LauncherConfiguration
    {
        public const string SettingsFileName = "launchersettings.json";

        public static IConfiguration Build()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(typeof(LauncherConfiguration).Assembly.Location)
                             ?? Directory.GetCurrentDirectory())
                .AddJsonFile(SettingsFileName, optional: true, reloadOnChange: false)
                .Build();
        }

        public static void ConfigureSerilog(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
