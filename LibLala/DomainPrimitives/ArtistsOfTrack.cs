using System;
using System.Collections.Generic;
using System.Linq;

namespace LibLala.DomainPrimitives
{
    internal class ArtistsOfTrack : IComparable<ArtistsOfTrack>
    {
        private List<Artist> _artists;

        public ArtistsOfTrack(List<string> artistStrings)
        {
            if (artistStrings is null)
            {
                throw new ArgumentNullException(paramName: nameof(artistStrings));
            }

            _artists = new List<Artist>();

            foreach (var artistString in artistStrings)
            {
                _artists.Add(new Artist(artistString));
            }

            EnsureDistinctArtists();
        }

        public ArtistsOfTrack(string artistsCsvString)
        {
            if (string.IsNullOrEmpty(artistsCsvString))
            {
                throw new ArgumentNullException(paramName: nameof(artistsCsvString));
            }

            _artists = new List<Artist>();

            var localArtist = artistsCsvString.Split(',');
            for (var i = 0; i < localArtist.Length; ++i)
            {
                _artists.Add(new Artist(localArtist[i]));
            }

            EnsureDistinctArtists();
        }

        private void EnsureDistinctArtists()
        {
            _artists = _artists.Distinct().ToList();
        }

        public string ToCsvString()
        {
            return string.Join(", ", _artists);
        }

        public List<string> ToStringList()
        {
            var stringList = new List<string>();

            foreach (var artist in _artists)
            {
                stringList.Add(artist.ToString());
            }

            return stringList;
        }

        public int CompareTo(ArtistsOfTrack other)
        {
            return string.Compare(ToCsvString(), other.ToCsvString(), StringComparison.Ordinal);
        }
    }
}
