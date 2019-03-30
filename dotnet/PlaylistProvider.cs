using Qml.Net;
using System.Collections.Generic;

namespace dotnet
{
    public class PlaylistProvider
    {
        public string[] playlistNames { get; set; }

        public PlaylistProvider()
        {
            playlistNames = new string[1];
            playlistNames[0] = "default";
        }

        public void writePlaylist(string name, INetJsValue json)
        {

        }

        public INetJsValue readPlaylist(string name)
        {
            return null;
        }

        public string playlistPath(string name)
        {
            return name;
        }

        public void deletePlaylist(string name)
        {

        }

        public void addTrackToNamedPlaylist(string listName, INetJsValue tracks)
        {

        }
    }
}