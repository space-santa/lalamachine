using System;
using System.Collections.Generic;
using System.Linq;
using LibLala.DomainPrimitives;
using Newtonsoft.Json;

namespace LibLala.LibLalaTagReader
{
    public class LibLalaTags
    {
        public LibLalaTags(string title, string path)
        {
            _artist = new Artists(new List<string>());
            _genre = new List<string>();
            Comment = "";
            Album = "";
            Title = title;
            Path = path;
        }
        public LibLalaTags(string title, string path, List<string> genre, List<string> artist)
        {
            _artist = new Artists(artist);
            _genre = genre;
            Comment = "";
            Album = "";
            Title = title;
            Path = path;
        }

        public LibLalaTags(LibLalaTags other)
        {
            if (other is null) throw new ArgumentNullException(paramName: nameof(other));

            _artist = new Artists(other.Artist);
            _genre = new List<string>();
            Album = other.Album;
            Comment = other.Comment;
            DiscNumber = other.DiscNumber;
            _genre = other.Genre;
            duration = other.duration;
            Title = other.Title;
            Track = other.Track;
            Year = other.Year;
            Path = other.Path;
            TrackId = other.TrackId;
        }

        public LibLalaTags(TagLib.File file, string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(paramName: nameof(path));
            if (file is null) throw new ArgumentNullException(paramName: nameof(file));

            Path = path;
            Album = file.Tag.Album;
            var x = file.Tag.AlbumArtists;
            var y = file.Tag.Performers;

            var z = new string[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);

            _artist = new Artists(z.ToList());
            Comment = file.Tag.Comment;
            DiscNumber = file.Tag.Disc;
            _genre = file.Tag.Genres.ToList();
            duration = file.Properties.Duration;
            Title = file.Tag.Title;
            Track = file.Tag.Track;
            Year = file.Tag.Year;
        }

        public bool isValid()
        {
            return Title != null && Title.Length > 0 && length > 0;
        }

        private Artists _artist;
        public List<string> Artist
        {
            get => _artist.ToStringList();
        }
        public string ArtistString
        {
            get => _artist.ToCsvString();
            set => _artist = new Artists(value);
        }

        private List<string> _genre;
        public List<string> Genre { get => _genre; }
        public string GenreString
        {
            get => JoinArrayWithComma(Genre.ToArray());
            set
            {
                if (value is null) throw new ArgumentNullException(paramName: nameof(value));
                _genre = value.Split(',').ToList();
            }
        }

        public TimeSpan duration { get; set; }

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

        private static string JoinArrayWithComma(string[] arr)
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
