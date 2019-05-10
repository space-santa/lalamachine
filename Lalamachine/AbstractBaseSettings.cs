using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lalamachine
{
    public abstract class AbstractBaseSettings
    {
        public AbstractBaseSettings()
        {
            _Settings = new Dictionary<string, string>();
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

        protected abstract void Init();

        protected abstract string targetPath();

        private void Load()
        {
            using (StreamReader file = File.OpenText(targetPath()))
            {
                JsonSerializer serializer = new JsonSerializer();
                _Settings = (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));
            }
        }

        private static void CreateLaladir()
        {
            Directory.CreateDirectory(LibLala.Constants.LALADIR);
        }

        protected void SetSettingsValue(string key, int value)
        {
            SetSettingsValue(key, value.ToString());
        }

        protected void SetSettingsValue(string key, double value)
        {
            SetSettingsValue(key, value.ToString());
        }

        protected void SetSettingsValue(string key, string value)
        {
            _Settings[key] = value;
            using (StreamWriter file = File.CreateText(targetPath()))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _Settings);
            }
        }

        protected void InitSettingsValue(string key, int value)
        {
            InitSettingsValue(key, value.ToString());
        }

        protected void InitSettingsValue(string key, double value)
        {
            InitSettingsValue(key, value.ToString());
        }

        protected void InitSettingsValue(string key, string value)
        {
            _Settings[key] = value;

        }

        protected int GetIntValue(string key)
        {
            return Convert.ToInt32(_Settings[key]);
        }

        protected double GetDoubleValue(string key)
        {
            return Convert.ToDouble(_Settings[key]);
        }

        protected string GetStringValue(string key)
        {
            return _Settings[key];
        }

        private Dictionary<string, string> _Settings;
    }
}
