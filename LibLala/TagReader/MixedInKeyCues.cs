using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace LibLala.LibLalaTagReader
{
    // The cue points that Mixed In Key writes into a track: base64 JSON like {"cues":[{"time":49.0},...]} with the times in
    // ms, in a GEOB frame of an mp3 or a com.mixedinkey.mixedinkey:cuepoints atom of an m4a. It puts them where phrases
    // start, on the first beat of a bar.
    public static class MixedInKeyCues
    {
        // In seconds, empty when the track has none or can't be read.
        public static IReadOnlyList<double> Read(string path)
        {
            try
            {
                using var file = TagLib.File.Create(path);
                var candidates = new List<string>();
                if (file.GetTag(TagLib.TagTypes.Id3v2) is TagLib.Id3v2.Tag id3)
                {
                    // GEOB frames. Not a cover, which is an attachment too, of maybe megabytes.
                    candidates.AddRange(id3.GetFrames<TagLib.Id3v2.AttachmentFrame>()
                        .Where(frame => frame.Type == TagLib.PictureType.NotAPicture)
                        .Select(frame => Encoding.ASCII.GetString(frame.Data.Data)));
                }
                if (file.GetTag(TagLib.TagTypes.Apple) is TagLib.Mpeg4.AppleTag apple && apple.GetDashBox("com.mixedinkey.mixedinkey", "cuepoints") is { } box)
                {
                    candidates.Add(box);
                }

                foreach (var candidate in candidates)
                {
                    if (Parse(candidate) is { Count: > 0 } cues)
                    {
                        return cues;
                    }
                }
            }
            catch (Exception e) when (e is TagLib.CorruptFileException or TagLib.UnsupportedFormatException or IOException or UnauthorizedAccessException)
            {
            }
            return [];
        }

        // Null when it isn't base64 JSON with cues.
        public static IReadOnlyList<double>? Parse(string base64)
        {
            ArgumentNullException.ThrowIfNull(base64);
            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64.Trim('\0', ' ', '\n', '\r')));
                using var document = JsonDocument.Parse(json);
                if (!document.RootElement.TryGetProperty("cues", out var cues) || cues.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }
                return cues.EnumerateArray()
                    .Where(cue => cue.TryGetProperty("time", out var time) && time.ValueKind == JsonValueKind.Number)
                    .Select(cue => cue.GetProperty("time").GetDouble() / 1000)
                    .ToList();
            }
            catch (Exception e) when (e is FormatException or JsonException or InvalidOperationException)
            {
                return null;
            }
        }
    }
}
