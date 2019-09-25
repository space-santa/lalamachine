﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace LibLala.TagReader
{
    public class Tags
    {
        public Tags(string title, string path)
        {
            _artist = new string[0];
            genre = new string[0];
            Comment = "";
            Album = "";
            Title = title;
            Path = path;
        }

        public Tags(Tags other)
        {
            _artist = new string[0];
            genre = new string[0];
            Album = other.Album;
            Artist = other.Artist;
            Comment = other.Comment;
            DiscNumber = other.DiscNumber;
            genre = other.genre;
            duration = other.duration;
            Title = other.Title;
            Track = other.Track;
            Year = other.Year;
            Path = other.Path;
            TrackId = other.TrackId;
        }

        public Tags(TagLib.File file, string path)
        {
            Path = path;
            Album = file.Tag.Album;
            var x = file.Tag.AlbumArtists;
            var y = file.Tag.Performers;

            var z = new string[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);

            _artist = new string[0];
            Artist = z;
            Comment = file.Tag.Comment;
            DiscNumber = file.Tag.Disc;
            genre = file.Tag.Genres;
            duration = file.Properties.Duration;
            Title = file.Tag.Title;
            Track = file.Tag.Track;
            Year = file.Tag.Year;
        }

        public bool isValid()
        {
            return Title != null && Title.Length > 0 && length > 0;
        }

        private string[] _artist;
        public string[] Artist
        {
            get => _artist;
            set
            {
                for (var i = 0; i < value.Length; ++i)
                {
                    value[i] = value[i].Trim();
                }

                _artist = value.Distinct().ToArray();
            }
        }
        public string ArtistString
        {
            get => JoinArrayWithComma(_artist.Distinct().ToArray());
            set
            {
                var localArtist = value.Split(',');

                for (var i = 0; i < localArtist.Length; ++i)
                {
                    localArtist[i] = localArtist[i].Trim();
                }

                _artist = localArtist.Distinct().ToArray();
            }
        }

        public string[] genre;
        public string GenreString
        {
            get => JoinArrayWithComma(genre);
            set => genre = value.Split(',');
        }

        public TimeSpan duration;

        public int length
        {
            get => (int)duration.TotalSeconds;
            set => duration = new TimeSpan(0, 0, value);
        }

        public string LengthString
        {
            get
            {
                var seconds = duration.Seconds;
                var minutesValue = (int)duration.TotalMinutes;
                var secondsString = $"{seconds}";
                if (secondsString.Length == 1)
                {
                    secondsString = $"0{secondsString}";
                }
                return $"{minutesValue}:{secondsString}";
            }
        }

        public string Title { get; set; }
        public uint Track { get; set; }
        public string Comment { get; set; }
        public string Album { get; set; }
        public uint Year { get; set; }
        public uint DiscNumber { get; set; }
        public int TrackId { get; set; }
        public string Path { get; set; }

        private string JoinArrayWithComma(string[] arr)
        {
            try
            {
                return string.Join(", ", arr);
            }
            catch (ArgumentNullException)
            {
                return "";
            }
        }

        public string ToJson()
        {
            var output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}
