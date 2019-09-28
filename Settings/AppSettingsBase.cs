using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Settings
{
    public class AppSettings : AppSettingsBase
    {
        public AppSettings(string appName, string settingsName) : base(new SettingsFile(appName, settingsName))
        { }
    }

    public abstract class AppSettingsBase
    {
        private readonly Dictionary<string, string> _settings;

        public AppSettingsBase(SettingsFile settingsFile)
        {
            if (settingsFile is null)
            {
                throw new ArgumentNullException(paramName: nameof(settingsFile));
            }

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
            _settings[key] = value;
            Save();
        }

        protected int GetInt(int initialValue = 0, [CallerMemberName] string key = "defaultInt")
        {
            try
            {
                return Convert.ToInt32(_settings[key], LibLala.Constants.CULTURE);
            }
            catch (KeyNotFoundException)
            {
                return initialValue;
            }
        }

        protected double GetDouble(double initialValue = 0.0, [CallerMemberName] string key = "defaultDouble")
        {
            try
            {
                return Convert.ToDouble(_settings[key], LibLala.Constants.CULTURE);
            }
            catch (KeyNotFoundException)
            {
                return initialValue;
            }
        }

        protected bool GetBool(bool initialValue = false, [CallerMemberName] string key = "defaultBool")
        {
            try
            {
                return Convert.ToBoolean(_settings[key], LibLala.Constants.CULTURE);
            }
            catch (KeyNotFoundException)
            {
                return initialValue;
            }
        }

        protected string GetString(string initialValue = "", [CallerMemberName] string key = "defaultString")
        {
            try
            {
                return _settings[key];
            }
            catch (KeyNotFoundException)
            {
                return initialValue;
            }
        }
    }
}
