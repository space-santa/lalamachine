using Newtonsoft.Json;

namespace Lalamachine
{
    class MetadataProvider
    {
        public string metaDataAsJsonString(string path)
        {
            var tags = LibLala.TagReader.TagReader.Read(path);
            return JsonConvert.SerializeObject(tags);
        }
    }
}
