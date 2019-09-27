using System.Collections.Generic;

namespace LalaDb.Data
{

    public class Genre
    {
        public int GenreId { get; set; }
        public string? Name { get; set; }

        public virtual List<GenreTrack>? GenreTracks { get; }
    }

    public class Artist
    {
        public int ArtistId { get; set; }
        public string? Name { get; set; }

        public virtual List<ArtistTrack>? ArtistTracks { get; }
    }

    public class Album
    {
        public int AlbumId { get; set; }
        public string? Name { get; set; }

        public virtual List<Track>? Tracks { get; }
    }

    public class Playlist
    {
        public int PlaylistId { get; set; }
        public string? Name { get; set; }
        public virtual List<PlaylistTrack>? Tracks { get; }
    }

    public class Track
    {
        public int TrackId { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public int TrackNumber { get; set; }
        public string? Path { get; set; }
        public int Length { get; set; }
        public int Year { get; set; }
        public int DiscNumber { get; set; }

        public int AlbumId { get; set; }
        public virtual Album? Album { get; set; }

        public virtual List<ArtistTrack>? ArtistTracks { get; }
        public virtual List<GenreTrack>? GenreTracks { get; }
    }


    public class ArtistTrack
    {
        public int ArtistTrackId { get; set; }

        public int ArtistId { get; set; }
        public virtual Artist? Artist { get; set; }

        public int TrackId { get; set; }
        public virtual Track? Track { get; set; }
    }


    public class GenreTrack
    {
        public int GenreTrackId { get; set; }

        public int GenreId { get; set; }
        public virtual Genre? Genre { get; set; }

        public int TrackId { get; set; }
        public virtual Track? Track { get; set; }
    }

    public class PlaylistTrack
    {
        public int PlaylistTrackId { get; set; }

        public int PlaylistId { get; set; }
        public virtual Playlist? Playlist { get; set; }

        // Because SQLite can't drop columns we have to keep the ID. DON'T USE IT.
        public int TrackId { get; set; }
        // Because SQLite doesn't agree with adding or dropping Foreign Keys, we only have the TrackPath here, not the virtual Track.
        public string? TrackPath { get; set; }

        public int Order { get; set; }
    }
}
