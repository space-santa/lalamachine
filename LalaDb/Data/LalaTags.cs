using System;
using System.Collections.Generic;
using System.Linq;
using LibLala.LibLalaTagReader;
using LibLala.Utils;

namespace LalaDb.Data
{
    public class LalaTags : LibLalaTags
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public static LalaTags Build(Track track)
        {
            ArgumentNullException.ThrowIfNull(track);

            if (track.Title is null || track.Path is null)
            {
                throw new System.ArgumentException(paramName: nameof(track), message: "A valid track must have title and path.");
            }

            var builder = new LibLalaTagsBuilder(track.Title, track.Path, track.Length);

            // Now comes a bunch of `track!`. For some reason the compiler doesn't notice that we never get here if track is null. First check in this method.
            // TODO: Make this not necessary.
            if (track!.Album?.Name is { }) { builder = builder.WithAlbum(track.Album.Name); }
            if (track!.ArtistTracks is { }) { builder = builder.WithArtist(ArtistTackListToStringList(track?.ArtistTracks)); }
            if (track!.Comment is { }) { builder = builder.WithComment(track.Comment); }
            if (track!.DiscNumber is { }) { builder = builder.WithDiscNumber((uint)track.DiscNumber); }
            if (track!.GenreTracks is { }) { builder = builder.WithGenre(GenreTackListToStringList(track?.GenreTracks)); }
            if (track!.TrackId is { }) { builder = builder.WithTrackId(track.TrackId); }
            if (track!.TrackNumber is { }) { builder = builder.WithTrackNumber((uint)track.TrackNumber); }
            if (track!.Year is { }) { builder = builder.WithYear(track.Year); }
            var tags = builder.Build();

            return new LalaTags(tags);
        }
        private LalaTags(LibLalaTags tags) : base(tags)
        {
        }

        private static IList<string> GenreTackListToStringList(IList<GenreTrack>? genreTracks)
        {
            if (genreTracks is null)
            {
                return new List<string>();
            }

            return StringUtils.SafeStringList(genreTracks.Select(x => x.Genre?.Name).ToList());
        }

        private static IList<string> ArtistTackListToStringList(IList<ArtistTrack>? artistTracks)
        {
            if (artistTracks is null)
            {
                return new List<string>();
            }
            return StringUtils.SafeStringList(artistTracks.Select(x => x.Artist?.Name).ToList());
        }
    }
}
