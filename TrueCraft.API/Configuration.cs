using System.IO;
using System.Text.Json;

namespace TrueCraft.API
{
    /// <summary>
    ///     Abstract base class for configurations read from JSON files.
    /// </summary>
    public abstract class Configuration
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        /// <summary>
        ///     Creates and returns a new configuration read from a JSON file.
        /// </summary>
        /// <typeparam name="T">The configuration type.</typeparam>
        /// <param name="configFileName">The path to the JSON file.</param>
        /// <returns></returns>
        public static T LoadConfiguration<T>(string configFileName) where T : new()
        {
            T config;

            if (File.Exists(configFileName))
            {
                using var file = File.OpenRead(configFileName);
                config = JsonSerializer.Deserialize<T>(file, SerializerOptions) ?? new T();
            }
            else
            {
                config = new T();
            }

            using (var writer = File.Create(configFileName))
            {
                JsonSerializer.Serialize(writer, config, SerializerOptions);
            }

            return config;
        }
    }
}
