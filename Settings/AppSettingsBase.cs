using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Settings
{
    public class AppSettings : AppSettingsBase
    {
        public AppSettings(string appName, string settingsName) : base(new SettingsFile(appName, settingsName))
        { }
    }

    public class AppSettingsBase
    {
        private readonly Dictionary<string, string> _settings;

        public AppSettingsBase(SettingsFile settingsFile)
        {
            ArgumentNullException.ThrowIfNull(settingsFile);

            SettingsFile = settingsFile;
            _settings = SettingsFile.Load();
        }

        private SettingsFile SettingsFile { get; set; }

        public void Save()
        {
            SettingsFile.Save(_settings);
        }

        protected void Set(int value, [CallerMemberName] string key = "defaultInt")
        {
            Set(value.ToString(LibLala.Constants.CULTURE), key);
        }

        protected void Set(double value, [CallerMemberName] string key = "defaultDouble")
        {
            Set(value.ToString(LibLala.Constants.CULTURE), key);
        }

        protected void Set(bool value, [CallerMemberName] string key = "defaultBool")
        {
            Set(value.ToString(LibLala.Constants.CULTURE), key);
        }

        protected void Set(string value, [CallerMemberName] string key = "defaultString")
        {
            // Sliders call this many times per second, only touch the file when something changed.
            if (_settings.TryGetValue(key, out var current) && current == value) { return; }

            _settings[key] = value;
            Save();
        }

        protected int GetInt(int initialValue = 0, [CallerMemberName] string key = "defaultInt")
        {
            if (_settings.TryGetValue(key, out var value) && int.TryParse(value, NumberStyles.Integer, LibLala.Constants.CULTURE, out var result))
            {
                return result;
            }

            return initialValue;
        }

        protected double GetDouble(double initialValue = 0.0, [CallerMemberName] string key = "defaultDouble")
        {
            if (_settings.TryGetValue(key, out var value) && double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, LibLala.Constants.CULTURE, out var result))
            {
                return result;
            }

            return initialValue;
        }

        protected bool GetBool(bool initialValue = false, [CallerMemberName] string key = "defaultBool")
        {
            if (_settings.TryGetValue(key, out var value) && bool.TryParse(value, out var result))
            {
                return result;
            }

            return initialValue;
        }

        protected string GetString(string initialValue = "", [CallerMemberName] string key = "defaultString")
        {
            return _settings.TryGetValue(key, out var value) ? value : initialValue;
        }
    }
}
