using System;
using System.Collections.Generic;
using System.Linq;

namespace LibLala.DomainPrimitives
{
    public class ArtistsOfTrack : IComparable<ArtistsOfTrack>
    {
        private List<ArtistName> _artists;

        public ArtistsOfTrack(List<string> artistStrings)
        {
            if (artistStrings is null)
            {
                throw new ArgumentNullException(paramName: nameof(artistStrings));
            }

            _artists = new List<ArtistName>();

            foreach (var artistString in artistStrings)
            {
                _artists.Add(new ArtistName(artistString));
            }

            EnsureDistinctArtists();
        }

        public ArtistsOfTrack(string artistsCsvString)
        {
            if (string.IsNullOrEmpty(artistsCsvString))
            {
                throw new ArgumentNullException(paramName: nameof(artistsCsvString));
            }

            _artists = new List<ArtistName>();

            var localArtist = artistsCsvString.Split(',');
            for (var i = 0; i < localArtist.Length; ++i)
            {
                _artists.Add(new ArtistName(localArtist[i]));
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

        public static bool operator ==(ArtistsOfTrack left, ArtistsOfTrack right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ArtistsOfTrack left, ArtistsOfTrack right)
        {
            return !(left == right);
        }

        public static bool operator <(ArtistsOfTrack left, ArtistsOfTrack right)
        {
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(ArtistsOfTrack left, ArtistsOfTrack right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(ArtistsOfTrack left, ArtistsOfTrack right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(ArtistsOfTrack left, ArtistsOfTrack right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
