using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiffBlit.Core.Actions;
using DiffBlit.Core.IO;
using DiffBlit.Core.Logging;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core
{
    // TODO: add logging

    /// <summary>
    /// Contains the information necessary to translate between snapshots, plus any additional actions added to account for dynamic content such as configuration data.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Package
    {
        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        /// <summary>
        /// The parent repository the package belongs to.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Repository Repository { get; private set; }

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
        public Snapshot SourceSnapshot { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Snapshot TargetSnapshot { get; set; }

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
        [JsonProperty(Required = Required.Always, ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<IAction> Actions { get; } = new List<IAction>();

        //TODO: will need to disable multi-threaded processing of actions during application if specified
        //[JsonProperty(Required = Required.Default)]
        //public bool PreserveActionOrder { get; set; }

        // TODO: when order is not important, perform multi-threaded execution in order of biggest to smallest
        /// <summary>
        /// Applies package actions in-order against the target directory using the contents of the package directory.
        /// </summary>
        /// <param name="packageDirectory">The package contents directory.</param>
        /// <param name="targetDirectory">The target base directory.</param>
        /// <param name="validateBefore">Indicates whether or not the target directory should be validated before package application.</param>
        /// <param name="validateAfter">Indicates whether or not the target directory should be validated after package application.</param>
        /// <param name="progressHandler">The optional handler to report progress to.</param>
        /// <param name="progressStatus">The optional progress status description.</param>
        public void Apply(Path packageDirectory, Path targetDirectory, bool validateBefore = true, bool validateAfter = true, ProgressChangedEventHandler progressHandler = null, string progressStatus = null)
        {
            // validate source content
            if (validateBefore && !new Snapshot(targetDirectory, progressHandler, "Validating source content").Contains(SourceSnapshot))
            {
                throw new DataException("Directory contents do not match the source snapshot.");
            }

            // create temporary directory to contain target backup in case of rollback
            Path backupDirectory = Utility.GetTempDirectory();

            int processedCount = 0;

            // TODO: verify backup/rollback logic
            try
            {
                const string backupStepName = "Performing backup";
                progressHandler?.Invoke(this, new ProgressChangedEventArgs(0, backupStepName));

                // backup content to be modified, skipping NoAction types
                foreach (var action in Actions.Where(a => !(a is NoAction)))
                {
                    Path targetPath = Path.Combine(targetDirectory, action.TargetPath);
                    if (File.Exists(targetPath))
                    {
                        Path backupPath = Path.Combine(backupDirectory, action.TargetPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                        File.Copy(targetPath, backupPath, true);
                    }

                    // propagate current progress upstream
                    processedCount++;
                    int progressPercentage = (int)(processedCount / (float)Actions.Count * 100);
                    progressHandler?.Invoke(this, new ProgressChangedEventArgs(progressPercentage, backupStepName));
                }

                progressHandler?.Invoke(this, new ProgressChangedEventArgs(0, progressStatus));

                // apply the actions in-order
                processedCount = 0;
                foreach (var action in Actions)
                {
                    action.Run(new ActionContext(targetDirectory, packageDirectory));

                    // propagate current progress upstream
                    processedCount++;
                    int progressPercentage = (int)(processedCount / (float)Actions.Count * 100);
                    progressHandler?.Invoke(this, new ProgressChangedEventArgs(progressPercentage, progressStatus));
                }

                // TODO: selective file validation fed from list of changed files after patch application
                // validate modified content
                if (validateAfter && !new Snapshot(targetDirectory, progressHandler, "Validating output").Contains(TargetSnapshot))
                {
                    throw new DataException("Directory contents do not match the target snapshot.");
                }
            }
            catch (Exception ex)
            {
                const string rollbackStepName = "Performing rollback";
                progressHandler?.Invoke(this, new ProgressChangedEventArgs(0, rollbackStepName));
                processedCount = 0;

                // if failure is detected, delete any existing targets and restore any backups
                foreach (var action in Actions.Where(a => !(a is NoAction)))
                {
                    // TODO: cleanup
                    try
                    {
                        File.Delete(Path.Combine(targetDirectory, action.TargetPath));
                    }
                    catch (Exception e)
                    {
                 
                    }
       
                    Path backupPath = Path.Combine(backupDirectory, action.TargetPath);
                    if (File.Exists(backupPath))
                        File.Copy(backupPath, Path.Combine(targetDirectory, action.TargetPath), true);

                    // propagate current progress upstream
                    processedCount++;
                    int progressPercentage = (int)(processedCount / (float)Actions.Count * 100);
                    progressHandler?.Invoke(this, new ProgressChangedEventArgs(progressPercentage, rollbackStepName));
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
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private Package()
        {
            
        }

        /// <summary>
        /// Generates package content differentials between the source and target snapshots in the specified output path. Note that additional package modifications may be required afterwards to account for content not included in the snapshots.
        /// </summary>
        /// <param name="repo">The initial repository configuration.</param>
        /// <param name="sourcePath">The absolute local source content path.</param>
        /// <param name="targetPath">The absolute localtarget content path.</param>
        /// <param name="deltaPath">The delta content path; these files should be uploaded to the repo.</param>
        /// <param name="settings">The optional package settings.</param>
        /// <param name="progressHandler">The optional handler to report progress to.</param>
        /// <param name="progressStatus">The optional progress status description.</param>
        /// <returns>The created package information.</returns>
        public Package(Repository repo, Path sourcePath, Path targetPath, Path deltaPath, PackageSettings settings = null, ProgressChangedEventHandler progressHandler = null, string progressStatus = null)
        {
            Repository = repo ?? throw new ArgumentNullException(nameof(repo));

            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException("Source path does not exist.");

            if (!Directory.Exists(targetPath))
                throw new DirectoryNotFoundException("Target path does not exist.");

            if (!Directory.Exists(deltaPath))
                throw new DirectoryNotFoundException("Delta path does not exist.");

            // use the specified settings or initialize with defaults if null
            PackageSettings pkgSettings = settings ?? new PackageSettings();

            // generate the empty package and create it's content directory based on its ID
            Path packagePath = Path.Combine(deltaPath, Id + "\\");
            Directory.CreateDirectory(packagePath);

            // TODO: automatic detection of relative versus absolute paths (absolute should override and not use base path if specified)

            try
            {
                #region Snapshots

                // generate source snapshot, use existing one in repo if content matches
                var sourceSnapshot = new Snapshot(sourcePath, progressHandler, "Generating source snapshot");
                if (repo.Snapshots.Contains(sourceSnapshot))
                {
                    sourceSnapshot = repo.Snapshots[repo.Snapshots.IndexOf(sourceSnapshot)];
                }
                else repo.Snapshots.Add(sourceSnapshot);

                // generate target snapshot, use existing one in repo if content matches
                var targetSnapshot = new Snapshot(targetPath, progressHandler, "Generating target snapshot");
                if (repo.Snapshots.Contains(targetSnapshot))
                {
                    targetSnapshot = repo.Snapshots[repo.Snapshots.IndexOf(targetSnapshot)];
                }
                else repo.Snapshots.Add(targetSnapshot);

                // abort if package already exists
                if (repo.Packages.Any(pkg => Equals(pkg.SourceSnapshot, sourceSnapshot) && Equals(pkg.TargetSnapshot, targetSnapshot)))
                {
                    throw new NotSupportedException("Package already exists for the specified source and target content.");
                }

                // link snapshots to the package
                SourceSnapshot = sourceSnapshot;
                TargetSnapshot = targetSnapshot;

                #endregion

                #region Lookup Tables

                // generate lookup tables
                var sourcePaths = new Dictionary<Path, FileInformation>();
                var sourceFiles = new Dictionary<Path, List<FileInformation>>();
                var sourceHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in sourceSnapshot.Files)
                {
                    sourcePaths[info.Path] = info;

                    Path fileName = Path.GetFileName(info.Path);
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

                var targetPaths = new Dictionary<Path, FileInformation>();
                var targetFiles = new Dictionary<Path, List<FileInformation>>();
                var targetHashes = new Dictionary<string, List<FileInformation>>();
                foreach (var info in targetSnapshot.Files)
                {
                    targetPaths[info.Path] = info;

                    Path fileName = Path.GetFileName(info.Path);
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

                #region Package Content Generation

                progressHandler?.Invoke(this, new ProgressChangedEventArgs(0, progressStatus));

                // no good way to get regular progress callbacks from all patchers so just count files processed instead for now
                int processedCount = 0;

                Parallel.ForEach(targetSnapshot.Files, file =>
                {
                    Path sourceFilePath = Path.Combine(sourcePath, file.Path);
                    Path targetFilePath = Path.Combine(targetPath, file.Path);
                    bool sourcePathMatch = sourcePaths.ContainsKey(file.Path);

                    // only applicable for files
                    string hash = !file.Path.IsDirectory ? BitConverter.ToString(file.Hash) : null;
                    bool sourceHashMatch = !file.Path.IsDirectory ? sourceHashes.ContainsKey(hash) : false;
                    bool sourceHashSingleMatch = !file.Path.IsDirectory && (sourceHashMatch && sourceHashes[hash].Count == 1);
                    bool targetHashSingleMatch = !file.Path.IsDirectory ? targetHashes[hash].Count == 1 : false;

                    // unchanged empty directory
                    if (file.Path.IsDirectory & sourcePathMatch)
                    {
                     
                    }

                    // added empty directory
                    else if (file.Path.IsDirectory && !sourcePathMatch)
                    {
                        lock (((ICollection)Actions).SyncRoot)
                        {
                            Actions.Add(new AddAction(file.Path, null));
                        }
                    }

                    // unchanged file
                    else if (!file.Path.IsDirectory && sourcePathMatch && sourceHashMatch)
                    {
                        // do nothing
                    }

                    // changed files (same path, different hash)
                    else if (!file.Path.IsDirectory && sourcePathMatch && !sourceHashMatch)
                    {
                        Path tempDeltaFile = Utility.GetTempFilePath();

                        try
                        {
                            // patches should already be compressed, no need to do so again
                            PackageSettings pkgSettingsCopy = (PackageSettings)pkgSettings.Clone();   // deep clone for thread safety
                            pkgSettingsCopy.CompressionEnabled = false;

                            // generate delta patch to temp location and generate content
                            Utility.GetPatcher(pkgSettingsCopy.PatchAlgorithmType).Create(sourceFilePath, targetFilePath, tempDeltaFile);
                            var content = new Content(tempDeltaFile, packagePath, pkgSettingsCopy);
                            lock (((ICollection) Actions).SyncRoot)
                            {
                                Actions.Add(new PatchAction(file.Path, file.Path, pkgSettingsCopy.PatchAlgorithmType, content));
                            }
                        }
                        finally
                        {
                            File.Delete(tempDeltaFile);
                        }
                    }

                    // moved files (different path, single hash match on both sides)
                    else if (!file.Path.IsDirectory && !sourcePathMatch && sourceHashSingleMatch && targetHashSingleMatch)
                    {
                        lock (((ICollection) Actions).SyncRoot)
                        {
                            Actions.Add(new MoveAction(sourceHashes[hash][0].Path, file.Path));
                        }
                    }

                    // search for copied files (multiple hash matches, different paths)
                    else if (!file.Path.IsDirectory && !sourcePathMatch && sourceHashMatch)
                    {
                        lock (((ICollection) Actions).SyncRoot)
                        {
                            Actions.Add(new CopyAction(sourceHashes[hash][0].Path, file.Path));
                        }
                    }

                    // search for added files (path and hash that exists in target but not source)
                    else if (!file.Path.IsDirectory && !sourcePathMatch && !sourceHashMatch)
                    {
                        var content = new Content(targetFilePath, packagePath, pkgSettings);

                        lock (((ICollection) Actions).SyncRoot)
                        {
                            Actions.Add(new AddAction(file.Path, content));
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("File action not found.");
                    }

                    // propagate current progress upstream
                    int count = Interlocked.Increment(ref processedCount);
                    int progressPercentage = (int)(count / (float)targetSnapshot.Files.Count * 100);
                    progressHandler?.Invoke(this, new ProgressChangedEventArgs(progressPercentage, progressStatus));
                });
                
                // deleted files & directories (path that exists in source but not target)
                foreach (var file in sourceSnapshot.Files)
                {
                    if (!targetPaths.ContainsKey(file.Path))
                    {
                        Actions.Add(new RemoveAction(file.Path));
                    }
                }

                // TODO: handle empty directory deletions caused by files getting deleted within
                // loop through destination directories and delete any that don't exist in source

                // TODO: handle file/directory renames that consist of case-changes only
                // loop through all files (should be 1:1 now) and rename if case mismatches (implies underlying directories are also renamed where applicable)

                #endregion
            }
            catch (Exception ex)
            {
                Directory.Delete(packagePath, true);
            }
        }

        /// <summary>
        /// Saves the package contents to the specified local directory.
        /// </summary>
        /// <param name="baseUri">The base URI where the package content exists.</param>
        /// <param name="localDirectory">The local directory path to save the content to.</param>
        /// <param name="progressHandler">The optional handler to report progress to.</param>
        /// <param name="progressStatus">The optional progress status description.</param>
        public void Save(Path baseUri, Path localDirectory, ProgressChangedEventHandler progressHandler = null, string progressStatus = null)
        {
            int processedCount = 0;
            foreach (IAction file in Actions)
            {
                Content content;
                switch (file)
                {
                    case AddAction add:
                        content = add.Content;
                        break;
                    case PatchAction patch:
                        content = patch.Content;
                        break;
                    default:
                        continue;
                }

                foreach (var part in content.Parts)
                {
                    Path repoPath = baseUri + (Id + "/") + (content.Id + "/") + part.Path;
                    Path packagePath = localDirectory + (Id + "/") + (content.Id + "/") + part.Path;
                    new ReadOnlyFile(repoPath).Copy(packagePath);
                }

                // propagate current progress upstream
                processedCount++;
                int progressPercentage = (int)(processedCount / (float)Actions.Count * 100);
                progressHandler?.Invoke(this, new ProgressChangedEventArgs(progressPercentage, progressStatus));
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
