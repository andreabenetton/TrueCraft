using System.IO;
using TrueCraft.API.Logging;

namespace TrueCraft.Core.Logging
{
    public class FileLogProvider : ILogProvider
    {
        public FileLogProvider(StreamWriter stream,
            LogCategory enabledCategories = LogCategory.Notice | LogCategory.Warning | LogCategory.Error)
        {
            Stream = stream;
            EnabledCategories = enabledCategories;
        }

        public StreamWriter Stream { get; set; }
        public LogCategory EnabledCategories { get; set; }

        public void Log(LogCategory category, string text, params object[] parameters)
        {
            if ((EnabledCategories & category) != 0) Stream.WriteLine(text, parameters);
        }
    }
}