using Qml.Net;
using System.Collections.Generic;
using PlaylistProvider;
using System.IO;
using System.Linq;

namespace Lalamachine
{
    [Signal("playlistNamesChanged")]
    public class PlaylistProvider
    {
        public string[] playlistNames
        {
            get
            {
                return Playlist.getAllNames().Where(x => x != Constants.MISC_PLAYLIST_NAME).ToArray();
            }
        }

        public PlaylistProvider()
        {
        }

        public void writePlaylist(string name, string json)
        {
            try
            {
                var playlist = Playlist.FromJson(json);
                playlist.save(name);
                this.ActivateSignal("playlistNamesChanged");
            }
            catch (System.ArgumentNullException)
            {
                // Probably an empty playlist the wants to be saved on close.
            }
        }

        public string readPlaylist(string name)
        {
            try
            {
                return Playlist.load(name).ToJson();
            }
            catch (FileNotFoundException)
            {
                return "{}";
            }
        }

        public string playlistPath(string name)
        {
            return Playlist.getPath(name);
        }

        public void deletePlaylist(string name)
        {
            File.Delete(Playlist.getPath(name));
            this.ActivateSignal("playlistNamesChanged");
        }

        public void addTracksToNamedPlaylist(string listName, string[] trackStrings)
        {
            var playlist = Playlist.load(listName);
            foreach (string trackString in trackStrings)
            {
                Track track = Track.FromJson(trackString);
                playlist.Add(track);
            }
            playlist.save(listName);
        }
    }
}