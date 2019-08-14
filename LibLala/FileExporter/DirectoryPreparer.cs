namespace LibLala.FileExporter
{
    public class DirectoryPreparer
    {
        private readonly IExporterDirectory _exporterDirectory;

        public DirectoryPreparer(IExporterDirectory exporterDirectory)
        {
            _exporterDirectory = exporterDirectory;
        }

        public string PrepareDestination(string destination)
        {
            if (_exporterDirectory.Exists(destination))
            {
                destination += _exporterDirectory.GetDirectorySuffix();
            }

            _exporterDirectory.CreateDirectory(destination);
            return destination;
        }
    }
}
