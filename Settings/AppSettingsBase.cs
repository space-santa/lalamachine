using System;
using System.Collections.Generic;

namespace Settings
{
    public class AppSettings : AppSettingsBase<SettingsFile>
    {
        public AppSettings(string appName, string settingsName) : base(appName, settingsName) { }
    }

    public abstract class AppSettingsBase<T> where T : ISettingsFile, new()
    {
        private Dictionary<string, string> _settings;

        public AppSettingsBase(string appName, string settingsName)
        {
            SettingsFile = new T
            {
                AppName = appName,
                SettingsName = settingsName
            };
            _settings = new Dictionary<string, string>();
            _settings = SettingsFile.Load();
        }

        ISettingsFile SettingsFile { get; set; }

        public void Save()
        {
            SettingsFile.Save(_settings);
        }

        protected void Set(string key, int value)
        {
            Set(key, value.ToString());
        }

        protected void Set(string key, double value)
        {
            Set(key, value.ToString());
        }

        protected void Set(string key, bool value)
        {
            Set(key, value.ToString());
        }

        protected void Set(string key, string value)
        {
            _settings[key] = value;
        }

        protected int GetInt(string key, int initial_value)
        {
            try
            {
                return Convert.ToInt32(_settings[key]);
            }
            catch (KeyNotFoundException)
            {
                return initial_value;
            }
        }

        protected double GetDouble(string key, double initial_value)
        {
            try
            {
                return Convert.ToDouble(_settings[key]);
            }
            catch (KeyNotFoundException)
            {
                return initial_value;
            }
        }

        protected bool GetBool(string key, bool initial_value)
        {
            try
            {
                return Convert.ToBoolean(_settings[key]);
            }
            catch (KeyNotFoundException)
            {
                return initial_value;
            }
        }

        protected string GetString(string key)
        {
            return _settings[key];
        }
    }
}
