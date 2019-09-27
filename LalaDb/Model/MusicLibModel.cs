﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LalaDb.Data;
using LibLala.LibLalaTagReader;
using Microsoft.EntityFrameworkCore;

namespace LalaDb.Model
{
    public class MusicLibModel
    {
        private readonly LalaContext _context;
        private readonly ScannerDb _scannerDb;
        public bool Scanning { get; set; }

        public MusicLibModel(LalaContext context)
        {
            _context = context;
            _scannerDb = new ScannerDb(_context);
            Scanning = false;
        }

        public string?[] genreList(string searchString)
        {
            if (Scanning) { return Array.Empty<string>(); }

            var list = _context.Genres.AsEnumerable()
                .Where(x => x.Name is { } && x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToArray();
            return list;
        }
        public string?[] artistList(string genreFilter, string searchString)
        {
            if (Scanning) { return Array.Empty<string>(); }

            if (genreFilter is null) genreFilter = "";

            string?[] list;

            if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks.AsEnumerable()
                               .Where(x => x.Genre?.Name == genreFilter)
                               .SelectMany(x => x.Track?.ArtistTracks)
                               .Select(x => x.Artist?.Name).Distinct().ToArray();
            }
            else
            {
                list = _context.Artists.AsEnumerable()
                                        .Where(x => x.Name is { } && x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                        .Select(x => x.Name)
                                        .OrderBy(x => x)
                                        .ToArray();
            }

            return list;
        }

        public string?[] albumList(string artistFilter, string genreFilter, string searchString)
        {
            if (Scanning) { return Array.Empty<string>(); }

            string?[] list;

            if (artistFilter is null) artistFilter = "";
            if (genreFilter is null) genreFilter = "";

            if (artistFilter.Length > 0)
            {
                list = _context.ArtistTracks.AsEnumerable()
                               .Where(x => x.Artist?.Name == artistFilter)
                               .Select(x => x.Track)
                               .Select(x => x?.Album?.Name).Distinct().ToArray();
            }
            else if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks.AsEnumerable()
                               .Where(x => x.Genre?.Name == genreFilter)
                               .Select(x => x.Track)
                               .Select(x => x?.Album?.Name).Distinct().ToArray();
            }
            else if (string.IsNullOrEmpty(searchString))
            {
                list = _context.Albums.Select(x => x.Name).ToArray();
            }
            else
            {
                list = _context.Albums.AsEnumerable()
                    .Where(x =>
                    {
                        if (x.Name is { }) return x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase);
                        return false;
                    })
                    .Select(x => x.Name)
                    .OrderBy(x => x).ToArray();
            }

            return list;
        }

        public Track?[] displayLib(string albumFilter, string artistFilter, string genreFilter, string searchString)
        {
            if (Scanning) { return Array.Empty<Track>(); }

            Track?[] list;

            if (artistFilter is null) artistFilter = "";
            if (genreFilter is null) genreFilter = "";
            if (albumFilter is null) albumFilter = "";

            if (albumFilter.Length > 0)
            {
                try
                {
                    var a = _context.Albums.AsEnumerable().Single(x => x.Name == albumFilter);
                    if (a.Tracks is { }) return a.Tracks.ToArray();
                    return Array.Empty<Track>();
                }
                catch (InvalidOperationException)
                {
                    //
                }
            }

            if (artistFilter.Length > 0)
            {
                list = _context.ArtistTracks.AsEnumerable().Where(x => x.Artist?.Name == artistFilter).Select(x => x.Track).ToArray();
                return list;
            }

            if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks.AsEnumerable().Where(x => x.Genre?.Name == genreFilter).Select(x => x.Track).ToArray();
                return list;
            }

            if (string.IsNullOrEmpty(searchString))
            {
                list = _context.Tracks
                            .Include(track => track.Album)
                            .Include(track => track.GenreTracks)
                                .ThenInclude(gt => gt.Genre)
                            .Include(track => track.ArtistTracks)
                                .ThenInclude(at => at.Artist)
                            .ToArray();
            }
            else
            {
                list = _context.Tracks.AsEnumerable()
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

        public List<LibLalaTags> getAlbumTracks(string name)
        {
            if (string.IsNullOrEmpty(name) || name == LibLala.Constants.ALL) return new List<LibLalaTags>();

            var tracks = _context.Albums.Single(x => x.Name == name).Tracks.OrderBy(x => x.DiscNumber).ThenBy(x => x.TrackNumber);
            var tagList = new List<LibLalaTags>();
            foreach (var track in tracks)
            {
                var lalaTags = new LalaTags(track);
                tagList.Add(lalaTags);
            }
            return tagList;
        }

        public LalaTags getMetadataForMrl(string path)
        {
            path = LibLala.Utils.StringUtils.RemoveFilePrefix(path);
            var tags = new LalaTags(_context.Tracks.Single(x => Path.GetFullPath(x.Path ?? "") == Path.GetFullPath(path)));
            return tags;
        }

        public async Task scanAsync(string path)
        {
            if (Scanning)
            {
                return;
            }
            Scanning = true;
            await Task.Run(() => LibLala.MusicScanner.DirectoryProcessor.ProcessDirectory(path, _scannerDb)).ConfigureAwait(true);
            Scanning = false;
        }
    }
}
