using Microsoft.EntityFrameworkCore;

namespace LalaDb.Data
{
    public class LalaContext : DbContext
    {
        public LalaContext(DbContextOptions<LalaContext> options)
           : base(options)
        { }

        public DbSet<Genre>? Genres { get; set; }
        public DbSet<Artist>? Artists { get; set; }
        public DbSet<Album>? Albums { get; set; }
        public DbSet<Playlist>? Playlists { get; set; }
        public DbSet<Track>? Tracks { get; set; }
        public DbSet<ArtistTrack>? ArtistTracks { get; set; }
        public DbSet<GenreTrack>? GenreTracks { get; set; }
        public DbSet<PlaylistTrack>? PlaylistTracks { get; set; }
    }
}
