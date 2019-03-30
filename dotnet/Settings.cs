using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnet
{
    public class Settings
    {
        public int x { get => Layout["x"]; set => SetLayoutValue("x", value); }
        public int y { get => Layout["y"]; set => SetLayoutValue("y", value); }
        public int height { get => Layout["h"]; set => SetLayoutValue("h", value); }
        public int width { get => Layout["w"]; set => SetLayoutValue("w", value); }
        public int libraryTopShelveHeight
        {
            get => Layout["libraryTopShelveHeight"];
            set => SetLayoutValue("libraryTopShelveHeight", value);
        }


        public Settings()
        {
            Layout = new Dictionary<string, int>();
            try
            {
                using (StreamReader file = File.OpenText(Config.SETTINGS_PATH))
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
            using (StreamWriter file = File.CreateText(Config.SETTINGS_PATH))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Layout);
            }
        }

        private Dictionary<string, int> Layout;
    }
}
