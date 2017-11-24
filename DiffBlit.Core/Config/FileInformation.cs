using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    [JsonObject(MemberSerialization.OptOut)]
    public class FileInformation
    {
        [JsonProperty(Required = Required.Always)]
        public string Path { get; set; }

        [JsonProperty(Required = Required.Always)]
        public byte[] Hash { get; set; }

        // TODO: DateTime if we wish to track that

        //[JsonIgnore]
        //public string Name => IsDirectory ? System.IO.Path.GetDirectoryName(Path) : System.IO.Path.GetFileName(Path);

        //[JsonIgnore]
        //public long Size { get; }

        //[JsonIgnore]
        //public bool IsDirectory => Path.EndsWith("/") || Path.EndsWith("\\");
    }
}
