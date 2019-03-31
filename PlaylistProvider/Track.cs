using System;
using Newtonsoft.Json;

namespace PlaylistProvider
{
    public class Track
    {
        public string album = "";
        public string artist = "";
        public string genre = "";
        public string comment = "";
        public string track = "";
        public string title = "";
        public string mrl = "";
        public string path = "";
        public int length = 0;
        public string lengthString = "";
        public int year = 0;
        public int discNumber = 0;
        public int id = 0;

        public static Track FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Track>(json);
        }

        public string ToJson()
        {
            string output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}
