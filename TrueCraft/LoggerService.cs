using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace TrueCraft
{
    public class LoggerService<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        readonly Microsoft.Extensions.Logging.ILogger _logger;

        public LoggerService(IConfiguration config)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
            string loggerName = Assembly.GetExecutingAssembly().FullName + "/" + typeof(T).FullName;
            Log.Information(@"{1} logger created.", loggerName);

            using (var logfactory = new SerilogLoggerFactory(Log.Logger))
                _logger = logfactory.CreateLogger(loggerName);
        }

        IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state) =>
            _logger.BeginScope<TState>(state);

        bool Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel logLevel) =>
            _logger.IsEnabled(logLevel);

        void Microsoft.Extensions.Logging.ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log<TState>(logLevel, eventId, state, exception, formatter);
    }
}
