using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PlaylistProvider
{
    public class Playlist : List<Track>
    {
        public static Playlist FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Playlist>(json);
        }

        public string ToJson()
        {
            string output = JsonConvert.SerializeObject(this);
            return output;
        }

        public void save(string name)
        {
            using (StreamWriter file = File.CreateText(getPath(name)))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }

        public static Playlist load(string name)
        {
            using (StreamReader file = File.OpenText(getPath(name)))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (Playlist)serializer.Deserialize(file, typeof(Playlist));
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

        public static string[] getAllNames()
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
                return names.ToArray();
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(playlistDir);
            }

            return new string[0];
        }
    }
}
