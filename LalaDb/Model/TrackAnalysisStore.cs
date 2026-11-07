using System;
using System.Collections.Generic;
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

    public sealed record TrackToAnalyze(string Path, FileStamp Stamp, int TaggedBeatsPerMinute);

    // The analyses of the tracks, by path. Each call has its own context, so it can be used from any thread.
    public class TrackAnalysisStore
    {
        private readonly Func<LalaContext> _createContext;

        public TrackAnalysisStore(Func<LalaContext> createContext)
        {
            _createContext = createContext;
        }

        // The library's tracks that weren't analyzed as their files are now, by this version of the analysis. Not the ones
        // whose files are gone.
        public IReadOnlyList<TrackToAnalyze> TracksToAnalyze()
        {
            using var context = _createContext();
            var analyzed = context.Set<TrackAnalysis>()
                .Where(a => a.AnalyzerVersion == BeatGridAnalyzer.Version)
                .ToDictionary(a => a.Path, a => new FileStamp(a.FileSize, a.FileWriteTicks), StringComparer.Ordinal);
            var tracks = context.Set<Track>()
                .Where(t => t.Path != null)
                .Select(t => new { Path = t.Path!, t.BeatsPerMinute })
                .ToList();

            var toAnalyze = new List<TrackToAnalyze>();
            foreach (var track in tracks.DistinctBy(t => t.Path))
            {
                if (FileStamp.Of(track.Path) is not { } stamp)
                {
                    continue;
                }
                if (!analyzed.TryGetValue(track.Path, out var analyzedStamp) || analyzedStamp != stamp)
                {
                    toAnalyze.Add(new TrackToAnalyze(track.Path, stamp, track.BeatsPerMinute));
                }
            }
            return toAnalyze;
        }

        // Null when the track wasn't analyzed as its file is now, or has no beat.
        public BeatGrid? FindGrid(string path)
        {
            using var context = _createContext();
            var analysis = context.Set<TrackAnalysis>().SingleOrDefault(a => a.Path == path);
            if (analysis is null || analysis.AnalyzerVersion != BeatGridAnalyzer.Version || analysis.BeatsPerMinute <= 0)
            {
                return null;
            }
            if (FileStamp.Of(path) != new FileStamp(analysis.FileSize, analysis.FileWriteTicks))
            {
                return null;
            }
            return new BeatGrid(analysis.BeatsPerMinute, analysis.FirstBeat, analysis.BeatDeviationMs, analysis.BeatCount);
        }

        // The stamp of the file as it was before the analysis, a file changed since is analyzed again. Without a grid when the
        // track has no beat.
        public void Save(string path, FileStamp stamp, BeatGrid? grid)
        {
            using var context = _createContext();
            var analyses = context.Set<TrackAnalysis>();
            var analysis = analyses.SingleOrDefault(a => a.Path == path);
            if (analysis is null)
            {
                analysis = new TrackAnalysis { Path = path };
                analyses.Add(analysis);
            }
            analysis.FileSize = stamp.Size;
            analysis.FileWriteTicks = stamp.WriteTicks;
            analysis.AnalyzerVersion = BeatGridAnalyzer.Version;
            analysis.BeatsPerMinute = grid?.BeatsPerMinute ?? 0;
            analysis.FirstBeat = grid?.FirstBeat ?? 0;
            analysis.BeatDeviationMs = grid?.DeviationMs ?? 0;
            analysis.BeatCount = grid?.BeatCount ?? 0;
            context.SaveChanges();
        }
    }
}
