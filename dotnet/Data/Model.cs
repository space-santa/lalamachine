using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet.Data
{

    public class Genre
    {
        public string GenreId { get; set; }
        public string Name { get; set; }
    }

    public class Artist
    {
        public string ArtistId { get; set; }
        public string Name { get; set; }

    }

    public class Album
    {
        public string AlbumId { get; set; }
        public string Name { get; set; }
    }

    public class Track
    {
        public string TrackId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public int TrackNumber { get; set; }
        public string Path { get; set; }
        public int Length { get; set; }
        public int Year { get; set; }
        public int DiscNumber { get; set; }

        public string AlbumId { get; set; }
    }

    public class GenresOfTrack
    {
        public string GenresOfTrackId { get; set; }
        public string TrackId { get; set; }
        public string GenreId { get; set; }
    }

    public class ArtistsOfTrack
    {
        public string ArtistsOfTrackId { get; set; }
        public string TrackId { get; set; }
        public string ArtistId { get; set; }
    }
}
