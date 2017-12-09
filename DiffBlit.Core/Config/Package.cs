using System;
using System.Collections.Generic;
using System.IO;
using DiffBlit.Core.Delta;
using DiffBlit.Core.Utilities;
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
        public List<IAction> Actions { get; } = new List<IAction>();

        /// <summary>
        /// Applies package actions in-order against the target directory using the contents of the package directory.
        /// </summary>
        /// <param name="source">The source snapshot.</param>
        /// <param name="target">The target snapshot.</param>
        /// <param name="packageDirectory">The package contents directory.</param>
        /// <param name="targetDirectory">The target base directory.</param>
        public void Apply(Snapshot source, Snapshot target, FilePath packageDirectory, FilePath targetDirectory)
        {
            // TODO: argument validation

            // validate source content
            source.Validate(targetDirectory);

            // create temporary directory to contain target backup in case of rollback
            FilePath backupDirectory = Utility.GetTempDirectory();

            try
            {
                // backup content to be modified
                foreach (var action in Actions)
                {
                    FilePath targetPath = Path.Combine(targetDirectory, action.TargetPath);
                    if (File.Exists(targetPath))
                    {
                        FilePath backupPath = Path.Combine(backupDirectory, action.TargetPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                        File.Copy(targetPath, backupPath, true);
                    }
                }

                // apply the actions in-order
                foreach (var action in Actions)
                {
                    action.Run(new ActionContext(targetDirectory, packageDirectory));
                }

                // validate modified content
                target.Validate(targetDirectory);
            }
            catch
            {
                // if failure is detected, delete any existing targets and restore any backups
                foreach (var action in Actions)
                {
                    File.Delete(Path.Combine(targetDirectory, action.TargetPath));
                    FilePath backupPath = Path.Combine(backupDirectory, action.TargetPath);
                    if (File.Exists(backupPath))
                        File.Copy(backupPath, Path.Combine(targetDirectory, action.TargetPath), true);
                }

                // re-throw the original exception
                throw;
            }
            finally
            {
                Directory.Delete(backupDirectory, true);
            }
        }

        /// <summary>
        /// Generates package content differentials between the source and target snapshots in the specified output path.
        /// </summary>
        /// <param name="repo">The initial repository configuration.</param>
        /// <param name="sourcePath">The absolute local source content path.</param>
        /// <param name="targetPath">The absolute localtarget content path.</param>
        /// <param name="deltaPath">The delta content path; these files should be uploaded to the repo.</param>
        /// <param name="settings">The package settings.</param>
        /// <returns>The created package information.</returns>
        public static Package Create(Repository repo, FilePath sourcePath, FilePath targetPath, FilePath deltaPath, PackageSettings settings = null)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException("Source path does not exist.");

            if (!Directory.Exists(targetPath))
                throw new DirectoryNotFoundException("Target path does not exist.");

            if (!Directory.Exists(deltaPath))
                throw new DirectoryNotFoundException("Delta path does not exist.");

            // use the specified settings or initialize with defaults if null
            PackageSettings pkgSettings = settings ?? new PackageSettings();

            // generate the empty package and create it's content directory based on its ID
            Package package = new Package();
            FilePath packagePath = Path.Combine(deltaPath, package.Id.ToString());
            Directory.CreateDirectory(packagePath);

            // TODO: automatic detection of relative versus absolute paths (absolute should override and not use base path if specified)

            try
            {
                #region Snapshots

                // generate source snapshot, use existing one in repo if content matches
                var sourceSnapshot = new Snapshot(sourcePath);
                if (repo.Snapshots.Contains(sourceSnapshot))
                {
                    sourceSnapshot = repo.Snapshots[repo.Snapshots.IndexOf(sourceSnapshot)];
                }
                else repo.Snapshots.Add(sourceSnapshot);

                // generate target snapshot, use existing one in repo if content matches
                var targetSnapshot = new Snapshot(targetPath);
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
                var sourcePaths = new Dictionary<FilePath, FileInformation>();
                var sourceFiles = new Dictionary<FilePath, List<FileInformation>>();
                var sourceHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in sourceSnapshot.Files)
                {
                    sourcePaths[info.Path] = info;

                    FilePath fileName = Path.GetFileName(info.Path);
                    if (!sourceFiles.ContainsKey(fileName))
                    {
                        sourceFiles[fileName] = new List<FileInformation>();
                    }
                    sourceFiles[fileName].Add(info);

                    if (info.Hash != null)
                    {
                        var hash = BitConverter.ToString(info.Hash);
                        if (!sourceHashes.ContainsKey(hash))
                        {
                            sourceHashes[hash] = new List<FileInformation>();
                        }
                        sourceHashes[hash].Add(info);
                    }
                }

                var targetPaths = new Dictionary<FilePath, FileInformation>();
                var targetFiles = new Dictionary<FilePath, List<FileInformation>>();
                var targetHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in targetSnapshot.Files)
                {
                    targetPaths[info.Path] = info;

                    FilePath fileName = Path.GetFileName(info.Path);
                    if (!targetFiles.ContainsKey(fileName))
                    {
                        targetFiles[fileName] = new List<FileInformation>();
                    }
                    targetFiles[fileName].Add(info);

                    if (info.Hash != null)
                    {
                        var hash = BitConverter.ToString(info.Hash);
                        if (!targetHashes.ContainsKey(hash))
                        {
                            targetHashes[hash] = new List<FileInformation>();
                        }
                        targetHashes[hash].Add(info);
                    }
                }

                #endregion

                foreach (var file in targetSnapshot.Files)
                {
                    FilePath sourceFilePath = Path.Combine(sourcePath, file.Path);
                    FilePath targetFilePath = Path.Combine(targetPath, file.Path);
                    bool sourcePathMatch = sourcePaths.ContainsKey(file.Path);

                    // unchanged empty directory
                    if (file.Path.IsDirectory & sourcePathMatch)
                    {
                        continue;
                    }

                    // added empty directory
                    if (file.Path.IsDirectory && !sourcePathMatch)
                    {
                        package.Actions.Add(new AddAction(file.Path, null));
                        continue;
                    }

                    string hash = BitConverter.ToString(file.Hash);
                    bool sourceHashMatch = sourceHashes.ContainsKey(hash);
                    bool sourceHashSingleMatch = sourceHashMatch && sourceHashes[hash].Count == 1;
                    bool targetHashSingleMatch = targetHashes[hash].Count == 1;

                    // unchanged file
                    if (sourcePathMatch && sourceHashMatch)
                        continue;

                    // changed files (same path, different hash)
                    if (sourcePathMatch && !sourceHashMatch)
                    {
                        FilePath tempDeltaFile = Path.GetTempFileName();
                        try
                        {
                            // TODO: patches should already be compressed, create a temp copy of the package settings with compression disabled
                            // generate delta patch to temp location and generate content
                            Utility.GetPatcher(pkgSettings.PatchAlgorithmType).Create(sourceFilePath, targetFilePath, tempDeltaFile);
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

                // deleted files & directories (path that exists in source but not target)
                foreach (var file in sourceSnapshot.Files)
                {
                    if (!targetPaths.ContainsKey(file.Path))
                    {
                        package.Actions.Add(new RemoveAction(file.Path));
                    }
                }

                // TODO: handle empty directory deletions caused by files getting deleted within
                // loop through destination directories and delete any that don't exist in source

                // TODO: handle file/directory renames that consist of case-changes only
                // loop through all files (should be 1:1 now) and rename if case mismatches (implies underlying directories are also renamed where applicable)
            }
            catch (Exception ex)
            {
                Directory.Delete(packagePath, true);
            }
 
            return package;
        }
    }
}
