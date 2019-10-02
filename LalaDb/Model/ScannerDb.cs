using System.Collections.Generic;
using System.Linq;
using LalaDb.Data;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using LibLala.MusicScanner;

namespace LalaDb.Model
{
    public class ScannerDb : IMusicDatabase
    {
        private readonly LalaContext _context;

        private BaseNameStringList<AlbumName> _albumList;
        private BaseNameStringList<ArtistName> _artistList;
        private BaseNameStringList<GenreName> _genreList;
        private List<LibLalaTags> _tagList;

        public ScannerDb(LalaContext context)
        {
            _context = context;
            _albumList = new BaseNameStringList<AlbumName>();
            _artistList = new BaseNameStringList<ArtistName>();
            _genreList = new BaseNameStringList<GenreName>();
            _tagList = new List<LibLalaTags>();
        }

        public void AddTagsToDatabase(LibLalaTags tags)
        {
            if (tags is null)
            {
                return;
            }

            _tagList.Add(tags);

            foreach (var genreName in tags.Genre)
            {
                _genreList.Add(new GenreName(genreName));
            }

            foreach (var artistName in tags.Artist)
            {
                _artistList.Add(new ArtistName(artistName));
            }

            _albumList.Add(new AlbumName(tags.Album));
        }

        private void PrepareTrackForSave(LibLalaTags tags)
        {
            var track = new Track();

            foreach (var g in tags.Genre)
            {
                var genre = _context.Genres.Single(x => x.Name == g);
                _context.GenreTracks?.Add(new GenreTrack { Genre = genre, Track = track });
            }

            foreach (var a in tags.Artist)
            {
                var artist = _context.Artists.Single(x => x.Name == a);
                _context.ArtistTracks?.Add(new ArtistTrack { Artist = artist, Track = track });
            }

            track.Album = _context.Albums.Single(x => x.Name == tags.Album);
            track.Comment = tags.Comment;
            track.DiscNumber = (int)tags.DiscNumber;
            track.Length = tags.Length;
            track.Path = tags.Path;
            track.Title = tags.Title;
            track.TrackNumber = (int)tags.Track;
            track.Year = tags.Year;

            _context.Tracks?.Add(track);
        }

        public void SaveChanges()
        {
            SaveAllGenres();
            SaveAllArtists();
            SaveAllAlbums();
            _context.SaveChanges();

            foreach (var tag in _tagList)
            {
                PrepareTrackForSave(tag);
            }

            _context.SaveChanges();
        }

        private void SaveAllGenres()
        {
            foreach (var genreName in _genreList.ToStringList())
            {
                var genre = new Genre
                {
                    Name = genreName
                };

                _context.Genres?.Add(genre);
            }
        }

        private void SaveAllArtists()
        {
            foreach (var artistName in _artistList.ToStringList())
            {
                var artist = new Artist
                {
                    Name = artistName
                };

                _context.Artists?.Add(artist);
            }
        }

        private void SaveAllAlbums()
        {
            foreach (var albumName in _albumList.ToStringList())
            {
                var album = new Album
                {
                    Name = albumName
                };

                _context.Albums?.Add(album);
            }
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
