using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TrueCraft.Core.Profiling;

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

        /// <summary>
        ///     The process-wide service provider. If a caller resolves before the
        ///     hosting process has bootstrapped one, a minimal fallback container is
        ///     built lazily — useful for unit tests that construct production types
        ///     directly without bootstrapping a real container. Tests that need
        ///     specific registrations should assign a built provider explicitly.
        /// </summary>
        public static IServiceProvider Services
        {
            get => _services ??= BuildFallback();
            set => _services = value;
        }

        private static IServiceProvider BuildFallback()
        {
            var services = new ServiceCollection();
            services.AddLogging(b => b.ClearProviders());
            // Core-level services every consumer can expect to resolve even when no
            // hosting process bootstrapped a richer container (unit tests, ad-hoc tools).
            services.AddSingleton<Profiler>();
            return services.BuildServiceProvider();
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
