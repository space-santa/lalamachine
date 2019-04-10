using Newtonsoft.Json;
using Qml.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lalamachine
{
    [Signal("playlistNamesChanged")]
    public class LalaPlaylistProvider
    {
        public string getAllPlaylistNames()
        {
            var names = AllPlaylistNames.Where(x => x != Constants.MISC_PLAYLIST_NAME).ToArray();
            return JsonConvert.SerializeObject(names);
        }

        public List<string> AllPlaylistNames { get; set; }

        public LalaPlaylistProvider()
        {
            AllPlaylistNames = getAllNames();
        }

        public void writePlaylist(string name, string json)
        {
            try
            {
                var playlist = JsonConvert.DeserializeObject<string[]>(json);
                save(name, playlist);
                AllPlaylistNames = getAllNames();
                this.ActivateSignal("playlistNamesChanged");
            }
            catch (ArgumentNullException)
            {
                // Probably an empty playlist the wants to be saved on close.
            }
        }

        public string readPlaylist(string name)
        {
            try
            {
                return JsonConvert.SerializeObject(load(name));
            }
            catch (FileNotFoundException)
            {
                return "[]";
            }
        }

        public string playlistPath(string name)
        {
            return getPath(name);
        }

        public void deletePlaylist(string name)
        {
            File.Delete(getPath(name));
            AllPlaylistNames = getAllNames();
            this.ActivateSignal("playlistNamesChanged");
        }

        public void addTracksToNamedPlaylist(string listName, string[] trackStrings)
        {
            var playlist = load(listName).ToList();

            foreach (string track in trackStrings)
            {
                playlist.Add(track);
            }

            save(listName, playlist.ToArray());
        }

        public void save(string name, string[] files)
        {
            using (StreamWriter file = File.CreateText(getPath(name)))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, files);
            }
        }

        public static string[] load(string name)
        {
            using (StreamReader file = File.OpenText(getPath(name)))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (string[])serializer.Deserialize(file, typeof(string[]));
            }
        }

        public static string getPath(string name)
        {
            if (name.Length < 1)
            {
                throw new ArgumentException("name can't be empty");
            }

            var fileName = Path.Combine(playlistDir, $"{name}.json");
            return fileName;
        }

        public static string playlistDir = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.Create
                ), "lalamachine", "playlists");

        public static List<string> getAllNames()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(playlistDir);
            try
            {
                FileInfo[] files = directoryInfo.GetFiles("*.json", SearchOption.TopDirectoryOnly);

                List<string> names = new List<string>();
                foreach (var file in files)
                {
                    names.Add(Path.GetFileNameWithoutExtension(file.Name));
                }
                return names;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(playlistDir);
            }

            return new List<string>();
        }
    }
}