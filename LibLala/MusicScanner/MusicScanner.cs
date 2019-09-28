using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibLala.MusicScanner
{
    public static class DirectoryProcessor
    {
        public static void ProcessDirectory(string path, IMusicDatabase db)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(paramName: nameof(path));
            }

            if (db is null)
            {
                throw new ArgumentNullException(paramName: nameof(db));
            }

            db.EnsureDatabase();

            foreach (var file in DirSearch(path))
            {
                var tags = new LibLala.LibLalaTagReader.TagReader().Read(file);
                db.AddTagsToDatabase(tags);
            }

            db.SaveChanges();
        }

        private static IEnumerable<string> DirSearch(string basePath)
        {
            var files = Directory.EnumerateFiles(basePath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".mp3", StringComparison.Ordinal) || s.EndsWith(".m4a", StringComparison.Ordinal));
            return files;
        }
    }
}
