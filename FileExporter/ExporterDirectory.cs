using System;
using System.IO;

namespace FileExporter
{
    public interface IExporterDirectory
    {
        bool Exists(string destination);
        void CreateDirectory(string destination);
        string GetDirectorySuffix();
    }

    public class ExporterDirectory : IExporterDirectory
    {
        public bool Exists(string destination)
        {
            return Directory.Exists(destination);
        }

        public void CreateDirectory(string destination)
        {
            Directory.CreateDirectory(destination);
        }

        public string GetDirectorySuffix()
        {
            return DateTime.Now.ToString("-yyyyMMdd_HHmmss");
        }
    }
}
