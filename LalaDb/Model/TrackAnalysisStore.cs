using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using LalaDb.Data;
using LibLala.Audio;
using LibLala.Audio.Analysis;

namespace LalaDb.Model
{
    // A file as it is now. When either changes, its analysis is out of date.
    public readonly record struct FileStamp(long Size, long WriteTicks)
    {
        // Null when there is no such file.
        public static FileStamp? Of(string path)
        {
            var file = new FileInfo(path);
            return file.Exists ? new FileStamp(file.Length, file.LastWriteTimeUtc.Ticks) : null;
        }
    }

    // A track and what to look for in it: the beats, the key, or both.
    public sealed record TrackToAnalyze(string Path, FileStamp Stamp, int TaggedBeatsPerMinute, bool NeedsGrid, bool NeedsKey);

    // The analyses of the tracks, by path. Each call has its own context, so it can be used from any thread.
    public class TrackAnalysisStore
    {
        private readonly Func<LalaContext> _createContext;

        public TrackAnalysisStore(Func<LalaContext> createContext)
        {
            _createContext = createContext;
        }

        // The library's tracks that weren't analyzed as their files are now, by these versions of the analyses. The key only
        // of tracks whose tags have none. Not the tracks whose files are gone.
        public IReadOnlyList<TrackToAnalyze> TracksToAnalyze()
        {
            using var context = _createContext();
            var analyzed = context.Set<TrackAnalysis>()
                .Select(a => new { a.Path, a.FileSize, a.FileWriteTicks, a.AnalyzerVersion, a.KeyAnalyzerVersion })
                .ToDictionary(a => a.Path, StringComparer.Ordinal);
            var tracks = context.Set<Track>()
                .Where(t => t.Path != null)
                .Select(t => new { Path = t.Path!, t.BeatsPerMinute, t.InitialKey })
                .ToList();

            var toAnalyze = new List<TrackToAnalyze>();
            foreach (var track in tracks.DistinctBy(t => t.Path))
            {
                if (FileStamp.Of(track.Path) is not { } stamp)
                {
                    continue;
                }
                // What was found in the file as it was before says nothing about it now.
                var analysis = analyzed.GetValueOrDefault(track.Path);
                if (analysis is not null && new FileStamp(analysis.FileSize, analysis.FileWriteTicks) != stamp)
                {
                    analysis = null;
                }
                var needsGrid = analysis is null || !BeatGridAnalyzer.IsCurrent(analysis.AnalyzerVersion, hasTaggedTempo: track.BeatsPerMinute > 0);
                var needsKey = string.IsNullOrWhiteSpace(track.InitialKey) && analysis?.KeyAnalyzerVersion != KeyAnalyzer.Version;
                if (needsGrid || needsKey)
                {
                    toAnalyze.Add(new TrackToAnalyze(track.Path, stamp, track.BeatsPerMinute, needsGrid, needsKey));
                }
            }
            return toAnalyze;
        }

        // Null when the track wasn't analyzed as its file is now, or has no beat. Also when the database can't be read right
        // now, e.g. busy with the analysis: the track plays all the same, only not beat-matched.
        public BeatGrid? FindGrid(string path)
        {
            try
            {
                return FindGridIn(path);
            }
            catch (Exception e) when (e is DbException or IOException)
            {
                return null;
            }
        }

        private BeatGrid? FindGridIn(string path)
        {
            using var context = _createContext();
            var analysis = context.Set<TrackAnalysis>().SingleOrDefault(a => a.Path == path);
            // A grid that was found with a tagged tempo, or else one that is about to be found again and good to play by
            // until then.
            if (analysis is null || !BeatGridAnalyzer.IsCurrent(analysis.AnalyzerVersion, hasTaggedTempo: true) || analysis.BeatsPerMinute <= 0)
            {
                return null;
            }
            if (FileStamp.Of(path) != new FileStamp(analysis.FileSize, analysis.FileWriteTicks))
            {
                return null;
            }
            return new BeatGrid(analysis.BeatsPerMinute, analysis.FirstBeat, analysis.BeatDeviationMs, analysis.BeatCount, analysis.FirstDownbeat, analysis.MusicStart, analysis.MusicEnd);
        }

        // What was looked for in the track, as it says, and nothing else: a grid found earlier in the same file stays when
        // only the key was looked for. Its stamp is of the file as it was before the analysis, a file changed since is
        // analyzed again. Without a grid when the track has no beat, without a key when it has none.
        public void Save(TrackToAnalyze track, BeatGrid? grid, MusicalKey? key)
        {
            ArgumentNullException.ThrowIfNull(track);

            using var context = _createContext();
            var analyses = context.Set<TrackAnalysis>();
            var analysis = analyses.SingleOrDefault(a => a.Path == track.Path);
            if (analysis is null)
            {
                analysis = new TrackAnalysis { Path = track.Path };
                analyses.Add(analysis);
            }
            // What was found in the file as it was before goes either way.
            var isSameFile = new FileStamp(analysis.FileSize, analysis.FileWriteTicks) == track.Stamp;
            analysis.FileSize = track.Stamp.Size;
            analysis.FileWriteTicks = track.Stamp.WriteTicks;
            if (track.NeedsGrid || !isSameFile)
            {
                analysis.AnalyzerVersion = track.NeedsGrid ? BeatGridAnalyzer.Version : 0;
                analysis.BeatsPerMinute = grid?.BeatsPerMinute ?? 0;
                analysis.FirstBeat = grid?.FirstBeat ?? 0;
                analysis.BeatDeviationMs = grid?.DeviationMs ?? 0;
                analysis.BeatCount = grid?.BeatCount ?? 0;
                analysis.FirstDownbeat = grid?.FirstDownbeat ?? 0;
                analysis.MusicStart = grid?.MusicStart ?? 0;
                analysis.MusicEnd = grid?.MusicEnd ?? 0;
            }
            if (track.NeedsKey || !isSameFile)
            {
                analysis.KeyAnalyzerVersion = track.NeedsKey ? KeyAnalyzer.Version : 0;
                analysis.Key = key?.Camelot ?? "";
            }
            context.SaveChanges();
        }
    }
}
