using Newtonsoft.Json;

namespace Lalamachine
{
    class MetadataProvider
    {
        public string metaDataAsJsonString(string path)
        {
            var tags = new LibLala.TagReader.TagReader().Read(path);
            return JsonConvert.SerializeObject(tags);
        }
    }
}
