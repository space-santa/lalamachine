using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnet
{
    public class LibSettings
    {
        public bool isActive { get; set; }
        public int libraryTopShelveHeight
        {
            get => _LibSettings["libraryTopShelveHeight"];
            set => SetLayoutValue("libraryTopShelveHeight", value);
        }


        public LibSettings()
        {
            isActive = false;
            _LibSettings = new Dictionary<string, int>();
            _LibSettings["libraryTopShelveHeight"] = 0;
            try
            {
                using (StreamReader file = File.OpenText(Config.LIB_SETTINGS_PATH))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _LibSettings = (Dictionary<string, int>)serializer.Deserialize(file, typeof(Dictionary<string, int>));
                }
            }
            catch (DirectoryNotFoundException)
            {
                Config.CreateLaladir();
            }
            catch (FileNotFoundException)
            {
                // Nothing to worry about, file just doesn't exist.
            }
        }

        private void SetLayoutValue(string key, int value)
        {
            _LibSettings[key] = value;
            using (StreamWriter file = File.CreateText(Config.LIB_SETTINGS_PATH))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _LibSettings);
            }
        }

        private Dictionary<string, int> _LibSettings;
    }
}
