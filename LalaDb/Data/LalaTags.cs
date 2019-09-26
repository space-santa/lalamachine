using System.Collections.Generic;
using System.Linq;
using LibLala.Utils;

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
                Artist = Utils.SafeStringList(track.ArtistTracks.Select(x => x.Artist?.Name).ToList());
            }
            Comment = track.Comment ?? "";
            DiscNumber = (uint)track.DiscNumber;
            if (track.GenreTracks is { })
            {
                genre = Utils.SafeStringList(track.GenreTracks.Select(x => x.Genre?.Name).ToList());
            }
            length = track.Length;
            Track = (uint)track.TrackNumber;
            Year = (uint)track.Year;
            TrackId = track.TrackId;
        }
    }
}
