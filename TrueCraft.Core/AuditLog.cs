using Microsoft.Extensions.Configuration;
using Serilog;

namespace TrueCraft
{
    /// <summary>
    ///     Security-relevant events (joins, leaves, ops actions, chat) routed to a
    ///     dedicated Serilog logger so they survive even when the diagnostic stream
    ///     is set to Warning. Configured from the <c>Audit</c> section of the
    ///     host process's JSON settings; if absent, falls back to
    ///     <see cref="Serilog.Core.Logger.None"/> and audit events are dropped.
    /// </summary>
    public static class AuditLog
    {
        private static Serilog.ILogger _logger = Serilog.Core.Logger.None;

        /// <summary>
        ///     Reads the <c>Audit</c> section of the supplied configuration and builds
        ///     the audit logger. Called once from <see cref="ServiceCollectionLoggingExtensions.AddSerilogLogging"/>.
        /// </summary>
        public static void Configure(IConfiguration configuration)
        {
            var section = configuration.GetSection("Audit");
            if (!section.Exists())
                return;

            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration, new Serilog.Settings.Configuration.ConfigurationReaderOptions
                {
                    SectionName = "Audit",
                })
                .CreateLogger();
        }

        public static void PlayerJoined(string player, string endpoint) =>
            _logger.Information("Player {Player} joined from {Endpoint}", player, endpoint);

        public static void PlayerLeft(string player) =>
            _logger.Information("Player {Player} left", player);

        public static void Chat(string player, string message) =>
            _logger.Information("Chat from {Player}: {Message}", player, message);
    }
}
