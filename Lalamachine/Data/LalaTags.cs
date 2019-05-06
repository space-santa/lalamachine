using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lalamachine.Data
{
    public class LalaTags : LibLala.TagReader.Tags
    {
        public LalaTags(Track track)
        {
            if (track.Album != null)
            {
                album = track.Album.Name;
            }
            if (track.ArtistTracks != null)
            {
                Artist = track.ArtistTracks.Select(x => x.Artist.Name).ToArray();
            }
            comment = track.Comment;
            discNumber = (uint)track.DiscNumber;
            if (track.GenreTracks != null)
            {
                genre = track.GenreTracks.Select(x => x.Genre.Name).ToArray();
            }
            length = track.Length;
            path = track.Path;
            title = track.Title;
            this.track = (uint)track.TrackNumber;
            year = (uint)track.Year;
        }
    }
}
