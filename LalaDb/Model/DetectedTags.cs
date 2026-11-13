using System;
using System.Collections.Generic;
using System.Linq;
using LalaDb.Data;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using Microsoft.EntityFrameworkCore;

namespace LalaDb.Model
{
    // The key and the tempo that the analysis found in the tracks, by path. They show for the tracks whose tags have none.
    public sealed class DetectedTags
    {
        private readonly Dictionary<string, (string Key, uint BeatsPerMinute)> _byPath;

        private DetectedTags(Dictionary<string, (string Key, uint BeatsPerMinute)> byPath)
        {
            _byPath = byPath;
        }

        public static DetectedTags None { get; } = new(new Dictionary<string, (string Key, uint BeatsPerMinute)>(StringComparer.Ordinal));

        // As the analysis has them now, also from a context that has read them before.
        public static DetectedTags Load(LalaContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var byPath = context.Set<TrackAnalysis>()
                .AsNoTracking()
                .Select(a => new { a.Path, a.Key, a.KeyAnalyzerVersion, a.BeatsPerMinute, a.BeatDeviationMs, a.BeatCount, a.AnalyzerVersion })
                .AsEnumerable()
                .ToDictionary(
                    a => a.Path,
                    a => (
                        Key: a.KeyAnalyzerVersion == KeyAnalyzer.Version ? a.Key : "",
                        BeatsPerMinute: a.AnalyzerVersion == BeatGridAnalyzer.Version ? BeatGrid.ShownBeatsPerMinuteOf(a.BeatsPerMinute, a.BeatDeviationMs, a.BeatCount) : 0),
                    StringComparer.Ordinal);
            return new DetectedTags(byPath);
        }

        // Empty when none was found.
        public string KeyOf(string path) => _byPath.TryGetValue(path, out var detected) ? detected.Key : "";

        // 0 when none was found.
        public uint BeatsPerMinuteOf(string path) => _byPath.TryGetValue(path, out var detected) ? detected.BeatsPerMinute : 0;
    }
}
