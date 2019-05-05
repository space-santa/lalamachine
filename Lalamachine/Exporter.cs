using Qml.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lalamachine
{
    [Signal("exportFinished")]
    class Exporter
    {
        public async void exportFilesAsync(string destination, string fileJson)
        {
            var files = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(fileJson);
            destination = LibLala.Utils.RemoveFilePrefix(destination);
            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = LibLala.Utils.RemoveFilePrefix(files[i]);
            }
            await Task.Run(() => LibLala.FileExporter.FileExporter.ExportPlaylist(destination, files));
            this.ActivateSignal("exportFinished");
        }
    }
}
