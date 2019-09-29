using System;
using System.Collections.Generic;
using System.Linq;
using LibLala.DomainPrimitives;
using Newtonsoft.Json;

namespace LibLala.LibLalaTagReader
{
    public class LibLalaTags
    {
        private readonly AlbumName _album;
        private readonly ArtistsOfTrack _artist;
        private readonly Comment _comment;
        private readonly DiscNumber _discNumber;
        private readonly GenresOfTrack _genre;
        private readonly TitleName _title;
        private readonly TrackNumber _trackNumber;
        private readonly TrackPath _trackPath;
        private readonly Year? _year;

        public LibLalaTags(string title, string path)
        {
            _artist = new ArtistsOfTrack(new List<string>());
            _genre = new GenresOfTrack(new List<string>());
            _comment = new Comment("");
            _album = new AlbumName("");
            _discNumber = new DiscNumber(1);
            _trackNumber = new TrackNumber(1);
            _title = new TitleName(title);
            _trackPath = new TrackPath(path);
        }
        internal LibLalaTags(LibLalaTagsBuilder builder)
        {
            _album = builder.Album;
            _artist = builder.Artist;
            _comment = builder.Comment;
            _discNumber = builder.DiscNumber;
            _genre = builder.Genre;
            _title = builder.Title;
            TrackId = builder.TrackId;
            _trackNumber = builder.TrackNumber;
            _trackPath = builder.TrackPath;
            _year = builder.Year;
        }

        public LibLalaTags(LibLalaTags other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(paramName: nameof(other));
            }

            _artist = new ArtistsOfTrack(other.Artist);
            _genre = new GenresOfTrack(other.Genre);
            _album = new AlbumName(other.Album);
            _comment = new Comment(other.Comment);
            _discNumber = new DiscNumber(other.DiscNumber);
            Duration = other.Duration;
            _title = new TitleName(other.Title);
            _trackNumber = new TrackNumber(other.Track);

            if (other.Year is { }) { _year = new Year(other.Year.Value); };

            _trackPath = new TrackPath(other.Path);
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

            _trackPath = new TrackPath(path);
            _album = new AlbumName(file.Tag.Album);
            var x = file.Tag.AlbumArtists;
            var y = file.Tag.Performers;

            var z = new string[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);

            _artist = new ArtistsOfTrack(z.ToList());
            _comment = new Comment(file.Tag.Comment);
            _discNumber = new DiscNumber(file.Tag.Disc);
            _genre = new GenresOfTrack(file.Tag.Genres.ToList());
            Duration = file.Properties.Duration;
            _title = new TitleName(file.Tag.Title);
            _trackNumber = new TrackNumber(file.Tag.Track);
            _year = new Year(file.Tag.Year);
        }

        public bool IsValid => Title != null && Title.Length > 0 && Length > 0;

        public string Album => _album.ToString();

        public List<string> Artist => _artist.ToStringList();
        public string ArtistString => _artist.ToCsvString();

        public string Comment => _comment.ToString();
        public uint DiscNumber => _discNumber.Value;

        // TODO: Duration should be a domain primitive, no?
        public TimeSpan Duration { get; set; }

        public List<string> Genre => _genre.ToStringList();
        public string GenreString => _genre.ToCsvString();

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

        public string Title => _title.ToString();
        public uint Track => _trackNumber.Value;
        public int? TrackId { get; }
        public string Path => _trackPath.FullName;

        public uint? Year => _year?.Value;

        public string ToJson()
        {
            var output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}
