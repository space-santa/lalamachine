using System;

namespace LibLala.DomainPrimitives
{
    public enum PlaylistColumnType
    {
        Track, Disc, Title, Length, Comment, Album, Artist, Genre, Year
    }
    public class PlaylistColumn
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public PlaylistColumn(string? headerContent)
        {
            if (string.IsNullOrEmpty(headerContent))
            {
                throw new ArgumentNullException(paramName: nameof(headerContent));
            }

            var title = headerContent.Trim();
            switch (title)
            {
                case "Track":
                    ColumnType = PlaylistColumnType.Track;
                    break;
                case "Disc":
                    ColumnType = PlaylistColumnType.Disc;
                    break;
                case "Title":
                    ColumnType = PlaylistColumnType.Title;
                    break;
                case "Length":
                    ColumnType = PlaylistColumnType.Length;
                    break;
                case "Comment":
                    ColumnType = PlaylistColumnType.Comment;
                    break;
                case "Artist":
                    ColumnType = PlaylistColumnType.Artist;
                    break;
                case "Album":
                    ColumnType = PlaylistColumnType.Album;
                    break;
                case "Genre":
                    ColumnType = PlaylistColumnType.Genre;
                    break;
                case "Year":
                    ColumnType = PlaylistColumnType.Year;
                    break;
                default:
                    throw new ArgumentException(paramName: headerContent, message: "Unknown column title.");
            }
        }

        public PlaylistColumnType ColumnType { get; }
    }
}
