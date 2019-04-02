using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotnet.Data
{
    public class LalaTags : TagReader.Tags
    {
        public LalaTags(Track track, LalaContext context)
        {
            album = context.Albums.Single(x => x.AlbumId == track.AlbumId).Name;

            var aots = context.ArtistsOfTracks.Where(x => x.TrackId == track.TrackId).ToArray();
            List<string> tmpArtists = new List<string>();
            foreach (var aot in aots)
            {
                var artist = context.Artists.Single(x => x.ArtistId == aot.ArtistId).Name;
                tmpArtists.Add(artist);
            }
            Artist = tmpArtists.ToArray();

            comment = track.Comment;
            discNumber = (uint)track.DiscNumber;

            var gots = context.GenresOfTracks.Where(x => x.TrackId == track.TrackId).ToArray();
            List<string> tmpGenres = new List<string>();
            foreach (var got in gots)
            {
                var genre = context.Genres.Single(x => x.GenreId == got.GenreId).Name;
                tmpGenres.Add(genre);
            }
            genre = tmpGenres.ToArray();

            length = track.Length;
            path = track.Path;
            title = track.Title;
            this.track = (uint)track.TrackNumber;
            year = (uint) track.Year;
        }
    }
}
