using System;
using System.IO;

namespace LibLala.FileExporter
{
    public static class ExportPreparer
    {
        public static string PrepareDestinationDirectory(string destination)
        {
            var exporterDirectory = new ExporterDirectory();
            var directoryPreparer = new DirectoryPreparer(exporterDirectory);
            return directoryPreparer.PrepareDestination(destination);
        }

        public static string GetNewFilename(int pos, int max, string path)
        {
            var digits = (int)Math.Floor(Math.Log10(max) + 1);
            var format = $"D{digits}";
            var retval = $"{pos.ToString(format)}-{Path.GetFileName(path)}";
            return retval;
        }
    }
}
