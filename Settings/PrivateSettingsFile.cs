using System;
using System.IO;

namespace Settings
{
    // For settings with a secret in them. On macOS and Linux a new file can be read by everyone, this one only by the user.
    // On Windows nothing else is needed, the user's app data folder is theirs.
    public class PrivateSettingsFile : SettingsFile
    {
        public PrivateSettingsFile(string appName, string settingsName) : base(appName, settingsName)
        {
        }

        public PrivateSettingsFile(string appName, string settingsName, string settingsDir) : base(appName, settingsName, settingsDir)
        {
        }

        protected override FileStream CreateFile(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                return base.CreateFile(path);
            }

            return new FileStream(path, new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite,
            });
        }
    }
}
