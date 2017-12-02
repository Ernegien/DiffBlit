using System;
using System.Collections;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class FileInformation : IEquatable<FileInformation>
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Path { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public byte[] Hash { get; set; }

        // TODO: DateTime if we wish to track that

        //[JsonIgnore]
        //public string Name => IsDirectory ? System.IO.Path.GetDirectoryName(Path) : System.IO.Path.GetFileName(Path);

        //[JsonProperty(Required = Required.Always)]
        //public long Size => new FileInfo(Path).Length;

        //[JsonIgnore]
        //public bool IsDirectory => Path.EndsWith("/") || Path.EndsWith("\\");

        /// <summary>
        /// TODO: description
        /// </summary>
        public FileInformation()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="path"></param>
        /// <param name="hash"></param>
        public FileInformation(string path, byte[] hash)
        {
            Path = path;
            Hash = hash;
        }

        /// <inheritdoc />
        public bool Equals(FileInformation other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Path == other.Path && 
                StructuralComparisons.StructuralEqualityComparer.Equals(Hash, other.Hash);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FileInformation info)
            {
                return info.Equals(this);
            }
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Path.GetHashCode() ^ Hash.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Path;
        }
    }
}
