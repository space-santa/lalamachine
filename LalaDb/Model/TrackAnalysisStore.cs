using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using LalaDb.Data;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using Microsoft.EntityFrameworkCore;

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

    // A track and what to look for in it: the beats, the key, the waveform, or more of them. With the grid that an earlier
    // version of the analysis found, when this one finds the same unless it may change: see BeatGridAnalyzer.MayChange.
    public sealed record TrackToAnalyze(string Path, FileStamp Stamp, int TaggedBeatsPerMinute, bool NeedsGrid, bool NeedsKey, bool NeedsWaveform, BeatGrid? Earlier = null);

    // The analyses and waveforms of the tracks, by path. Each call has its own context, so it can be used from any thread.
    public class TrackAnalysisStore
    {
        private readonly Func<LalaContext> _createContext;
        // One SQLite connection writes at a time anyway, and this way two never wait for each other. Nor do the analysis and
        // the player both add the waveform of the same track.
        private readonly System.Threading.Lock _writeGate = new();

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
                .AsNoTracking()
                .ToDictionary(a => a.Path, StringComparer.Ordinal);
            // Not the levels, only whether they are of the file as it is now.
            var waveforms = context.Set<TrackWaveform>()
                .Where(w => w.Version == Waveform.Version)
                .Select(w => new { w.Path, w.FileSize, w.FileWriteTicks })
                .ToDictionary(w => w.Path, w => new FileStamp(w.FileSize, w.FileWriteTicks), StringComparer.Ordinal);
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
                var needsGrid = analysis is null || !BeatGridAnalyzer.IsCurrent(analysis.AnalyzerVersion);
                var needsKey = string.IsNullOrWhiteSpace(track.InitialKey) && analysis?.KeyAnalyzerVersion != KeyAnalyzer.Version;
                var earlier = needsGrid
                    && analysis is not null
                    && BeatGridAnalyzer.IsCurrentUnlessChanged(analysis.AnalyzerVersion, hasTaggedTempo: track.BeatsPerMinute > 0)
                    ? GridOf(analysis)
                    : null;
                var needsWaveform = !waveforms.TryGetValue(track.Path, out var waveformStamp) || waveformStamp != stamp;
                if (needsGrid || needsKey || needsWaveform)
                {
                    toAnalyze.Add(new TrackToAnalyze(track.Path, stamp, track.BeatsPerMinute, needsGrid, needsKey, needsWaveform, earlier));
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
            // Also one that is about to be found again, good to play by until then.
            if (analysis is null || !BeatGridAnalyzer.IsPlayable(analysis.AnalyzerVersion) || analysis.BeatsPerMinute <= 0)
            {
                return null;
            }
            if (FileStamp.Of(path) != new FileStamp(analysis.FileSize, analysis.FileWriteTicks))
            {
                return null;
            }
            return GridOf(analysis);
        }

        // Null without a beat. The one place that reads a grid from the columns, with SetGrid the one that writes it, so
        // that every grid that is read has all that was saved: its intro too.
        private static BeatGrid? GridOf(TrackAnalysis analysis)
        {
            if (analysis.BeatsPerMinute <= 0)
            {
                return null;
            }
            var intro = analysis.IntroBeatsPerMinute > 0
                ? new BeatGrid(analysis.IntroBeatsPerMinute, analysis.IntroFirstBeat, analysis.IntroBeatDeviationMs, analysis.IntroBeatCount, analysis.IntroFirstDownbeat, analysis.IntroMusicStart, analysis.IntroMusicEnd)
                : null;
            return new BeatGrid(analysis.BeatsPerMinute, analysis.FirstBeat, analysis.BeatDeviationMs, analysis.BeatCount, analysis.FirstDownbeat, analysis.MusicStart, analysis.MusicEnd) { Intro = intro };
        }

        // All 0 without a grid, the intro's without one.
        private static void SetGrid(TrackAnalysis analysis, BeatGrid? grid)
        {
            analysis.BeatsPerMinute = grid?.BeatsPerMinute ?? 0;
            analysis.FirstBeat = grid?.FirstBeat ?? 0;
            analysis.BeatDeviationMs = grid?.DeviationMs ?? 0;
            analysis.BeatCount = grid?.BeatCount ?? 0;
            analysis.FirstDownbeat = grid?.FirstDownbeat ?? 0;
            analysis.MusicStart = grid?.MusicStart ?? 0;
            analysis.MusicEnd = grid?.MusicEnd ?? 0;
            var intro = grid?.Intro;
            analysis.IntroBeatsPerMinute = intro?.BeatsPerMinute ?? 0;
            analysis.IntroFirstBeat = intro?.FirstBeat ?? 0;
            analysis.IntroBeatDeviationMs = intro?.DeviationMs ?? 0;
            analysis.IntroBeatCount = intro?.BeatCount ?? 0;
            analysis.IntroFirstDownbeat = intro?.FirstDownbeat ?? 0;
            analysis.IntroMusicStart = intro?.MusicStart ?? 0;
            analysis.IntroMusicEnd = intro?.MusicEnd ?? 0;
        }

        // Null when there is none of the file as it is now, by this version. Also when the database can't be read right now:
        // then the player reads it from the file.
        public Waveform? FindWaveform(string path)
        {
            try
            {
                return FindWaveformIn(path);
            }
            catch (Exception e) when (e is DbException or IOException)
            {
                return null;
            }
        }

        // Of a track that played before the analysis got to it, so that the analysis doesn't read it again. Its stamp is of
        // the file as it was before it was read, like the analysis's.
        public void SaveWaveform(string path, FileStamp stamp, Waveform waveform)
        {
            ArgumentNullException.ThrowIfNull(waveform);

            lock (_writeGate)
            {
                using var context = _createContext();
                SetWaveform(context, path, stamp, waveform);
                context.SaveChanges();
            }
        }

        private Waveform? FindWaveformIn(string path)
        {
            using var context = _createContext();
            var stored = context.Set<TrackWaveform>().SingleOrDefault(w => w.Path == path);
            if (stored is null || stored.Version != Waveform.Version || FileStamp.Of(path) != new FileStamp(stored.FileSize, stored.FileWriteTicks))
            {
                return null;
            }
            return Waveform.FromBytes(stored.Levels);
        }

        // What was looked for in the track, as it says, and nothing else: a grid found earlier in the same file stays when
        // only the key was looked for. Its stamp is of the file as it was before the analysis, a file changed since is
        // analyzed again. Without a grid when the track has no beat, without a key when it has none. A waveform that was
        // looked for and isn't there is looked for again next time.
        public void Save(TrackToAnalyze track, BeatGrid? grid, MusicalKey? key, Waveform? waveform)
        {
            ArgumentNullException.ThrowIfNull(track);

            lock (_writeGate)
            {
                SaveAnalysis(track, grid, key, waveform);
            }
        }

        private void SaveAnalysis(TrackToAnalyze track, BeatGrid? grid, MusicalKey? key, Waveform? waveform)
        {
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
                SetGrid(analysis, grid);
            }
            if (track.NeedsKey || !isSameFile)
            {
                analysis.KeyAnalyzerVersion = track.NeedsKey ? KeyAnalyzer.Version : 0;
                analysis.Key = key?.Camelot ?? "";
            }
            if (track.NeedsWaveform && waveform is not null)
            {
                SetWaveform(context, track.Path, track.Stamp, waveform);
            }
            context.SaveChanges();
        }

        private static void SetWaveform(LalaContext context, string path, FileStamp stamp, Waveform waveform)
        {
            var waveforms = context.Set<TrackWaveform>();
            var stored = waveforms.SingleOrDefault(w => w.Path == path);
            if (stored is null)
            {
                stored = new TrackWaveform { Path = path };
                waveforms.Add(stored);
            }
            stored.FileSize = stamp.Size;
            stored.FileWriteTicks = stamp.WriteTicks;
            stored.Version = Waveform.Version;
            stored.Levels = waveform.ToBytes();
        }
    }
}
