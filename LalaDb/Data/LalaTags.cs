using System.Collections.Generic;
using System.Linq;
using LibLala.Utils;

namespace LalaDb.Data
{
    public class LalaTags : LibLala.LibLalaTagReader.LibLalaTags
    {
        public LalaTags(Track? track) : base(
            track?.Title ?? "",
            track?.Path ?? "",
            GenreTackListToStringList(track?.GenreTracks),
            ArtistTackListToStringList(track?.ArtistTracks),
            track?.Album?.Name ?? "", track?.Comment ?? "")
        {
            if (track is null)
            {
                return;
            }

            DiscNumber = (uint)track.DiscNumber;
            Length = track.Length;
            Track = (uint)track.TrackNumber;
            Year = (uint)track.Year;
            TrackId = track.TrackId;
        }

        private static List<string> GenreTackListToStringList(List<GenreTrack>? genreTracks)
        {
            if (genreTracks is null)
            {
                return new List<string>();
            }

            return StringUtils.SafeStringList(genreTracks.Select(x => x.Genre?.Name).ToList());
        }

        private static List<string> ArtistTackListToStringList(List<ArtistTrack>? artistTracks)
        {
            if (artistTracks is null)
            {
                return new List<string>();
            }
            return StringUtils.SafeStringList(artistTracks.Select(x => x.Artist?.Name).ToList());
        }
    }
}
