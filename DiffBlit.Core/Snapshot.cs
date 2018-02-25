using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Logging;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core
{
    /// <summary>
    /// Contains a list of all static file content.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, IsReference = true)]
    public class Snapshot : IEquatable<Snapshot>
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

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
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private Snapshot()
        {
            
        }

        /// <summary>
        /// Generates a snapshot using the specified path information.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="progressHandler">The optional handler to report progress to.</param>
        /// <param name="progressStatus">The optional progress status description.</param>
        public Snapshot(string directoryPath, ProgressChangedEventHandler progressHandler = null, string progressStatus = null)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath), "The directory path is required.");

            const string defaultProgressStatus = "Generating snapshot";

            progressHandler?.Invoke(this, new ProgressChangedEventArgs(0, progressStatus ?? defaultProgressStatus));

            // get list of files
            var files = new DirectoryInfo(directoryPath).GetFiles("*", SearchOption.AllDirectories);

            // maintain progress status
            long hashedBytes = 0;
            long totalBytes = files.Sum(file => file.Length);

            // TODO: process from biggest to smallest
            // add all files to the snapshot manifest
            Parallel.ForEach(files, file =>
            {
                // TODO: cleanup - attempts to snapshot log files as they're in use when ran within content directory
                try
                {
                    // compute the hash updating progress along the way if needed
                    var hash = progressHandler == null ? Utility.ComputeHash(file.FullName) :
                        Utility.ComputeHash(file.FullName, (sender, args) =>
                        {
                            // update hashed bytes count in a thread safe manner
                            Interlocked.Add(ref hashedBytes, (int)args.UserState);

                            // propagate current progress upstream
                            int progressPercentage = (int)(hashedBytes / (float)totalBytes * 100);
                            progressHandler.Invoke(this, new ProgressChangedEventArgs(progressPercentage, progressStatus ?? defaultProgressStatus));
                        });

                    Logger?.Debug("Computed hash for {0} of {1}", file, hash.ToBase64());

                    // ensure the add is thread safe
                    lock (((ICollection)Files).SyncRoot)
                    {
                        // file path relative to the base content path specified
                        Files.Add(new FileInformation(file.FullName.Substring(directoryPath.Length).TrimStart('/', '\\'), hash));
                    }
                }
                catch
                {
                   // do nothing for now
                }
            });

            // TODO: add back in
            // add all empty directories to the snapshot manifest
            //foreach (var directory in Utility.GetEmptyDirectories(directoryPath))
            //{
            //    // TODO: check path schema to determine whether to use forward or backwards slash?
            //    var relativePath = directory.Substring(directoryPath.Length + 1) + "\\"; // file path relative to the base content path specified, including trailing slash
            //    Files.Add(new FileInformation(relativePath));
            //}

            //  order files by path name
            Files = Files.OrderBy(p => p.Path.Name).ToList();
        }

        /// <summary>
        /// Returns true if the other snapshot is contained within.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(Snapshot other)
        {
            // TODO: log differences between the two
            return other.Files.All(file => Files.Contains(file));
        }

        /// <summary>
        /// Attempts to locate files with the matching hash.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns>Returns the list of files found.</returns>
        public List<FileInformation> FindFilesFromHash(byte[] hash)
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
