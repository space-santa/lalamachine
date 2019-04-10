using Newtonsoft.Json;
using Qml.Net;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lalamachine
{
    [Signal("playlistColumnsChanged")]
    [Signal("volumeChanged")]
    public class Config : AbstractBaseSettings
    {
        [NotifySignal("volumeChanged")]
        public double volume
        {
            get => GetDoubleValue("volume");
            set => SetSettingsValue("volume", value);
        }

        public string lastPlaylist
        {
            get => GetStringValue("lastPlaylist");
            set => SetSettingsValue("lastPlaylist", value);
        }

        public string libPath
        {
            get => GetStringValue("libPath");
            set => SetSettingsValue("libPath", LalaUtils.Utils.RemoveFilePrefix(value));
        }

        public string playlistColumns
        {
            get => GetStringValue("playlistColumns");
            set
            {
                SetSettingsValue("playlistColumns", value);
                this.ActivateSignal("playlistColumnsChanged");
            }
        }

        protected override void Init()
        {
            InitSettingsValue("volume", 0.5);
            InitSettingsValue("lastPlaylist", "");
            InitSettingsValue("libPath", "");
            InitSettingsValue(
                "playlistColumns", 
                @"[
                    { ""key"": ""track"", ""value"": true },
                    { ""key"": ""discNumber"", ""value"": true },
                    { ""key"": ""title"", ""value"": true },
                    { ""key"": ""comment"", ""value"": true },
                    { ""key"": ""length"", ""value"": true },
                    { ""key"": ""genre"", ""value"": true },
                    { ""key"": ""album"", ""value"": true },
                    { ""key"": ""artist"", ""value"": true },
                    { ""key"": ""year"", ""value"": true }
                ]");
        }

        protected override string targetPath()
        {
            return Constants.CONFIG_PATH;
        }
    }
}