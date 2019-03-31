using Newtonsoft.Json;
using Qml.Net;
using System;
using System.Collections.Generic;
using System.IO;

namespace dotnet
{
    [Signal("volumeChanged")]
    public class Config
    {
        public static string LALADIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lalamachine");
        public static string SETTINGS_PATH = Path.Combine(LALADIR, "settings.json");
        public static string LIB_SETTINGS_PATH = Path.Combine(LALADIR, "libsettings.json");
        public static string CONFIG_PATH = Path.Combine(LALADIR, "config.json");

        public double volume
        {
            get => Convert.ToDouble(_Config["volume"]);
            set => SetConfigValue("volume", value);
        }

        public string lastPlaylist
        {
            get => _Config["lastPlaylist"];
            set => SetConfigValue("lastPlaylist", value);
        }

        public string libPath
        {
            get => _Config["libPath"];
            set => SetConfigValue("libPath", value);
        }

        public string playlistColumns
        {
            get => _Config["playlistColumns"];
            set => SetConfigValue("playlistColumns", value);
        }

        public Config()
        {
            Init();

            try
            {
                Load();
            }
            catch (DirectoryNotFoundException)
            {
                CreateLaladir();
            }
            catch (FileNotFoundException)
            {
                // Nothing to worry about, file just doesn't exist.
            }
        }

        private void Init()
        {
            _Config = new Dictionary<string, string>();
            _Config["volume"] = "0.5";
            _Config["lastPlaylist"] = "";
            _Config["libPath"] = "";
            _Config["playlistColumns"] = @"
                [
                    { ""key"": ""track"", ""value"": true },
                    { ""key"": ""discNumber"", ""value"": true },
                    { ""key"": ""title"", ""value"": true },
                    { ""key"": ""comment"", ""value"": true },
                    { ""key"": ""length"", ""value"": true },
                    { ""key"": ""genre"", ""value"": true },
                    { ""key"": ""album"", ""value"": true },
                    { ""key"": ""artist"", ""value"": true },
                    { ""key"": ""year"", ""value"": true }
                ]";
        }

        private void Load()
        {
            using (StreamReader file = File.OpenText(dotnet.Config.CONFIG_PATH))
            {
                JsonSerializer serializer = new JsonSerializer();
                _Config = (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));
            }
        }

        public static void CreateLaladir()
        {
            Directory.CreateDirectory(LALADIR);
        }

        private void SetConfigValue(string key, double value)
        {
            SetConfigValue(key, $"{value}");
        }

        private void SetConfigValue(string key, string value)
        {
            _Config[key] = value;
            using (StreamWriter file = File.CreateText(dotnet.Config.CONFIG_PATH))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _Config);
            }
        }

        private Dictionary<string, string> _Config;
    }
}