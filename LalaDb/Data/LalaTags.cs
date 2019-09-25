using System.Linq;

namespace LalaDb.Data
{
    public class LalaTags : LibLala.TagReader.Tags
    {
        public LalaTags(Track track) : base(track.Title, track.Path)
        {
            if (track.Album != null)
            {
                Album = track.Album.Name;
            }
            if (track.ArtistTracks != null)
            {
                Artist = track.ArtistTracks.Select(x => x.Artist.Name).ToArray();
            }
            Comment = track.Comment;
            DiscNumber = (uint)track.DiscNumber;
            if (track.GenreTracks != null)
            {
                genre = track.GenreTracks.Select(x => x.Genre.Name).ToArray();
            }
            length = track.Length;
            Track = (uint)track.TrackNumber;
            Year = (uint)track.Year;
            TrackId = track.TrackId;
        }
    }
}
