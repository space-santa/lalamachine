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
            get => Layout["libraryTopShelveHeight"];
            set => SetLayoutValue("libraryTopShelveHeight", value);
        }


        public LibSettings()
        {
            isActive = false;
            Layout = new Dictionary<string, int>();
            Layout["libraryTopShelveHeight"] = 0;
            try
            {
                using (StreamReader file = File.OpenText(Config.LIB_SETTINGS_PATH))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Layout = (Dictionary<string, int>)serializer.Deserialize(file, typeof(Dictionary<string, int>));
                }
            }
            catch (IOException)
            {
                // Nothing to worry about, file just doesn't exist.
            }
        }

        private void SetLayoutValue(string key, int value)
        {
            Layout[key] = value;
            using (StreamWriter file = File.CreateText(Config.LIB_SETTINGS_PATH))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Layout);
            }
        }

        private Dictionary<string, int> Layout;
    }
}
