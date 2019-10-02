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
            ColumnType = title switch
            {
                "Track" => PlaylistColumnType.Track,
                "Disc" => PlaylistColumnType.Disc,
                "Title" => PlaylistColumnType.Title,
                "Length" => PlaylistColumnType.Length,
                "Comment" => PlaylistColumnType.Comment,
                "Artist" => PlaylistColumnType.Artist,
                "Album" => PlaylistColumnType.Album,
                "Genre" => PlaylistColumnType.Genre,
                "Year" => PlaylistColumnType.Year,
                _ => throw new ArgumentException(paramName: headerContent, message: "Unknown column title."),
            };
        }

        public PlaylistColumnType ColumnType { get; }
    }
}
