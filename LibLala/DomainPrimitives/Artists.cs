using System;
using System.Collections.Generic;
using System.Linq;

namespace LibLala.DomainPrimitives
{
    internal class Artists : IComparable<Artists>
    {
        private List<Artist> _artists;

        public Artists(List<string> artistStrings)
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

        public Artists(string artistsCsvString)
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

        public int CompareTo(Artists other)
        {
            return string.Compare(ToCsvString(), other.ToCsvString(), StringComparison.Ordinal);
        }
    }
}
