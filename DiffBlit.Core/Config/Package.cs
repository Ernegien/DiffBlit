using System;
using System.Collections.Generic;
using System.IO;
using DiffBlit.Core.Delta;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Package
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
        public DateTime DateTime { get; } = DateTime.UtcNow;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid SourceSnapshotId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid TargetSnapshotId { get; set; } = Guid.NewGuid();

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
        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IList<IAction> Actions { get; } = new List<IAction>();

        /// <summary>
        /// Generates package content differentials between the source and target snapshots in the specified output path.
        /// </summary>
        /// <param name="repo">The initial repository configuration.</param>
        /// <param name="sourcePath">The absolute local source content path.</param>
        /// <param name="targetPath">The absolute localtarget content path.</param>
        /// <param name="deltaPath">The delta content path; these files should be uploaded to the repo.</param>
        /// <param name="settings">The package settings.</param>
        /// <returns>The created package information.</returns>
        public static Package Create(Repository repo, string sourcePath, string targetPath, string deltaPath, PackageSettings settings = null)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            if (deltaPath == null)
                throw new ArgumentNullException(nameof(deltaPath));

            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException("Source path does not exist.");

            if (!Directory.Exists(targetPath))
                throw new DirectoryNotFoundException("Target path does not exist.");

            // use the specified settings or initialize with defaults if null
            PackageSettings pkgSettings = settings ?? new PackageSettings();

            Package package = new Package();
            string packagePath = Path.Combine(deltaPath, package.Id.ToString());

            // TODO: automatic detection of relative versus absolute paths (absolute should override and not use base path if specified)

            try
            {
                #region Snapshots

                // generate source snapshot, use existing one in repo if content matches
                var sourceSnapshot = Snapshot.Create(sourcePath);
                if (repo.Snapshots.Contains(sourceSnapshot))
                {
                    sourceSnapshot = repo.Snapshots[repo.Snapshots.IndexOf(sourceSnapshot)];
                }
                else repo.Snapshots.Add(sourceSnapshot);

                // generate target snapshot, use existing one in repo if content matches
                var targetSnapshot = Snapshot.Create(targetPath);
                if (repo.Snapshots.Contains(targetSnapshot))
                {
                    targetSnapshot = repo.Snapshots[repo.Snapshots.IndexOf(targetSnapshot)];
                }
                else repo.Snapshots.Add(targetSnapshot);

                // link snapshots to the package
                package.SourceSnapshotId = sourceSnapshot.Id;
                package.TargetSnapshotId = targetSnapshot.Id;

                #endregion

                #region Lookup Tables

                // generate lookup tables
                var sourcePaths = new Dictionary<string, FileInformation>();
                var sourceFiles = new Dictionary<string, List<FileInformation>>();
                var sourceHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in sourceSnapshot.Files)
                {
                    sourcePaths[info.Path.ToLowerInvariant()] = info;

                    string fileName = Path.GetFileName(info.Path).ToLowerInvariant();
                    if (!sourceFiles.ContainsKey(fileName))
                    {
                        sourceFiles[fileName] = new List<FileInformation>();
                    }
                    sourceFiles[fileName].Add(info);

                    var hash = BitConverter.ToString(info.Hash);
                    if (!sourceHashes.ContainsKey(hash))
                    {
                        sourceHashes[hash] = new List<FileInformation>();
                    }
                    sourceHashes[hash].Add(info);
                }

                var targetPaths = new Dictionary<string, FileInformation>();
                var targetFiles = new Dictionary<string, List<FileInformation>>();
                var targetHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in targetSnapshot.Files)
                {
                    targetPaths[info.Path.ToLowerInvariant()] = info;

                    string fileName = Path.GetFileName(info.Path).ToLowerInvariant();
                    if (!targetFiles.ContainsKey(fileName))
                    {
                        targetFiles[fileName] = new List<FileInformation>();
                    }
                    targetFiles[fileName].Add(info);

                    var hash = BitConverter.ToString(info.Hash);
                    if (!targetHashes.ContainsKey(hash))
                    {
                        targetHashes[hash] = new List<FileInformation>();
                    }
                    targetHashes[hash].Add(info);
                }

                #endregion

                // create package directory
                Directory.CreateDirectory(packagePath);

                foreach (var file in targetSnapshot.Files)
                {
                    string hash = BitConverter.ToString(file.Hash);
                    string sourceFilePath = Path.Combine(sourcePath, file.Path);
                    string targetFilePath = Path.Combine(targetPath, file.Path);

                    bool sourcePathMatch = sourcePaths.ContainsKey(file.Path.ToLowerInvariant());
                    bool sourceHashMatch = sourceHashes.ContainsKey(hash);
                    bool sourceHashSingleMatch = sourceHashMatch && sourceHashes[hash].Count == 1;
                    bool targetHashSingleMatch = targetHashes[hash].Count == 1;

                    // unchanged file
                    if (sourcePathMatch && sourceHashMatch)
                        continue;

                    // changed files (same path, different hash)
                    if (sourcePathMatch && !sourceHashMatch)
                    {
                        string tempDeltaFile = Path.GetTempFileName();
                        try
                        {
                            // TODO: patches should already be compressed, create a temp copy of the package settings with compression disabled
                            // generate delta patch to temp location and generate content
                            new XDeltaPatcher().Create(sourceFilePath, targetFilePath, tempDeltaFile);
                            var content = Content.Create(tempDeltaFile, packagePath, pkgSettings);
                            package.Actions.Add(new PatchAction(file.Path, file.Path, pkgSettings.PatchAlgorithmType, content));
                        }
                        finally
                        {
                            File.Delete(tempDeltaFile);
                        }

                        continue;
                    }

                    // moved files (different path, single hash match on both sides)
                    if (!sourcePathMatch && sourceHashSingleMatch && targetHashSingleMatch)
                    {
                        package.Actions.Add(new MoveAction(sourceHashes[hash][0].Path, file.Path));
                        continue;
                    }

                    // search for copied files (multiple hash matches, different paths)
                    if (!sourcePathMatch && sourceHashMatch)
                    {
                        package.Actions.Add(new CopyAction(sourceHashes[hash][0].Path, file.Path));
                        continue;
                    }

                    // search for added files (path and hash that exists in target but not source)
                    if (!sourcePathMatch && !sourceHashMatch)
                    {
                        var content = Content.Create(targetFilePath, packagePath, pkgSettings);
                        package.Actions.Add(new AddAction(file.Path, content));
                        continue;
                    }

                    throw new InvalidOperationException("File action not found.");
                }

                // deleted files (path that exists in source but not target)
                foreach (var file in sourceSnapshot.Files)
                {
                    if (!targetPaths.ContainsKey(file.Path.ToLowerInvariant()))
                    {
                        package.Actions.Add(new RemoveAction(file.Path));
                    }
                }

                // TODO: handle file/directory renames that consist of case-changes only
                // TODO: handle empty directory creations/deletions
            }
            catch
            {
                Directory.Delete(packagePath, true);
            }
 
            return package;
        }
    }
}
