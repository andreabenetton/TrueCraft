using System.IO;
using Newtonsoft.Json;

namespace TrueCraft.API
{
    /// <summary>
    ///     Abstract base class for configurations read from JSON files.
    /// </summary>
    public abstract class Configuration
    {
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
                var deserializer = new JsonSerializer();

                using (var file = File.OpenText(configFileName))
                {
                    config = (T) deserializer.Deserialize(file, typeof(T));
                }
            }
            else
            {
                config = new T();
            }

            var serializer = new JsonSerializer();

            using (var writer = new StreamWriter(configFileName))
            {
                serializer.Serialize(writer, config);
            }

            return config;
        }
    }
}