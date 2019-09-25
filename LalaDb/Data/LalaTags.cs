using System.Collections.Generic;
using System.Linq;

namespace LalaDb.Data
{
    public class LalaTags : LibLala.TagReader.Tags
    {
        public LalaTags(Track track) : base(track.Title ?? "", track.Path ?? "")
        {
            if (track.Album is { })
            {
                Album = track.Album.Name ?? "";
            }
            if (track.ArtistTracks is { })
            {
                Artist = SafeStringList(track.ArtistTracks.Select(x => x.Artist?.Name).ToList());
            }
            Comment = track.Comment ?? "";
            DiscNumber = (uint)track.DiscNumber;
            if (track.GenreTracks is { })
            {
                genre = SafeStringList(track.GenreTracks.Select(x => x.Genre?.Name).ToList());
            }
            length = track.Length;
            Track = (uint)track.TrackNumber;
            Year = (uint)track.Year;
            TrackId = track.TrackId;
        }

        private List<string> SafeStringList(List<string?> unsafeList)
        {
            var safeList = new List<string>();
            foreach (var value in unsafeList)
            {
                if (value is { }) safeList.Add(value);
            }
            return safeList;

        }
    }
}
