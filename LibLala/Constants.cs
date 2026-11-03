using System;
using System.Globalization;
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

        // The playlist that the hearts of the tracks go by. It is always there, and can't be renamed or deleted.
        public const string LIKEDPLAYLISTNAME = "Liked";

        public const string ALL = "-- all --";

        public static CultureInfo CULTURE { get; } = CultureInfo.GetCultureInfo("en-AU");
    }
}
