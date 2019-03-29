using Qml.Net;

namespace dotnet
{
    [Signal("volumeChanged")]
    public class Config
    {
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