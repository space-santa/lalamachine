using System;
using System.Collections.Generic;
using System.Linq;

namespace LibLala.DomainPrimitives
{
    public class GenresOfTrack : IComparable<GenresOfTrack>
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
            if (other is null) { throw new ArgumentNullException(paramName: nameof(other)); }
            return string.Compare(ToCsvString(), other.ToCsvString(), StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(GenresOfTrack left, GenresOfTrack right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(GenresOfTrack left, GenresOfTrack right)
        {
            return !(left == right);
        }

        public static bool operator <(GenresOfTrack left, GenresOfTrack right)
        {
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(GenresOfTrack left, GenresOfTrack right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(GenresOfTrack left, GenresOfTrack right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(GenresOfTrack left, GenresOfTrack right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
