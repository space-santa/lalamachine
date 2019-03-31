using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnet
{
    public class Constants
    {
        public static string LALADIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lalamachine");
        public static string SETTINGS_PATH = Path.Combine(LALADIR, "settings.json");
        public static string LIB_SETTINGS_PATH = Path.Combine(LALADIR, "libsettings.json");
        public static string CONFIG_PATH = Path.Combine(LALADIR, "config.json");

        public static string MISC_PLAYLIST_NAME = "cs1m090";
    }
}
