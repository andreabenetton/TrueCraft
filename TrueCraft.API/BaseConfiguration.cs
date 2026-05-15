using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace TrueCraft.API
{
    /// <summary>
    ///     Abstract base class for configurations read from JSON files.
    /// </summary>
    public abstract class BaseConfiguration
    {
        protected readonly IConfiguration ConfigurationHolder;

        /// <summary>
        ///     Creates and returns a new configuration read from a JSON file.
        /// </summary>
        /// <param name="configFileName">The path to the JSON file.</param>
        /// <returns></returns>
        public BaseConfiguration(string configFileName)
        {
            ConfigurationHolder = new ConfigurationBuilder()
                .SetBasePath(AssemblyDirectory)
                .AddJsonFile(configFileName, optional: false, reloadOnChange: false)
                .Build();
        }

        public BaseConfiguration(IConfiguration configuration)
        {
            ConfigurationHolder = configuration;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetEntryAssembly().Location;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public IConfiguration Configuration => ConfigurationHolder;
    }
}
