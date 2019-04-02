using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotnet.Data;
using Microsoft.EntityFrameworkCore;
using MusicScanner;
using TagReader;

namespace dotnet
{
    public class ScannerDb : IMusicDatabase
    {
        LalaContext _context;

        public ScannerDb(LalaContext Context)
        {
            _context = Context;
        }

        public void AddTagsToDatabase(Tags tags)
        {
            Track track = new Track();
            track.TrackId = Guid.NewGuid().ToString();

            foreach (var genre in tags.genre)
            {
                string genreId;

                if (_context.Genres.Any(x => x.Name == genre))
                {
                    genreId = _context.Genres.Single(x => x.Name == genre).GenreId;
                }
                else
                {
                    var dbGenre = new Genre();
                    dbGenre.Name = genre;
                    dbGenre.GenreId = Guid.NewGuid().ToString();
                    _context.Genres.Add(dbGenre);
                    genreId = dbGenre.GenreId;
                }

                var got = new GenresOfTrack();
                got.TrackId = track.TrackId;
                got.GenreId = genreId;
                _context.GenresOfTracks.Add(got);
            }

            foreach (var artist in tags.Artist)
            {
                string artistId ;

                if (_context.Artists.Any(x => x.Name == artist))
                {
                    artistId = _context.Artists.Single(x => x.Name == artist).ArtistId;
                }
                else
                {
                    var dbArtist = new Artist();
                    dbArtist.Name = artist;
                    dbArtist.ArtistId = Guid.NewGuid().ToString();
                    _context.Artists.Add(dbArtist);
                    artistId = dbArtist.ArtistId;
                }

                var aot = new ArtistsOfTrack();
                aot.TrackId = track.TrackId;
                aot.ArtistId = artistId;
                _context.ArtistsOfTracks.Add(aot);
            }

            if (_context.Albums.Any(x => x.Name == tags.album))
            {
                track.AlbumId = _context.Albums.Single(x => x.Name == tags.album).AlbumId;
            }
            else
            {
                var dbAlbum = new Album();
                dbAlbum.Name = tags.album;
                dbAlbum.AlbumId = Guid.NewGuid().ToString();
                track.AlbumId = dbAlbum.AlbumId;
                _context.Albums.Add(dbAlbum);
            }

            track.Comment = tags.comment;
            track.DiscNumber = (int)tags.discNumber;
            track.Length = tags.length;
            track.Path = tags.path;
            track.Title = tags.title;
            track.TrackNumber = (int)tags.track;
            track.Year = (int)tags.year;

            _context.Tracks.Add(track);
            _context.SaveChanges();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void EnsureDatabase()
        {
            _context.Database.EnsureDeleted();
            // Migrate is mutually exclusive with EnsureCreated. And we want to be able to migrate in the future, just in case.
            _context.Database.Migrate();
        }
    }
}
