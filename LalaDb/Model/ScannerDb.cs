using System.Linq;
using LalaDb.Data;
using LibLala.MusicScanner;

namespace LalaDb.Model
{
    public class ScannerDb : IMusicDatabase
    {
        private readonly LalaContext _context;

        public ScannerDb(LalaContext Context)
        {
            _context = Context;
        }

        public void AddTagsToDatabase(LibLala.LibLalaTagReader.LibLalaTags tags)
        {
            if (tags is null)
            {
                return;
            }

            var track = new Track();

            foreach (var g in tags.Genre)
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

                    _context.Genres?.Add(genre);
                }

                _context.GenreTracks?.Add(new GenreTrack { Genre = genre, Track = track });
            }

            foreach (var a in tags.Artist)
            {
                Artist artist;

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
                    _context.Artists?.Add(artist);
                }


                _context.ArtistTracks?.Add(new ArtistTrack { Artist = artist, Track = track });
            }

            if (_context.Albums.Any(x => x.Name == tags.Album))
            {
                track.Album = _context.Albums.Single(x => x.Name == tags.Album);
            }
            else
            {
                track.Album = new Album
                {
                    Name = tags.Album
                };
            }

            track.Comment = tags.Comment;
            track.DiscNumber = (int)tags.DiscNumber;
            track.Length = tags.length;
            track.Path = tags.Path;
            track.Title = tags.Title;
            track.TrackNumber = (int)tags.Track;
            track.Year = (int)tags.Year;

            _context.Tracks?.Add(track);
            _context.SaveChanges();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void EnsureDatabase()
        {
            _context.Albums?.RemoveRange(_context.Albums);
            _context.Artists?.RemoveRange(_context.Artists);
            _context.ArtistTracks?.RemoveRange(_context.ArtistTracks);
            _context.Genres?.RemoveRange(_context.Genres);
            _context.GenreTracks?.RemoveRange(_context.GenreTracks);
            _context.Tracks?.RemoveRange(_context.Tracks);
        }
    }
}
