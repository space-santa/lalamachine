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
            track?.Album?.Name ?? "",
            track?.Comment ?? "",
            track?.Year ?? null,
            track?.DiscNumber ?? null,
            track?.TrackNumber ?? null,
            track?.TrackId ?? null)
        {
            if (track is null)
            {
                return;
            }

            Length = track.Length;
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
