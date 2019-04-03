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

            foreach (var g in tags.genre)
            {
                Genre genre;

                if (_context.Genres.Any(x => x.Name == g))
                {
                    genre = _context.Genres.Single(x => x.Name == g);
                }
                else
                {
                    genre = new Genre
                    {
                        Name = g
                    };

                    _context.Genres.Add(genre);
                }

                GenreTrack gt = new GenreTrack
                {
                    Genre = genre,
                    Track = track
                };
                _context.GenreTracks.Add(gt);
            }

            foreach (var a in tags.Artist)
            {
                Artist artist ;

                if (_context.Artists.Any(x => x.Name == a))
                {
                    artist = _context.Artists.Single(x => x.Name == a);
                }
                else
                {
                    artist = new Artist
                    {
                        Name = a
                    };
                    _context.Artists.Add(artist);
                }

                ArtistTrack at = new ArtistTrack
                {
                    Artist = artist,
                    Track = track
                };
                _context.ArtistTracks.Add(at);
            }

            if (_context.Albums.Any(x => x.Name == tags.album))
            {
                track.Album = _context.Albums.Single(x => x.Name == tags.album);
            }
            else
            {
                track.Album = new Album
                {
                    Name = tags.album
                };
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
