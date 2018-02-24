using System;
using System.Collections;
using System.Collections.Generic;
using DiffBlit.Core.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, ItemIsReference = false)]
    public class FileInformation : IEquatable<FileInformation>, IEqualityComparer<FileInformation>
    {
        // TODO: validate during set
        /// <summary>
        /// An absolute or relative path to a file or directory. Directories MUST end with a trailing slash to differentiate from extensionless files.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path Path { get; private set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public byte[] Hash { get; private set; }

        // TODO: DateTime if we wish to track that as part of file/folder differentials (ChangeDateAction)

        // TODO: would be useful during validation, quick check that sizes match before scanning further
        //[JsonProperty(Required = Required.Always)]
        //public long Size => new FileInfo(Path).Length;

        /// <summary>
        /// 
        /// </summary>
        [JsonConstructor]
        private FileInformation()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="path"></param>
        /// <param name="hash"></param>
        public FileInformation(Path path, byte[] hash = null)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Hash = hash;
        }

        #region Equality

        /// <inheritdoc />
        public bool Equals(FileInformation other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Path.Equals(other.Path) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(Hash, other.Hash);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileInformation)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Path.GetHashCode();
                if (Hash != null)
                {
                    hash = hash * 23 + Hash.GetHashCode();
                }
                return hash;
            }
        }

        //public int CompareTo(FileInformation other)
        //{
        //    throw new NotImplementedException();
        //}

        /// <inheritdoc />
        public override string ToString()
        {
            return Path;
        }

        /// <inheritdoc />
        public bool Equals(FileInformation x, FileInformation y)
        {
            return x.Equals(y);
        }

        /// <inheritdoc />
        public int GetHashCode(FileInformation obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }

        #endregion
    }
}
