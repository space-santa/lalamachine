using Qml.Net;
using System;
using System.IO;

namespace dotnet
{
    [Signal("volumeChanged")]
    public class Config
    {
        public static string LALADIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lalamachine");
        public double volume { get; set; }
        public string lastPlaylist { get; set; }
        public string libPath { get; set; }
        public INetJsValue playlistColumns { get; set; }

        public Config()
        {
            volume = 0.5;
            lastPlaylist = "";
            libPath = "";
        }
    }
}