using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Snapshot : IEquatable<Snapshot>
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [JsonConverter(typeof(VersionJsonConverter))]
        public Version Version { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<FileInformation> Files { get; } = new List<FileInformation>();

        /// <summary>
        /// Checks if the specified directory data matches the snapshot.
        /// </summary>
        /// <param name="directory"></param>
        public void Validate(FilePath directory)
        {
            foreach (var file in Files)
            {
                FilePath path = Path.Combine(directory, file.Path);

                if (file.Path.IsDirectory)
                {
                    if (!Directory.Exists(path))
                        throw new DataException($"Directory {path} does not exist.");
                }
                else if (!Utility.ComputeHash(path).IsEqual(file.Hash))
                    throw new DataException($"File validation failed for {path}");
            }
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public Snapshot()
        {
            
        }

        /// <summary>
        /// Generates a snapshot using the specified path information.
        /// </summary>
        /// <param name="path"></param>
        public Snapshot(FilePath path)
        {
            // add all files to the snapshot manifest
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var hash = Utility.ComputeHash(file);
                var relativePath = file.Substring(path.Path.Length + 1); // file path relative to the base content path specified
                Files.Add(new FileInformation(relativePath, hash));
            }

            // add all empty directories to the snapshot manifest
            foreach (var directory in Utility.GetEmptyDirectories(path))
            {
                // TODO: check path schema to determine whether to use forward or backwards slash?
                var relativePath = directory.Substring(path.Path.Length + 1) + "\\"; // file path relative to the base content path specified, including trailing slash
                Files.Add(new FileInformation(relativePath));
            }
        }

        /// <summary>
        /// Returns true if the snapshot contents are contained in the other.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(Snapshot other)
        {
            HashSet<FilePath> otherSnapshots = new HashSet<FilePath>();
            foreach (var file in other.Files)
            {
                otherSnapshots.Add(file.Path);
            }
            return Files.All(file => otherSnapshots.Contains(file.Path));
        }

        /// <inheritdoc />
        public bool Equals(Snapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Files, other.Files);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Snapshot) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Files != null ? Files.GetHashCode() : 0;
        }
    }
}
