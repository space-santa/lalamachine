﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicScanner
{
    public class MusicScanner
    {
        public static void ProcessDirectory(string path, IMusicDatabase db)
        {
            db.EnsureDatabase();

            foreach (string file in DirSearch(path))
            {
                var tags = LibLala.TagReader.TagReader.Read(file);
                db.AddTagsToDatabase(tags);
            }

            db.SaveChanges();
        }

        private static IEnumerable<string> DirSearch(string basePath)
        {
            var files = Directory.EnumerateFiles(basePath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".mp3") || s.EndsWith(".m4a"));
            return files;
        }
    }
}
