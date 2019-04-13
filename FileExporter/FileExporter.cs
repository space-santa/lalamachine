using System;
using System.IO;

namespace FileExporter
{
    public class FileExporter
    {
        public static void ExportPlaylist(string destination, string[] files)
        {
            var actualDestination = ExportPreparer.PrepareDestinationDirectory(destination);
            int position = 1;
            int totalNumber = files.Length;

            foreach (string file in files)
            {
                var targetName = ExportPreparer.GetNewFilename(position, totalNumber, file);
                var fullTargetPath = Path.Combine(actualDestination, targetName);
                File.Copy(file, fullTargetPath);
                position += 1;
            }
        }
    }
}
