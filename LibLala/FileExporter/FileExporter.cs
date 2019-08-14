using System.IO;

namespace LibLala.FileExporter
{
    public class FileExporter
    {
        public static void ExportPlaylist(string destination, string[] files)
        {
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
