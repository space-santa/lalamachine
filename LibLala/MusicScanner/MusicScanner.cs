using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LibLala.MusicScanner
{
    public class FileScannedEventArgs : EventArgs
    {
        public FileScannedEventArgs(int count)
        {
            Count = count;
        }
        public int Count { get; }
    }

    public class DirectoryProcessor
    {
        readonly IMusicDatabase _db;
        readonly string _path;
        IEnumerable<string> Files { get => GetFiles(); }
        public int FileCount { get => Files.Count(); }

        public DirectoryProcessor(IMusicDatabase db, string path)
        {
            _db = db;
            _path = path;
        }

        public event EventHandler<FileScannedEventArgs>? FileScannedEvent;
        protected virtual void InvokeFileScannedEvent(int count)
        {
            var args = new FileScannedEventArgs(count);
            FileScannedEvent?.Invoke(this, args);
        }

        public void ProcessDirectory()
        {
            var stopwatch = Stopwatch.StartNew();

            _db.EnsureDatabase();

            int count = 0;
            foreach (var file in Files)
            {
                var tags = new LibLalaTagReader.TagReader().Read(file);
                _db.AddTagsToDatabase(tags);
                count += 1;
                InvokeFileScannedEvent(count);
            }

            _db.SaveChanges();
            var duration = stopwatch.Elapsed;
            Console.WriteLine($"Scanning took {duration.ToString()}");
        }

        private IEnumerable<string> GetFiles()
        {
            var files = Directory.EnumerateFiles(_path, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".mp3", StringComparison.Ordinal) || s.EndsWith(".m4a", StringComparison.Ordinal));
            return files;
        }
    }
}
