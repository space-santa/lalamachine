using Qml.Net;
using System;
using System.IO;

namespace dotnet
{
    [Signal("volumeChanged")]
    public class Config
    {
        public static string LALADIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lalamachine");
        public static string SETTINGS_PATH = Path.Combine(LALADIR, "settings.json");
        public static string LIB_SETTINGS_PATH = Path.Combine(LALADIR, "libsettings.json");

        public double volume { get; set; }
        public string lastPlaylist { get; set; }
        public string libPath { get; set; }
        public string[] playlistColumns { get; set; }

        public Config()
        {
            volume = 0.5;
            lastPlaylist = "";
            libPath = "";
            playlistColumns = new string[9];
        }

        public static void CreateLaladir()
        {
            Directory.CreateDirectory(LALADIR);
        }
    }
}