using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotnet
{
    class Exporter
    {
        public async void exportFilesAsync(string destination, string fileJson)
        {
            var files = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(fileJson);
            destination = LalaUtils.Utils.RemoveFilePrefix(destination);
            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = LalaUtils.Utils.RemoveFilePrefix(files[i]);
            }
            await Task.Run(() => FileExporter.FileExporter.ExportPlaylist(destination, files));
        }
    }
}
