using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Settings
{
    public class SettingsFile
    {
        public string AppName { get; set; }
        public string SettingsName { get; set; }

        public virtual Dictionary<string, string> Load()
        {
            try
            {
                using (var file = File.OpenText(SettingsPath))
                {
                    var serializer = new JsonSerializer();
                    return (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));
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

        private string SettingsDir => LibLala.Constants.LALADIR;
        private string SettingsPath => Path.Combine(SettingsDir, $"{SettingsName}.json");
    }
}
