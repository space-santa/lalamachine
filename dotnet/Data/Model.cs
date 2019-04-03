﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet.Data
{

    public class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; }

        public List<GenreTrack> GenreTracks { get; set; }
    }

    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }

        public List<ArtistTrack> ArtistTracks { get; set; }
    }

    public class Album
    {
        public int AlbumId { get; set; }
        public string Name { get; set; }

        public List<Track> Tracks { get; set; }
    }

    public class Track
    {
        public int TrackId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public int TrackNumber { get; set; }
        public string Path { get; set; }
        public int Length { get; set; }
        public int Year { get; set; }
        public int DiscNumber { get; set; }

        public int AlbumId { get; set; }
        public Album Album { get; set; }

        public List<ArtistTrack> ArtistTracks { get; set; }
        public List<GenreTrack> GenreTracks { get; set; }
    }

    public class GenreTrack
    {
        public int GenreTrackId { get; set; }

        public int TrackId { get; set; }
        public Track Track { get; set; }

        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }

    public class ArtistTrack
    {
        public int ArtistTrackId { get; set; }

        public int TrackId { get; set; }
        public Track Track { get; set; }

        public int ArtistId { get; set; }
        public Artist Artist { get; set; }
    }
}
