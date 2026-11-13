using System;
using System.IO;

namespace LibLala.LibLalaTagReader
{
    // What's shown of the track that plays: in the player, and in the system's media controls. Artist and album can be
    // empty. The cover is the picture in the track's tags as it's stored, like a JPEG, null without one.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The picture as it's stored, it's handed on as it is.")]
    public sealed record TrackInfo(string Title, string Artist, string Album, byte[]? Cover)
    {
        public static readonly TrackInfo None = new("", "", "", null);

        // With one read of the file, it can be on a network drive. A track without a title, or one whose tags can't be read,
        // by its file name.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TagLib throws all kinds of exceptions for tags it can't make sense of, like ArgumentOutOfRangeException. Such a track plays all the same, it's shown by its file name.")]
        public static TrackInfo Read(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            if (path.Length == 0)
            {
                return None;
            }

            try
            {
                using var file = TagLib.File.Create(path);
                var tags = new LibLalaTags(file, Path.GetFullPath(path));
                return new TrackInfo(tags.Title, tags.ArtistString, tags.Album, CoverArt.Of(file.Tag));
            }
            catch (Exception)
            {
                return new TrackInfo(Path.GetFileNameWithoutExtension(path), "", "", null);
            }
        }
    }
}
