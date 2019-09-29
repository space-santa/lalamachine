﻿using System;
using System.Collections.Generic;
using System.Text;
using LibLala.DomainPrimitives;

namespace LibLala.LibLalaTagReader
{
    public class LibLalaTagsBuilder
    {
        public AlbumName Album { get; private set; }
        public ArtistsOfTrack Artist { get; private set; }
        public Comment Comment { get; private set; }
        public DiscNumber DiscNumber { get; private set; }
        public GenresOfTrack Genre { get; private set; }
        public TitleName Title { get; }
        public int? TrackId { get; private set; }
        public TrackNumber TrackNumber { get; private set; }
        public TrackPath TrackPath { get; }
        public Year? Year { get; private set; }

        public LibLalaTagsBuilder(string title, string path)
        {
            Album = new AlbumName("");
            Artist = new ArtistsOfTrack(new List<string>());
            Comment = new Comment("");
            DiscNumber = new DiscNumber(1);
            Genre = new GenresOfTrack(new List<string>());
            Title = new TitleName(title);
            TrackNumber = new TrackNumber(1);
            TrackPath = new TrackPath(path);
        }

        public LibLalaTagsBuilder WithAlbum(string album)
        {
            Album = new AlbumName(album);
            return this;
        }

        public LibLalaTagsBuilder WithArtist(string artist)
        {
            Artist = new ArtistsOfTrack(artist);
            return this;
        }

        public LibLalaTagsBuilder WithArtist(List<string> artist)
        {
            Artist = new ArtistsOfTrack(artist);
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
            Genre = new GenresOfTrack(genre);
            return this;
        }

        public LibLalaTagsBuilder WithGenre(List<string> genre)
        {
            Genre = new GenresOfTrack(genre);
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

        public LibLalaTagsBuilder WithYear(uint year)
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
