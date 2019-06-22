using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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
                using (StreamReader file = File.OpenText(SettingsPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));
                }
            }
            catch (DirectoryNotFoundException)
            {
                CreateSettingsdir();
            }
            catch (FileNotFoundException)
            {
                // Nothing to worry about, file just doesn't exist.
            }

            return new Dictionary<string, string>();
        }

        public virtual void Save(Dictionary<string, string> settings)
        {
            using (StreamWriter file = File.CreateText(SettingsPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, settings);
            }
        }

        private string SettingsDir { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName); }
        private string SettingsPath { get => Path.Combine(SettingsDir, $"{SettingsName}.json"); }

        private void CreateSettingsdir()
        {
            Directory.CreateDirectory(SettingsDir);
        }
    }
}
