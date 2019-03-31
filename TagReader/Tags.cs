using System;
using System.Linq;
using Newtonsoft.Json;

namespace TagReader
{
    public class Tags
    {
        public bool isValid()
        {
            return title != null && title.Length > 0 && length > 0;
        }
        public string album;
        public string[] artist;
        public string artistString
        {
            get
            {
                return JoinArrayWithComma(artist.Distinct().ToArray());
            }
            set
            {
                var localArtist = value.Split(',');

                for (int i = 0; i < localArtist.Length; ++i)
                {
                    localArtist[i] = localArtist[i].Trim();
                }

                artist = localArtist.Distinct().ToArray();
            }
        }
        public string comment;
        public string[] genre;
        public string genreString
        {
            get
            {
                return JoinArrayWithComma(genre);
            }
            set
            {
                genre = value.Split(',');
            }
        }
        public string title;
        public uint track;
        public uint year;
        public uint discNumber;
        public TimeSpan duration;

        public int length
        {
            get
            {
                return (int)duration.TotalSeconds;
            }
            set
            {
                duration = new TimeSpan(0, 0, value);
            }
        }

        public string lengthString
        {
            get
            {
                int seconds = duration.Seconds;
                int minutesValue = (int)duration.TotalMinutes;
                string secondsString = $"{seconds}";
                if (secondsString.Length == 1) {
                    secondsString = $"0{secondsString}";
                }
                return $"{minutesValue}:{secondsString}";
            }
        }
        public string path;

        private string JoinArrayWithComma(string[] arr)
        {
            try
                {
                    return string.Join(", ", arr);
                }
                catch(System.ArgumentNullException)
                {
                    return "";
                }
        }

        public string ToJson()
        {
            string output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}