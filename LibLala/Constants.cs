using System;
using System.IO;

namespace LibLala
{
    public static class Constants
    {
        public static string LALADIR { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lalamachine"); }
        public static string SETTINGSPATH { get => Path.Combine(LALADIR, "settings.json"); }
        public static string LIBSETTINGSPATH { get => Path.Combine(LALADIR, "libsettings.json"); }
        public static string CONFIGPATH { get => Path.Combine(LALADIR, "config.json"); }
        public static string DBPATH { get => Path.Combine(LALADIR, "musiclib.db"); }

        public const string MISCPLAYLISTNAME = "cs1m090";

        public const string ALL = "-- all --";

        public static CultureInfo CULTURE { get => new CultureInfo("en-AU"); }
    }
}
