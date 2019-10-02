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
        private readonly ArtistList _artist;
        private readonly Comment _comment;
        private readonly DiscNumber _discNumber;
        private readonly GenreList _genre;
        private readonly TrackLength _length;
        private readonly TitleName _title;
        private readonly TrackNumber _trackNumber;
        private readonly Year _year;

        internal LibLalaTags(LibLalaTagsBuilder builder)
        {
            _album = builder.Album;
            _artist = builder.Artist;
            _comment = builder.Comment;
            _discNumber = builder.DiscNumber;
            _genre = builder.Genre;
            _length = builder.Length;
            _title = builder.Title;
            TrackId = builder.TrackId;
            _trackNumber = builder.TrackNumber;
            TrackPath = builder.TrackPath;
            _year = builder.Year;
        }

        public LibLalaTags(LibLalaTags other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(paramName: nameof(other));
            }

            _artist = new ArtistList(other.Artist);
            _genre = new GenreList(other.Genre);
            _album = new AlbumName(other.Album);
            _comment = new Comment(other.Comment);
            _discNumber = new DiscNumber(other.DiscNumber);
            _length = new TrackLength(other.Length);
            _title = new TitleName(other.Title);
            _trackNumber = new TrackNumber(other.Track);

            _year = new Year(other.Year);

            TrackPath = other.TrackPath;
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

            TrackPath = new TrackPath(path);
            _album = new AlbumName(file.Tag.Album ?? "");
            var x = file.Tag.AlbumArtists;
            var y = file.Tag.Performers;

            var z = new string[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);

            _artist = new ArtistList(z.ToList());
            _comment = new Comment(file.Tag.Comment ?? "");
            _discNumber = new DiscNumber(file.Tag.Disc);
            _genre = new GenreList(file.Tag.Genres.ToList());
            _length = new TrackLength(file.Properties.Duration);
            _title = new TitleName(file.Tag.Title);
            _trackNumber = new TrackNumber(file.Tag.Track);
            _year = new Year((int)file.Tag.Year);
        }

        public bool IsValid => Title != null && Title.Length > 0 && Length > 0;

        public string Album => _album.ToString();

        public List<string> Artist => _artist.ToStringList();
        public string ArtistString => _artist.ToCsvString();

        public string Comment => _comment.ToString();
        public uint DiscNumber => _discNumber.Value;

        public List<string> Genre => _genre.ToStringList();
        public string GenreString => _genre.ToCsvString();

        public int Length => _length.TotalSeconds;
        public string LengthString => _length.ToString();

        public string Title => _title.ToString();
        public uint Track => _trackNumber.Value;
        public int? TrackId { get; }
        public string Path => TrackPath.FullName;
        internal ITrackPath TrackPath { get; }

        public int Year => _year.Value;

        public string ToJson()
        {
            var output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}
