using System;
using System.IO;

namespace LibLala.FileExporter
{
    public static class PlaylistExporter
    {
        public static void ExportPlaylist(string destination, string[] files)
        {
            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentNullException(paramName: nameof(destination));
            }

            if (files is null)
            {
                throw new ArgumentNullException(paramName: nameof(files));
            }

            var actualDestination = ExportPreparer.PrepareDestinationDirectory(destination);
            var position = 1;
            var totalNumber = files.Length;

            foreach (var file in files)
            {
                var targetName = ExportPreparer.GetNewFilename(position, totalNumber, file);
                var fullTargetPath = Path.Combine(actualDestination, targetName);
                File.Copy(file, fullTargetPath);
                position += 1;
            }
        }
    }
}
