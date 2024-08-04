using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LibLala.DomainPrimitives;

namespace LibLala.LibLalaTagReader
{
    public class LibLalaTagsBuilder
    {
        public AlbumName Album { get; private set; }
        public ArtistList Artist { get; private set; }
        public Comment Comment { get; private set; }
        public DiscNumber DiscNumber { get; private set; }
        public GenreList Genre { get; private set; }
        public TrackLength Length { get; private set; }
        public TitleName Title { get; }
        public int? TrackId { get; private set; }
        public TrackNumber TrackNumber { get; private set; }
        public ITrackPath TrackPath { get; }
        public Year Year { get; private set; }

        public LibLalaTagsBuilder(string title, string path, int totalSeconds)
        {
            Album = new AlbumName("");
            Artist = new ArtistList(new List<string>());
            Comment = new Comment("");
            DiscNumber = new DiscNumber(1);
            Genre = new GenreList(new List<string>());
            Length = new TrackLength(totalSeconds);
            Title = new TitleName(title);
            TrackNumber = new TrackNumber(1);
            TrackPath = new TrackPath(path);
            Year = new Year(-1);
        }

        public LibLalaTagsBuilder(string title, ITrackPath fakePath, int totalSeconds)
        {
            Album = new AlbumName("");
            Artist = new ArtistList(new List<string>());
            Comment = new Comment("");
            DiscNumber = new DiscNumber(1);
            Genre = new GenreList(new List<string>());
            Length = new TrackLength(totalSeconds);
            Title = new TitleName(title);
            TrackNumber = new TrackNumber(1);
            TrackPath = fakePath;
            Year = new Year(-1);
        }

        public LibLalaTagsBuilder WithAlbum(string album)
        {
            Album = new AlbumName(album);
            return this;
        }

        public LibLalaTagsBuilder WithArtist(string artist)
        {
            Artist = new ArtistList(artist);
            return this;
        }

        public LibLalaTagsBuilder WithArtist(IList<string> artist)
        {
            Artist = new ArtistList(artist);
            return this;
        }

        public LibLalaTagsBuilder WithComment(string comment)
        {
            Comment = new Comment(comment);
            return this;
        }

        public LibLalaTagsBuilder WithDiscNumber(uint discNumber)
        {
            DiscNumber = new DiscNumber(discNumber);
            return this;
        }

        public LibLalaTagsBuilder WithGenre(string genre)
        {
            Genre = new GenreList(genre);
            return this;
        }

        public LibLalaTagsBuilder WithGenre(IList<string> genre)
        {
            Genre = new GenreList(genre);
            return this;
        }

        public LibLalaTagsBuilder WithTrackId(int trackId)
        {
            TrackId = trackId;
            return this;
        }

        public LibLalaTagsBuilder WithTrackNumber(uint trackNumber)
        {
            TrackNumber = new TrackNumber(trackNumber);
            return this;
        }

        public LibLalaTagsBuilder WithYear(int year)
        {
            Year = new Year(year);
            return this;
        }

        public LibLalaTags Build()
        {
            return new LibLalaTags(this);
        }
    }
}
