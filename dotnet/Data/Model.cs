using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet
{

    public class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; }
    }

    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }

    }

    public class Album
    {
        public int AlbumId { get; set; }
        public string Name { get; set; }
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

        public int GenreId { get; set; }
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }

    }
}
