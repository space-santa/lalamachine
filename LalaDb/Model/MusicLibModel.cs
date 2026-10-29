using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LalaDb.Data;
using LibLala.LibLalaTagReader;
using LibLala.MusicScanner;
using Microsoft.EntityFrameworkCore;

namespace LalaDb.Model
{
    public class MusicLibModel
    {
        private readonly LalaContext _context;
        private readonly Func<LalaContext> _createScanContext;
        public bool Scanning { get; private set; }

        public MusicLibModel(LalaContext context, Func<LalaContext> createScanContext)
        {
            _context = context;
            _createScanContext = createScanContext;
            Scanning = false;
        }

        public string?[] GenreList(string searchString)
        {
            if (Scanning) { return Array.Empty<string>(); }

            if (_context.Genres is null) { return Array.Empty<string>(); }

            string?[] list = _context.Genres.AsEnumerable()
                .Where(x => x.Name is { } && x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToArray();

            return list;
        }
        public string?[] ArtistList(string genreFilter, string searchString)
        {
            if (Scanning) { return Array.Empty<string>(); }

            if (genreFilter is null)
            {
                genreFilter = "";
            }

            if (genreFilter.Length > 0 && _context.GenreTracks != null)
            {
                return _context.GenreTracks.AsEnumerable()
                               .Where(x => x.Genre?.Name == genreFilter)
                               .SelectMany(x =>
                               {
                                   if (x.Track != null && x.Track.ArtistTracks != null)
                                   {
                                       return x.Track.ArtistTracks;
                                   }

                                   return new List<ArtistTrack>();
                               })
                               .Select(x => x.Artist?.Name).Distinct().ToArray();
            }

            if (_context.Artists != null)
            {
                return _context.Artists.AsEnumerable()
                                        .Where(x => x.Name is { } && x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                        .Select(x => x.Name)
                                        .OrderBy(x => x)
                                        .ToArray();
            }

            return Array.Empty<string>();
        }

        public string?[]? AlbumList(string artistFilter, string genreFilter, string searchString)
        {
            if (Scanning) { return Array.Empty<string>(); }

            string?[]? list;

            if (artistFilter is null)
            {
                artistFilter = "";
            }

            if (genreFilter is null)
            {
                genreFilter = "";
            }

            if (artistFilter.Length > 0)
            {
                list = _context.ArtistTracks?.AsEnumerable()
                               .Where(x => x.Artist?.Name == artistFilter)
                               .Select(x => x.Track)
                               .Select(x => x?.Album?.Name).Distinct().ToArray();
            }
            else if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks?.AsEnumerable()
                               .Where(x => x.Genre?.Name == genreFilter)
                               .Select(x => x.Track)
                               .Select(x => x?.Album?.Name).Distinct().ToArray();
            }
            else if (string.IsNullOrEmpty(searchString))
            {
                list = _context.Albums?.Select(x => x.Name).ToArray();
            }
            else
            {
                list = _context.Albums?.AsEnumerable()
                    .Where(x =>
                    {
                        if (x.Name is { })
                        {
                            return x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase);
                        }

                        return false;
                    })
                    .Select(x => x.Name)
                    .OrderBy(x => x).ToArray();
            }

            return list;
        }

        public Track?[]? DisplayLib(string albumFilter, string artistFilter, string genreFilter, string searchString)
        {
            if (Scanning) { return Array.Empty<Track>(); }

            Track?[]? list;

            if (artistFilter is null)
            {
                artistFilter = "";
            }

            if (genreFilter is null)
            {
                genreFilter = "";
            }

            if (albumFilter is null)
            {
                albumFilter = "";
            }

            if (albumFilter.Length > 0)
            {
                try
                {
                    var a = _context.Albums?.AsEnumerable().Single(x => x.Name == albumFilter);
                    if (a != null && a.Tracks is { })
                    {
                        return a.Tracks.ToArray();
                    }

                    return Array.Empty<Track>();
                }
                catch (InvalidOperationException)
                {
                    //
                }
            }

            if (artistFilter.Length > 0)
            {
                list = _context.ArtistTracks?.AsEnumerable().Where(x => x.Artist?.Name == artistFilter).Select(x => x.Track).ToArray();
                return list;
            }

            if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks?.AsEnumerable().Where(x => x.Genre?.Name == genreFilter).Select(x => x.Track).ToArray();
                return list;
            }

            if (string.IsNullOrEmpty(searchString) && _context.Tracks != null)
            {
                list = _context.Tracks
                            .Include(track => track.Album!)
                            .Include(track => track.GenreTracks!)
                                .ThenInclude(gt => gt.Genre!)
                            .Include(track => track.ArtistTracks!)
                                .ThenInclude(at => at.Artist!)
                            .ToArray();
            }
            else
            {
                list = _context.Tracks?.AsEnumerable()
                    .Where(x =>
                    {
                        if (x.Title is { })
                        {
                            return x.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase);
                        }
                        return false;
                    }).ToArray();
            }

            return list;
        }

        public IList<LibLalaTags> GetAlbumTracks(string name)
        {
            if (string.IsNullOrEmpty(name) || name == LibLala.Constants.ALL)
            {
                return new List<LibLalaTags>();
            }

            var tracks = _context.Albums?.FirstOrDefault(x => x.Name == name)?.Tracks?.OrderBy(x => x.DiscNumber).ThenBy(x => x.TrackNumber);
            var tagList = new List<LibLalaTags>();

            if (tracks is null) { return tagList; }

            foreach (var track in tracks)
            {
                var lalaTags = LalaTags.Build(track);
                tagList.Add(lalaTags);
            }

            return tagList;
        }

        /// <summary>
        /// Replaces the library with the music found in <paramref name="path"/>.
        /// </summary>
        /// <returns>The files that couldn't be read and were left out.</returns>
        public async Task<IReadOnlyList<string>> ScanAsync(string path)
        {
            if (Scanning)
            {
                return Array.Empty<string>();
            }
            Scanning = true;
            try
            {
                // The scan runs on a worker thread, so it needs its own context. A DbContext isn't thread-safe.
                using var scanContext = _createScanContext();
                var directoryProcessor = new DirectoryProcessor(new ScannerDb(scanContext), path);
                directoryProcessor.FileScannedEvent += FileScannedInvoker;
                var fileCount = await Task.Run(() => directoryProcessor.FileCount).ConfigureAwait(true);
                InvokeFilesToScanChangedEvent(fileCount);
                await Task.Run(() => directoryProcessor.ProcessDirectory()).ConfigureAwait(true);
                return directoryProcessor.SkippedFiles;
            }
            finally
            {
                // The library was replaced behind this context's back, so drop everything it has cached.
                _context.ChangeTracker.Clear();
                Scanning = false;
            }
        }

        public event EventHandler<FileScannedEventArgs>? FilesToScanChangedEvent;
        protected virtual void InvokeFilesToScanChangedEvent(int count)
        {
            var args = new FileScannedEventArgs(count);
            FilesToScanChangedEvent?.Invoke(this, args);
        }

        internal void FileScannedInvoker(object? sender, FileScannedEventArgs args)
        {
            InvokeFileScannedEvent(args.Count);
        }

        public event EventHandler<FileScannedEventArgs>? FileScannedEvent;
        protected virtual void InvokeFileScannedEvent(int count)
        {
            var args = new FileScannedEventArgs(count);
            FileScannedEvent?.Invoke(this, args);
        }
    }
}
