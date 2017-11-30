using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiffBlit.Core.Delta;
using DiffBlit.Core.Extensions;
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
        public string Name { get; set; } = string.Empty;

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


        // TODO: set working directory to package path! config paths will be relative to this. restore to previous afterwards?
        //Directory.SetCurrentDirectory(outputPath);

        // TODO: case-insensitive paths for now, should path case changes constitute a move? probably...

        /// <summary>
        /// Generates package content differentials between the source and target snapshots in the specified output path.
        /// </summary>
        /// <param name="repo">The initial repository configuration.</param>
        /// <param name="sourcePath">The source content path.</param>
        /// <param name="targetPath">The target content path.</param>
        /// <param name="deltaPath">The delta content path; these files should be uploaded to the repo.</param>
        /// <param name="deltaExtension">The delta content file extension.</param>
        /// <returns>The created package information.</returns>
        public static Package Create(Repository repo, string sourcePath, string targetPath, string deltaPath, string deltaExtension = ".jar")
        {
            Package package = new Package();
            string packagePath = Path.Combine(deltaPath, package.Id.ToString());

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
                var sourceHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in sourceSnapshot.Content)
                {
                    sourcePaths[info.Path.ToLowerInvariant()] = info;
                    var hash = BitConverter.ToString(info.Hash);
                    if (!sourceHashes.ContainsKey(hash))
                    {
                        sourceHashes[hash] = new List<FileInformation>();
                    }
                    sourceHashes[hash].Add(info);
                }

                var targetPaths = new Dictionary<string, FileInformation>();
                var targetHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in targetSnapshot.Content)
                {
                    targetPaths[info.Path.ToLowerInvariant()] = info;
                    var hash = BitConverter.ToString(info.Hash);
                    if (!targetHashes.ContainsKey(hash))
                    {
                        targetHashes[hash] = new List<FileInformation>();
                    }
                    targetHashes[hash].Add(info);
                }

                #endregion

                // TODO: clean up and centralize path combines
                // create package directory
                Directory.CreateDirectory(packagePath);

                foreach (var file in targetSnapshot.Content)
                {
                    string hash = BitConverter.ToString(file.Hash);
                    bool sourcePathMatch = sourcePaths.ContainsKey(file.Path.ToLowerInvariant());
                    bool sourceHashMatch = sourceHashes.ContainsKey(hash);
                    bool sourceHashSingleMatch = sourceHashMatch && sourceHashes[hash].Count == 1;
                    bool sourceHashMultipleMatch = sourceHashMatch && sourceHashes[hash].Count > 1;
                    bool targetHashSingleMatch = targetHashes[hash].Count == 1;

                    // unchanged file (TODO: handle renames)
                    if (sourcePathMatch && sourceHashMatch)
                        continue;

                    // changed files (same path, different hash)
                    if (sourcePathMatch && !sourceHashMatch)
                    {
                        string tempDeltaFile = Path.GetTempFileName();
                        try
                        {
                            // generate delta patch
                            new XDeltaPatcher().Create(Path.Combine(Environment.CurrentDirectory, sourcePath, file.Path), Path.Combine(Environment.CurrentDirectory, targetPath, file.Path), tempDeltaFile);
                            var content = Content.Create(tempDeltaFile, packagePath, chunkSize: 16);    // TODO: small chunk size for testing part logic, remove
                            package.Actions.Add(new PatchAction(file.Path, file.Path, PatchAlgorithmType.XDelta, content));
                        }
                        finally
                        {
                            File.Delete(tempDeltaFile);
                        }
 
                        continue;
                    }

                    // TODO: search for added files (path and hash that exists in target but not source)
                    if (!sourcePathMatch && !sourceHashMatch)    // TODO: fix
                    {
                        var content = Content.Create(Path.Combine(Environment.CurrentDirectory, targetPath, file.Path), packagePath, compress: true, chunkSize: 16);    // TODO: small chunk size for testing part logic, remove
                        package.Actions.Add(new AddAction(file.Path, content));

                        continue;
                    }

                    // moved files (different path, single hash match on both sides)
                    if (!sourcePathMatch && sourceHashSingleMatch && targetHashSingleMatch)
                    {
                        package.Actions.Add(new MoveAction(sourceHashes[BitConverter.ToString(file.Hash)][0].Path, file.Path));
                        continue;
                    }

                    // search for copied files (multiple hash matches, different paths)
                    if (!sourcePathMatch && sourceHashMultipleMatch)
                    {
                        package.Actions.Add(new CopyAction(sourceHashes[hash][0].Path, file.Path));
                        continue;
                    }

                    // TODO: if reached here, must be removed file?
                    // TODO: duplicate
                }

                // deleted files (path that exists in source but not target)
                foreach (var file in sourceSnapshot.Content)
                {
                    if (!targetPaths.ContainsKey(file.Path.ToLowerInvariant()))
                    {
                        package.Actions.Add(new RemoveAction(file.Path));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Directory.Delete(packagePath);
                throw;
            }
 
            return package;
        }
    }
}
