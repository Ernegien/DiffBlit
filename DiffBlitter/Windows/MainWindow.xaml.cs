using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DiffBlit.Core;
using DiffBlit.Core.IO;
using DiffBlit.Core.Logging;
using DiffBlit.Core.Utilities;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlitter.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields & Properties

        /// <summary>
        /// The current logging instance which may be null until defined by the caller.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILogger Logger => LoggerBase.CurrentInstance;

        /// <summary>
        /// Keeps track of whether a task is currently running or not.
        /// </summary>
        private int _taskRunState;

        private Repository _packageRepository;
        private Repository PackageRepository
        {
            get
            {
                if (_packageRepository == null)
                {
                    Logger?.Info("Downloading package repository configuration from {0}", Config.ContentRepoUri);
                    string json = new ReadOnlyFile(Config.ContentRepoUri).ReadAllText();
                    _packageRepository = Repository.Deserialize(json);
                }

                return _packageRepository;
            }
        }

        private Snapshot _detectedSnapshot;

        private readonly string _contentPath = string.IsNullOrWhiteSpace(Config.ContentPath)
            ? AppDomain.CurrentDomain.BaseDirectory
            : Config.ContentPath;

        private readonly BackgroundWorker _detectionWorker, _patchWorker, _snapshotWorker, _packageWorker, _updateWorker;

        // TODO: subscribe all workers to this and cancel upon app close?
        // CancellationTokenSource cancellation;

        #endregion

        #region Construction & Destruction

        public MainWindow()
        {
            // catch any unhandled exceptions (in this thread only)
            Application.Current.DispatcherUnhandledException += UnhandledException;

            // initialize the default logging instance used throughout the core library
            LoggerBase.CurrentInstance = new SeriLogger(Config.LogLevel);

            InitializeComponent();

            // update the title with the application version
            Title += " " + Assembly.GetExecutingAssembly().GetName().Version;

            // wire up worker responsible for version detection
            _detectionWorker = Utilities.Utility.CreateBackgroundWorker(VersionDetectionWorker_DoWork, WorkerOnProgressChanged, DetectionCompletedHandler);

            // wire up worker responsible for applying patches
            _patchWorker = Utilities.Utility.CreateBackgroundWorker(PatchWorkerDoWork, WorkerOnProgressChanged, PatchCompletedHandler);

            // wire up worker responsible for creating packages
            _packageWorker = Utilities.Utility.CreateBackgroundWorker(PackageWorker_DoWork, WorkerOnProgressChanged, PackageCompletedHandler);

            // wire up worker responsible for creating snapshots
            _snapshotWorker = Utilities.Utility.CreateBackgroundWorker(SnapshotWorker_DoWork, WorkerOnProgressChanged, SnapshotCompletedHandler);

            // wire up worker responsible for updating the application
            _updateWorker = Utilities.Utility.CreateBackgroundWorker(UpdateWorker_DoWork, WorkerOnProgressChanged, UpdateCompletedHandler);

            // check for updates
            _updateWorker.RunWorkerAsync();
        }

        public void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger?.Error(e.Exception, "Unhandled exception");
        }

        #endregion

        #region Workers

        private void VersionDetectionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger?.Debug("Version detection request");

            // guard against multiple tasks running at the same time
            if (!TryEnterTask())
                return;

            try
            {
                Logger?.Info("Version detection started");

                _detectedSnapshot = null;

                // avoids hashing your entire computer because you decided to run this at the root of your C drive and not configure the ContentPath in the app.config
                string requiredFilePath = Path.Combine(_contentPath + "\\", Config.FileMustExist);
                if (!string.IsNullOrWhiteSpace(Config.FileMustExist) &&
                    !File.Exists(requiredFilePath))
                {
                    Logger?.Warn("{0} not found", requiredFilePath);
                    return;
                }

                // determine current version quickly via a targeted file check if possible
                if (!string.IsNullOrWhiteSpace(Config.VersionFilePath))
                {
                    var snapshots = PackageRepository.FindSnapshotsFromFile(_contentPath, Config.VersionFilePath);

                    if (snapshots?.Count == 1)
                        _detectedSnapshot = snapshots.First();
                }

                // otherwise fall back to full hash verification
                if (_detectedSnapshot == null)
                {
                    Logger?.Info("Falling back to full hash verification");
                    _detectedSnapshot = PackageRepository.FindSnapshotFromDirectory(_contentPath, WorkerOnProgressChanged, "Detecting version");
                }

                Logger?.Info("Version detection completed");
            }
            finally
            {
                // allow other tasks to run
                ExitTask();
            }
        }

        private void DetectionCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateDetectedSnapshot(_detectedSnapshot);
            TargetVersion.IsEnabled = true;
            TaskCompleted(e);
        }

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger?.Debug("Update request");

            // guard against multiple tasks running at the same time
            if (!TryEnterTask())
                return;

            try
            {
                // attempt to open the updater repo config
                _updateWorker.ReportProgress(0, "Checking for updates");
                Logger?.Info("Downloading updater repository configuration from {0}", Config.UpdaterRepoUri);
                var repo = Repository.Deserialize(new ReadOnlyFile(Config.UpdaterRepoUri).ReadAllText());

                // check for the newest version available
                var updateSnapshot = repo.Snapshots.OrderByDescending(t => t.Version).FirstOrDefault();
                if (updateSnapshot == null)
                {
                    Logger?.Info("No updates available");
                    return;
                }

                // get the current version
                var binPath = Assembly.GetExecutingAssembly().Location;
                var binDir = System.IO.Path.GetDirectoryName(binPath) + "\\";
                var currentVersion = new Version(FileVersionInfo.GetVersionInfo(binPath).FileVersion);

                // abort if no updates available
                if (updateSnapshot.Version <= currentVersion)
                {
                    Logger?.Info("Already updated");
                    return;
                }

                if (!Config.UpdaterAutoUpdate)
                {
                    // give user the choice to accept or decline
                    MessageBoxResult choice = MessageBox.Show(
                        $"Update {updateSnapshot.Version} is available. Would you like to apply it?",
                        "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    // abort if declined
                    if (choice != MessageBoxResult.Yes)
                    {
                        Logger?.Debug("Update declined");
                        return;
                    }
                }

                const string backupExtension = ".backup";
                string remoteFileDirectory = Path.Combine(Path.GetDirectoryName(Config.UpdaterRepoUri) + "/", updateSnapshot.Version.ToString());

                try
                {
                    // apply updates
                    foreach (var file in updateSnapshot.Files)
                    {
                        Logger?.Info("Updating {0}", file);

                        // build local and remote file paths
                        string remoteFilePath = Path.Combine(remoteFileDirectory + "/", file.Path);
                        string localFilePath = Path.Combine(binDir, file.Path);

                        // rename local file with backup extension if it exists, deleting any that already exist
                        if (File.Exists(localFilePath))
                        {
                            string backupPath = localFilePath + backupExtension;
                            File.Delete(backupPath);
                            File.Move(localFilePath, backupPath);
                        }

                        // download remote file
                        new ReadOnlyFile(remoteFilePath).Copy(localFilePath);

                        // TODO: validate downloaded content

                        Logger?.Info("Finished updating {0}", file);
                    }

                    Logger?.Info("Update completed, restarting application");

                    // start a new instance of the updated application and exit the existing
                    // TODO: start cmd with timer to delete backup files
                    Process.Start(Assembly.GetExecutingAssembly().Location);
                    Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                }
                catch
                {
                    foreach (var file in Directory.GetFiles(binDir, "*" + backupExtension, SearchOption.AllDirectories))
                    {
                        string originalPath = file.Substring(0, file.Length - backupExtension.Length);
                        File.Delete(originalPath);
                        File.Move(file, originalPath);
                    }

                    throw;
                }
            }
            finally
            {
                // allow other tasks to run
                ExitTask();
            }
        }

        private void UpdateCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            TaskCompleted(e, true);

            // run version detection immediately after update check on startup
            _detectionWorker.RunWorkerAsync();

            if (e.Error != null)
            {
                Logger?.Error(e.Error, "Update fialed");
                MessageBox.Show("Update failed. See log for additional information.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SnapshotWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger?.Debug("Snapshot creation request");

            // guard against multiple tasks running at the same time
            if (!TryEnterTask())
                return;

            try
            {
                Logger?.Info("Snapshot creation started");

                // get snapshot information
                bool snapshotSettingsValid = false;
                string snapshotName = null, snapshotDescription = null, snapshotDirectory = null, repoConfigPath = null;
                Version snapshotVersion = null;
                Dispatcher.Invoke(() =>
                {
                    SnapshotSettingsWindow snapshotSettings = new SnapshotSettingsWindow();
                    snapshotSettings.ShowDialog();
                    snapshotSettingsValid = snapshotSettings.DialogResult.GetValueOrDefault();
                    snapshotName = snapshotSettings.SnapshotName.Text;
                    Version.TryParse(snapshotSettings.SnapshotVersion.Text, out snapshotVersion);
                    snapshotDescription = snapshotSettings.SnapshotDescription.Text;
                    snapshotDirectory = snapshotSettings.SnapshotDirectory.Path;
                    repoConfigPath = snapshotSettings.ConfigPath.Path;
                });

                // only continue if the information specified is valid
                if (!snapshotSettingsValid)
                {
                    Logger?.Info("Snapshot creation canceled");
                    return;
                }

                // create the snapshot
                Snapshot snap = new Snapshot(snapshotDirectory, WorkerOnProgressChanged)
                {
                    Name = snapshotName,
                    Description = snapshotDescription,
                    Version = snapshotVersion
                };

                // add it to the repo if it doesn't already exist
                // TODO: what if you just want to update an existing snapshot's name/description etc.?
                Repository repo = Repository.Deserialize(File.ReadAllText(repoConfigPath));
                if (!repo.Snapshots.Contains(snap))
                {
                    repo.Snapshots.Add(snap);
                    File.WriteAllText(repoConfigPath, repo.Serialize());
                }

                Logger?.Info("Snapshot creation completed");
            }
            finally
            {
                // allow other tasks to run
                ExitTask();
            }
        }

        private void SnapshotCompletedHandler(object s, RunWorkerCompletedEventArgs e)
        {
            TaskCompleted(e);
        }

        private void PackageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger?.Debug("Package creation request");

            // guard against multiple tasks running at the same time
            if (!TryEnterTask())
                return;

            try
            {
                Logger?.Info("Package creation started");

                _packageWorker.ReportProgress(0, "Obtaining package information");

                // TODO: get package settings version, name, description etc.
                // get all of the information necessary to create the package
                bool packageSettingsValid = false;
                string sourceName = null, sourceVersion = null, sourceDescription = null, sourceDirectory = null;
                string targetName = null, targetVersion = null, targetDescription = null, targetDirectory = null;
                string repoPath = null, packagePath = null;

                Dispatcher.Invoke(() =>
                {
                    PackageSettingsWindow packageSettings = new PackageSettingsWindow();
                    packageSettings.ShowDialog();
                    packageSettingsValid = packageSettings.DialogResult.GetValueOrDefault();
                    sourceName = packageSettings.SourceName.Text;
                    sourceVersion = packageSettings.SourceVersion.Text;
                    sourceDescription = packageSettings.SourceDescription.Text;
                    sourceDirectory = packageSettings.SourceDirectory.Path;
                    targetName = packageSettings.TargetName.Text;
                    targetVersion = packageSettings.TargetVersion.Text;
                    targetDescription = packageSettings.TargetDescription.Text;
                    targetDirectory = packageSettings.TargetDirectory.Path;
                    repoPath = packageSettings.ConfigPath.Path;
                    packagePath = packageSettings.OutputDirectory.Path;
                });

                // only continue if the information specified is valid
                if (!packageSettingsValid)
                {
                    Logger?.Info("Package creation canceled");
                    // TODO: display error messagebox
                    return;
                }

                // open the repo config
                var repo = Repository.Deserialize(File.ReadAllText(repoPath));

                // generate package content
                Logger?.Info("Generating package content");
                var package = new Package(repo, sourceDirectory + "\\", targetDirectory + "\\", packagePath + "\\", null, WorkerOnProgressChanged, "Generating package");
                package.Name = "Package Name Goes Here"; // TODO: package name and description

                // update the source snapshot info
                var sourceSnapshot = package.SourceSnapshot;
                sourceSnapshot.Name = sourceName;
                sourceSnapshot.Version = new Version(sourceVersion);
                sourceSnapshot.Description = sourceDescription;

                // update the target snapshot info
                var targetSnapshot = package.TargetSnapshot;
                targetSnapshot.Name = targetName;
                targetSnapshot.Version = new Version(targetVersion);
                targetSnapshot.Description = targetDescription;

                // bind the new package to the repo
                repo.Packages.Add(package);

                // save repo config in package output directory
                File.WriteAllText(repoPath, repo.Serialize());

                Logger?.Info("Package creation completed");
            }
            finally
            {
                // allow other tasks to run
                ExitTask();
            }
        }

        private void PackageCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            TaskCompleted(e);
        }

        private void PatchWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Logger?.Debug("Patch request");

            // guard against multiple tasks running at the same time
            if (!TryEnterTask())
                return;

            try
            {
                Logger?.Info("Patch started");

                // disable UI
                Dispatcher.Invoke(() =>
                {
                    Patch.IsEnabled = false;
                    TargetVersion.IsEnabled = false;
                });

                var version = e.Argument as Version;

                Package package = PackageRepository.FindPackage(_detectedSnapshot.Version, version);
                Logger?.Info("Package found for source version {0} to target version {1}",
                    package.SourceSnapshot.Version, package.TargetSnapshot.Version);

                var packageDirectory = Utility.GetTempDirectory();

                try
                {
                    // download package contents locally
                    Logger?.Info("Attempting to download package contents from {0}", Config.ContentRepoUri);
                    package.Save(Path.GetDirectoryName(Config.ContentRepoUri), packageDirectory,
                        WorkerOnProgressChanged, "Downloading package");

                    // apply package
                    Logger?.Info("Applying package");
                    package.Apply(Path.Combine(packageDirectory, package.Id + "\\"), _contentPath,
                        Config.ValidateBeforePackageApply, Config.ValidateAfterPackageApply, WorkerOnProgressChanged,
                        "Applying package");
                }
                finally
                {
                    Directory.Delete(packageDirectory, true);
                }

                // update detected version
                _detectedSnapshot = package.TargetSnapshot;

                // update the UI
                Dispatcher.Invoke(() =>
                {
                    UpdateDetectedSnapshot(_detectedSnapshot);
                });

                Logger?.Info("Patch completed");
            }
            finally
            {
                // allow other tasks to run
                ExitTask();
            }
        }

        private void PatchCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            TargetVersion.IsEnabled = true;
            TaskCompleted(e);
        }

        #endregion

        #region UI Methods

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as MenuItem)?.Header)
            {
                case "_Exit":
                    Logger?.Info("Application shutdown requested");
                    Application.Current.Shutdown();
                    break;
                // TODO: editable app.config via SettingsWindow
                //case "_Settings":
                //    throw new NotImplementedException();
                //    break;
                case "_Create Repository Configuration":

                    Logger?.Info("Generating new repository configuration");
                    string repoConfigDirectory = Utility.ShowDirectoryPicker("Select the output directory");
                    if (repoConfigDirectory == null)
                    {
                        Logger?.Info("Repo configuration generation cancelled");
                        return;
                    }
                 
                    File.WriteAllText(System.IO.Path.Combine(repoConfigDirectory, "repo.json"), new Repository().Serialize());

                    break;
                case "_Create Package":
                    _packageWorker.RunWorkerAsync();
                    break;
                case "_Create Snapshot":
                    _snapshotWorker.RunWorkerAsync();
                    break;
                case "_About":
                    Logger?.Info("About clicked");
                    Process.Start("https://github.com/Ernegien/DiffBlit/tree/master/DiffBlitter");
                    break;
            }
        }

        private void TargetVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_detectedSnapshot == null)
                throw new NullReferenceException("Unknown source content version.");

            // update target information
            var targetSnapshot = TargetVersion.SelectedValue as Snapshot;
            TargetName.Text = targetSnapshot?.Name;
            TargetDescription.Text = targetSnapshot?.Description;

            Patch.IsEnabled = targetSnapshot != null;
            Patch.Content = targetSnapshot == null || _detectedSnapshot.Version < targetSnapshot.Version ? "Update" : "Rollback";
        }

        private void Patch_Click(object sender, RoutedEventArgs e)
        {
            _patchWorker.RunWorkerAsync((TargetVersion.SelectedValue as Snapshot)?.Version);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Logger?.Info("Application closing");
            _detectionWorker?.Dispose();
            _patchWorker?.Dispose();
            _packageWorker?.Dispose();
            _updateWorker?.Dispose();
            _snapshotWorker?.Dispose();
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Status.Content = e.UserState as string;
                Progress.Value = (e.ProgressPercentage / 100.0f) * (Progress.Maximum - Progress.Minimum);
            });
        }

        private void UpdateDetectedSnapshot(Snapshot snapshot)
        {
            SourceVersion.Text = snapshot?.Version.ToString() ?? "Unknown";
            SourceName.Text = snapshot?.Name ?? "Unknown";
            SourceDescription.Text = snapshot?.Description ?? "Unknown";

            // build the target combobox based on what packages are applicable to the detected source
            TargetVersion.ItemsSource = PackageRepository.Packages.Where(s => s.SourceSnapshot.Id == snapshot?.Id).OrderByDescending(s => s.TargetSnapshot.Version);
            TargetVersion.DisplayMemberPath = "TargetSnapshot.Version";
            TargetVersion.SelectedValuePath = "TargetSnapshot";
            TargetVersion.SelectedIndex = 0;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Returns true if given permission to run the requested task.
        /// </summary>
        /// <returns></returns>
        private bool TryEnterTask()
        {
            // lock attempt which returns success
            bool hasPermission = Interlocked.CompareExchange(ref _taskRunState, 1, 0) == 0;

            if (!hasPermission)
            {
                Logger?.Debug("Another task is already running");
                MessageBox.Show("Please wait for the current operation to finish first.",
                    "Hold up!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
 
            return hasPermission;
        }

        /// <summary>
        /// Clears the task run state so another task may be entered.
        /// </summary>
        private void ExitTask()
        {
            // NOTE: don't care if this isn't atomic and a task was denied an opportunity to run because the current was on the edge of finishing
            _taskRunState = 0;
        }

        private void TaskCompleted(RunWorkerCompletedEventArgs e, bool swallowException = false)
        {
            // report idle status
            Status.Content = "Idle";
            Progress.Value = 0;

            // propagate any exception if desired
            if (!swallowException && e.Error != null)
                throw e.Error;
        }

        #endregion
    }
}
