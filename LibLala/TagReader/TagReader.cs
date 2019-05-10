using System;

namespace LibLala.TagReader
{
    public class TagReaderException : Exception
    {
        public TagReaderException(string message) : base(message)
        {
        }
    }

    public static class TagReader
    {
        public static Tags Read(string path)
        {
            path = LibLala.Utils.RemoveFilePrefix(path);

            if (path.Length == 0)
            {
                throw new TagReaderException("You must give a value for path.");
            }

            TagLib.File file;

            try
            {
                file = TagLib.File.Create(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new TagReaderException($"Can't open {path}.");
            }

            var tags = new Tags();
            tags.album = file.Tag.Album;
            var x = file.Tag.AlbumArtists;
            var y = file.Tag.Performers;

            var z = new string[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);

            tags.Artist = z;
            tags.comment = file.Tag.Comment;
            tags.discNumber = file.Tag.Disc;
            tags.genre = file.Tag.Genres;
            tags.duration = file.Properties.Duration;
            tags.path = System.IO.Path.GetFullPath(path);
            tags.title = file.Tag.Title;
            tags.track = file.Tag.Track;
            tags.year = file.Tag.Year;

            return tags;
        }
    }
}
