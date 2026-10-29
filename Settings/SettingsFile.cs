using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Settings
{
    public class SettingsFile
    {
        public SettingsFile(string appName, string settingsName) : this(appName, settingsName, LibLala.Constants.LALADIR)
        {
        }

        public SettingsFile(string appName, string settingsName, string settingsDir)
        {
            AppName = appName;
            SettingsName = settingsName;
            SettingsDir = settingsDir;
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
            catch (JsonException)
            {
                // A corrupt file must not stop the app from starting, fall back to the defaults.
            }

            return new Dictionary<string, string>();
        }

        public virtual void Save(Dictionary<string, string> settings)
        {
            // Write a temporary file first, so a crash mid-write can't leave a truncated settings file behind.
            var tempPath = $"{SettingsPath}.tmp";
            using (var file = File.Create(tempPath))
            {
                JsonSerializer.Serialize(file, settings);
            }
            File.Move(tempPath, SettingsPath, overwrite: true);
        }

        private string SettingsDir { get; }
        private string SettingsPath => Path.Combine(SettingsDir, $"{SettingsName}.json");
    }
}
