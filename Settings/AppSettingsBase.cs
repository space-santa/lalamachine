using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Settings
{
    public class AppSettings : AppSettingsBase
    {
        public AppSettings(string appName, string settingsName) : base(new SettingsFile { AppName = appName, SettingsName = settingsName })
        { }
    }

    public abstract class AppSettingsBase
    {
        private Dictionary<string, string> _settings;

        public AppSettingsBase(SettingsFile settingsFile)
        {
            SettingsFile = settingsFile;
            _settings = SettingsFile.Load();
        }

        SettingsFile SettingsFile { get; set; }

        public void Save()
        {
            SettingsFile.Save(_settings);
        }

        protected void Set(int value, [CallerMemberName] string key = "defaultInt")
        {
            Set(value.ToString(), key);
        }

        protected void Set(double value, [CallerMemberName] string key = "defaultDouble")
        {
            Set(value.ToString(), key);
        }

        protected void Set(bool value, [CallerMemberName] string key = "defaultBool")
        {
            Set(value.ToString(), key);
        }

        protected void Set(string value, [CallerMemberName] string key = "defaultString")
        {
            _settings[key] = value;
            Save();
        }

        protected int GetInt(int initial_value = 0, [CallerMemberName] string key = "defaultInt")
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

        protected double GetDouble(double initial_value = 0.0, [CallerMemberName] string key = "defaultDouble")
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

        protected bool GetBool(bool initial_value = false, [CallerMemberName] string key = "defaultBool")
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

        protected string GetString(string initial_value = "", [CallerMemberName] string key = "defaultString")
        {
            try
            {
                return _settings[key];
            }
            catch (KeyNotFoundException)
            {
                return initial_value;
            }
        }
    }
}
