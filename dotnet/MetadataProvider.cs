using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace dotnet
{
    class MetadataProvider
    {
        public string metaDataAsJsonString(string path)
        {
            var tags = TagReader.TagReader.Read(path);
            return JsonConvert.SerializeObject(tags);
        }
    }
}
