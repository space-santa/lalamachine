using System;
using System.Collections.Generic;
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

        private void PrepareTrackForSave(LibLalaTags tags, Dictionary<string, Genre> genres, Dictionary<string, Artist> artists, Dictionary<string, Album> albums)
        {
            var track = new Track();

            foreach (var g in tags.Genre)
            {
                _context.GenreTracks?.Add(new GenreTrack { Genre = genres[g], Track = track });
            }

            foreach (var a in tags.Artist)
            {
                _context.ArtistTracks?.Add(new ArtistTrack { Artist = artists[a], Track = track });
            }

            track.Album = albums[tags.Album];
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
            var genres = SaveAllGenres();
            var artists = SaveAllArtists();
            var albums = SaveAllAlbums();

            foreach (var tag in _tagList)
            {
                PrepareTrackForSave(tag, genres, artists, albums);
            }

            // A single SaveChanges runs in one transaction, so the old library is only replaced once the new one is complete.
            _context.SaveChanges();
        }

        private Dictionary<string, Genre> SaveAllGenres()
        {
            var genres = new Dictionary<string, Genre>(StringComparer.Ordinal);

            foreach (var genreName in _genreList.ToStringList())
            {
                var genre = new Genre
                {
                    Name = genreName
                };

                _context.Genres?.Add(genre);
                genres[genreName] = genre;
            }

            return genres;
        }

        private Dictionary<string, Artist> SaveAllArtists()
        {
            var artists = new Dictionary<string, Artist>(StringComparer.Ordinal);

            foreach (var artistName in _artistList.ToStringList())
            {
                var artist = new Artist
                {
                    Name = artistName
                };

                _context.Artists?.Add(artist);
                artists[artistName] = artist;
            }

            return artists;
        }

        private Dictionary<string, Album> SaveAllAlbums()
        {
            var albums = new Dictionary<string, Album>(StringComparer.Ordinal);

            foreach (var albumName in _albumList.ToStringList())
            {
                var album = new Album
                {
                    Name = albumName
                };

                _context.Albums?.Add(album);
                albums[albumName] = album;
            }

            return albums;
        }

        public void EnsureDatabase()
        {
            // Every scan starts from scratch, otherwise a second scan in the same session duplicates every track.
            _albumList = new BaseNameStringList<AlbumName>();
            _artistList = new BaseNameStringList<ArtistName>();
            _genreList = new BaseNameStringList<GenreName>();
            _tagList = new List<LibLalaTags>();

            _context.Albums?.RemoveRange(_context.Albums);
            _context.Artists?.RemoveRange(_context.Artists);
            _context.ArtistTracks?.RemoveRange(_context.ArtistTracks);
            _context.Genres?.RemoveRange(_context.Genres);
            _context.GenreTracks?.RemoveRange(_context.GenreTracks);
            _context.Tracks?.RemoveRange(_context.Tracks);
        }
    }
}
