using Qml.Net;
using System.Collections.Generic;

namespace dotnet
{
    public class PlaylistProvider
    {
        public List<string> playlistNames { get; set; }

        public PlaylistProvider()
        {
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