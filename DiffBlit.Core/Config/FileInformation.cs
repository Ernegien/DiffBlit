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
        // TODO: validate during set
        /// <summary>
        /// An absolute or relative path to a file or directory. Directories MUST end with a trailing slash to differentiate from extensionless files.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public FilePath Path { get; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public byte[] Hash { get; }

        // TODO: DateTime if we wish to track that as part of file/folder differentials (ChangeDateAction)

        // TODO: would be useful during validation, quick check that sizes match before scanning further
        //[JsonProperty(Required = Required.Always)]
        //public long Size => new FileInfo(Path).Length;

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
        public FileInformation(FilePath path, byte[] hash = null)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Hash = hash;
        }

        /// <inheritdoc />
        public bool Equals(FileInformation other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Path.Path == other.Path.Path && 
                StructuralComparisons.StructuralEqualityComparer.Equals(Hash, other.Hash);
        }

        #region Equality

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
            return Path.GetHashCode() ^ (Hash == null ? 0 : Hash.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Path;
        }

        #endregion
    }
}
