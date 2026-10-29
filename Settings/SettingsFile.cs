using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Settings
{
    public class SettingsFile
    {
        public SettingsFile(string appName, string settingsName)
        {
            AppName = appName;
            SettingsName = settingsName;
        }
        public string AppName { get; }
        public string SettingsName { get; }

        public virtual Dictionary<string, string> Load()
        {
            try
            {
                using var file = File.OpenRead(SettingsPath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(file) ?? new Dictionary<string, string>();
            }
            catch (FileNotFoundException)
            {
                // Nothing to worry about, file just doesn't exist.
            }

            return new Dictionary<string, string>();
        }

        public virtual void Save(Dictionary<string, string> settings)
        {
            using var file = File.Create(SettingsPath);
            JsonSerializer.Serialize(file, settings);
        }

        private static string SettingsDir => LibLala.Constants.LALADIR;
        private string SettingsPath => Path.Combine(SettingsDir, $"{SettingsName}.json");
    }
}
