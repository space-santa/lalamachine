using System;
using System.IO;

namespace LibLala.FileExporter
{
    public interface IExporterDirectory
    {
        bool Exists(string destination);
        void CreateDirectory(string destination);
        string GetDirectorySuffix();
    }

    public class ExporterDirectory : IExporterDirectory
    {
        private readonly TimeProvider _time;

        public ExporterDirectory() : this(TimeProvider.System)
        {
        }

        public ExporterDirectory(TimeProvider time)
        {
            _time = time;
        }

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
            return _time.GetLocalNow().ToString("-yyyyMMdd_HHmmss", LibLala.Constants.CULTURE);
        }
    }
}
