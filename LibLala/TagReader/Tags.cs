using System;
using System.Collections.Generic;
using System.Linq;
using LibLala.DomainPrimitives;
using Newtonsoft.Json;

namespace LibLala.LibLalaTagReader
{
    public class LibLalaTags
    {
        private readonly Album _album;
        private ArtistsOfTrack _artist;
        private readonly Comment _comment;
        private GenresOfTrack _genre;

        public LibLalaTags(string title, string path)
        {
            _artist = new ArtistsOfTrack(new List<string>());
            _genre = new GenresOfTrack(new List<string>());
            _comment = new Comment("");
            _album = new Album("");
            Title = title;
            Path = path;
        }
        public LibLalaTags(string title, string path, List<string> genre, List<string> artist, string album, string comment)
        {
            _artist = new ArtistsOfTrack(artist);
            _genre = new GenresOfTrack(genre);
            _comment = new Comment(comment);
            _album = new Album(album);
            Title = title;
            Path = path;
        }

        public LibLalaTags(LibLalaTags other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(paramName: nameof(other));
            }

            _artist = new ArtistsOfTrack(other.Artist);
            _genre = new GenresOfTrack(other.Genre);
            _album = new Album(other.Album);
            _comment = new Comment(other.Comment);
            DiscNumber = other.DiscNumber;
            Duration = other.Duration;
            Title = other.Title;
            Track = other.Track;
            Year = other.Year;
            Path = other.Path;
            TrackId = other.TrackId;
        }

        public LibLalaTags(TagLib.File file, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(paramName: nameof(path));
            }

            if (file is null)
            {
                throw new ArgumentNullException(paramName: nameof(file));
            }

            Path = path;
            _album = new Album(file.Tag.Album);
            var x = file.Tag.AlbumArtists;
            var y = file.Tag.Performers;

            var z = new string[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);

            _artist = new ArtistsOfTrack(z.ToList());
            _comment = new Comment(file.Tag.Comment);
            DiscNumber = file.Tag.Disc;
            _genre = new GenresOfTrack(file.Tag.Genres.ToList());
            Duration = file.Properties.Duration;
            Title = file.Tag.Title;
            Track = file.Tag.Track;
            Year = file.Tag.Year;
        }

        public bool IsValid => Title != null && Title.Length > 0 && Length > 0;

        public List<string> Artist
        {
            get => _artist.ToStringList();
        }
        public string ArtistString
        {
            get => _artist.ToCsvString();
            set => _artist = new ArtistsOfTrack(value);
        }

        public List<string> Genre { get => _genre.ToStringList(); }
        public string GenreString
        {
            get => _genre.ToCsvString();
            set => _genre = new GenresOfTrack(value);
        }

        public TimeSpan Duration { get; set; }

        public int Length
        {
            get => (int)Duration.TotalSeconds;
            set => Duration = new TimeSpan(0, 0, value);
        }

        public string LengthString
        {
            get
            {
                var seconds = Duration.Seconds;
                var minutesValue = (int)Duration.TotalMinutes;
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
        public string Comment { get => _comment.ToString(); }
        public string Album { get => _album.ToString(); }
        public uint Year { get; set; }
        public uint DiscNumber { get; set; }
        public int TrackId { get; set; }
        public string Path { get; set; }

        public string ToJson()
        {
            var output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}
