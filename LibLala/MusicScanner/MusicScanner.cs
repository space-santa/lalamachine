using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LibLala.MusicScanner
{
    public static class DirectoryProcessor
    {
        public static void ProcessDirectory(string path, IMusicDatabase db)
        {
            var stopwatch = Stopwatch.StartNew();
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
                var tags = new LibLalaTagReader.TagReader().Read(file);
                db.AddTagsToDatabase(tags);
            }

            db.SaveChanges();
            var duration = stopwatch.Elapsed;
            Console.WriteLine($"Scanning took {duration.ToString()}");
        }

        private static IEnumerable<string> DirSearch(string basePath)
        {
            var files = Directory.EnumerateFiles(basePath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".mp3", StringComparison.Ordinal) || s.EndsWith(".m4a", StringComparison.Ordinal));
            return files;
        }
    }
}
