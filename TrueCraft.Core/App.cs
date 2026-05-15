using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TrueCraft
{
    /// <summary>
    ///     Process-wide DI container handle. Bootstrap once at process start
    ///     (in <c>Program.Main</c> for server, launcher <c>Program.Main</c> for client);
    ///     resolve everywhere else via <see cref="Services"/>.
    /// </summary>
    public static class App
    {
        private static IServiceProvider _services;

        public static IServiceProvider Services
        {
            get => _services ?? throw new InvalidOperationException(
                "App.Services is not initialized. Call App.Services = services.BuildServiceProvider() at process start.");
            set => _services = value;
        }

        public static ILogger<T> LoggerFor<T>() =>
            Services.GetRequiredService<ILoggerFactory>().CreateLogger<T>();

        public static Microsoft.Extensions.Logging.ILogger LoggerFor(string name) =>
            Services.GetRequiredService<ILoggerFactory>().CreateLogger(name);
    }

    public static class ServiceCollectionLoggingExtensions
    {
        /// <summary>
        ///     Registers a Serilog-backed <see cref="ILoggerFactory"/> and the open-generic
        ///     <see cref="ILogger{T}"/>. The Serilog pipeline is read from the supplied
        ///     <paramref name="configuration"/>'s <c>Serilog</c> section.
        /// </summary>
        public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger, dispose: false);
            });
            return services;
        }
    }
}
