using System;
using System.Collections.Generic;
using System.Linq;

namespace LibLala.DomainPrimitives
{
    internal class GenresOfTrack : IComparable<GenresOfTrack>
    {
        private List<Genre> _genres;

        public GenresOfTrack(List<string> nameStrings)
        {
            if (nameStrings is null)
            {
                throw new ArgumentNullException(paramName: nameof(nameStrings));
            }

            _genres = new List<Genre>();

            foreach (var name in nameStrings)
            {
                _genres.Add(new Genre(name));
            }

            EnsureDistinctArtists();
        }

        public GenresOfTrack(string nameCsvString)
        {
            if (string.IsNullOrEmpty(nameCsvString))
            {
                throw new ArgumentNullException(paramName: nameof(nameCsvString));
            }

            _genres = new List<Genre>();

            var localName = nameCsvString.Split(',');
            for (var i = 0; i < localName.Length; ++i)
            {
                _genres.Add(new Genre(localName[i]));
            }

            EnsureDistinctArtists();
        }

        private void EnsureDistinctArtists()
        {
            _genres = _genres.Distinct().ToList();
        }

        public string ToCsvString()
        {
            return string.Join(", ", _genres);
        }

        public List<string> ToStringList()
        {
            var stringList = new List<string>();

            foreach (var genre in _genres)
            {
                stringList.Add(genre.ToString());
            }

            return stringList;
        }

        public int CompareTo(GenresOfTrack other)
        {
            return string.Compare(ToCsvString(), other.ToCsvString(), StringComparison.Ordinal);
        }
    }
}
