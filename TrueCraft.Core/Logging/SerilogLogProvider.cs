using Serilog;
using Serilog.Events;
using TrueCraft.API.Logging;

namespace TrueCraft.Core.Logging
{
    /// <summary>
    ///     ILogProvider backend that delegates to the global Serilog logger
    ///     (<see cref="Serilog.Log"/>). Maps <see cref="LogCategory"/> values to Serilog
    ///     levels; filtering is delegated to Serilog's MinimumLevel configuration.
    /// </summary>
    public sealed class SerilogLogProvider : ILogProvider
    {
        public void Log(LogCategory category, string text, params object[] parameters)
        {
            // Existing callers use positional placeholders ("{0}" / "{1}"), not Serilog's
            // named templates. Pre-format with string.Format so the template Serilog sees
            // is the final message — we lose structured properties but keep the legacy
            // call-site semantics.
            var message = parameters is { Length: > 0 } ? string.Format(text, parameters) : text;
            Serilog.Log.Write(MapLevel(category), "{Message}", message);
        }

        private static LogEventLevel MapLevel(LogCategory category) => category switch
        {
            LogCategory.Packets => LogEventLevel.Verbose,
            LogCategory.Debug => LogEventLevel.Debug,
            LogCategory.Notice => LogEventLevel.Information,
            LogCategory.Warning => LogEventLevel.Warning,
            LogCategory.Error => LogEventLevel.Error,
            _ => LogEventLevel.Information,
        };
    }
}
