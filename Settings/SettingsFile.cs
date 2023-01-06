using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

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
                using (StreamReader file = File.OpenText(SettingsPath))
                {
                    var serializer = new JsonSerializer();
                    var fileAsDict = (Dictionary<string, string>?)serializer.Deserialize(file, typeof(Dictionary<string, string>));

                    if (fileAsDict is null)
                    {
                        return new Dictionary<string, string>();
                    }
                    else
                    {
                        return fileAsDict;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // Nothing to worry about, file just doesn't exist.
            }

            return new Dictionary<string, string>();
        }

        public virtual void Save(Dictionary<string, string> settings)
        {
            using (var file = File.CreateText(SettingsPath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, settings);
            }
        }

        private static string SettingsDir => LibLala.Constants.LALADIR;
        private string SettingsPath => Path.Combine(SettingsDir, $"{SettingsName}.json");
    }
}
