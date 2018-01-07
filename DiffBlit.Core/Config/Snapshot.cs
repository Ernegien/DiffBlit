using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, IsReference = true)]
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

        // TODO: progress indicator callback
        /// <summary>
        /// Checks if the specified directory data matches the snapshot.
        /// </summary>
        /// <param name="directory"></param>
        public void Validate(string directory)
        {
            Parallel.ForEach(Files, file =>
            {
                Path path = Path.Combine(directory, file.Path);

                if (file.Path.IsDirectory)
                {
                    if (!Directory.Exists(path))
                        throw new DataException($"Directory {path} does not exist.");
                }
                else if (!Utility.ComputeHash(path).IsEqual(file.Hash))
                    throw new DataException($"File validation failed for {path}");
            });
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private Snapshot()
        {
            
        }

        // TODO: progress indicator callback
        /// <summary>
        /// Generates a snapshot using the specified path information.
        /// </summary>
        /// <param name="repo"></param>
        public Snapshot(string directoryPath)
        {
            //Repository = repo ?? throw new ArgumentNullException(nameof(repo));

            // normalize directory path without the trailing slash
            directoryPath = directoryPath.TrimEnd('/', '\\');

            // add all files to the snapshot manifest
            // TODO: new ParallelOptions { MaxDegreeOfParallelism = 4 }
            object locker = new object();
            Parallel.ForEach(Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories), file =>
            {
                var hash = Utility.ComputeHash(file);
                var relativePath = file.Substring(directoryPath.Length + 1); // file path relative to the base content path specified

                lock (locker)
                {
                    Files.Add(new FileInformation(relativePath, hash));
                }
            });

            // add all empty directories to the snapshot manifest
            foreach (var directory in Utility.GetEmptyDirectories(directoryPath))
            {
                // TODO: check path schema to determine whether to use forward or backwards slash?
                var relativePath = directory.Substring(directoryPath.Length + 1) + "\\"; // file path relative to the base content path specified, including trailing slash
                Files.Add(new FileInformation(relativePath));
            }

            // order files by path name
            //Files = Files.OrderBy(p => p.Path.Name).ToList();
        }

        /// <summary>
        /// Returns true if the snapshot contents are contained in the other.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(Snapshot other)
        {
            return Files.All(file => other.Files.Contains(file));
        }

        /// <summary>
        /// Attempts to locate files with the matching hash.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns>Returns the list of files found.</returns>
        public List<FileInformation> FindFileFromHash(byte[] hash)
        {
            return Files.FindAll(file => file.Hash.IsEqual(hash));
        }

        /// <summary>
        ///Attempts to locate a file with the matching path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns the file or null if not found.</returns>
        public FileInformation FindFileFromPath(string path)
        {
            return Files.FirstOrDefault(file => file.Path.Equals(path));
        }

        /// <inheritdoc />
        public bool Equals(Snapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Files.Count == other.Files.Count && Files.All(file => other.Files.Contains(file));
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

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
