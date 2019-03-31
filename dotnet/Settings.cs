using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dotnet
{
    public class Settings : AbstractBaseSettings
    {
        public bool isActive { get; set; }
        public int x { get => GetIntValue("x"); set => SetSettingsValue("x", value); }
        public int y { get => GetIntValue("y"); set => SetSettingsValue("y", value); }
        public int height { get => GetIntValue("h"); set => SetSettingsValue("h", value); }
        public int width { get => GetIntValue("w"); set => SetSettingsValue("w", value); }

        protected override void Init()
        {
            isActive = false;
            InitSettingsValue("x", 0);
            InitSettingsValue("y", 0);
            InitSettingsValue("h", 0);
            InitSettingsValue("w", 0);
        }

        protected override string targetPath()
        {
            return Constants.SETTINGS_PATH;
        }
    }
}
