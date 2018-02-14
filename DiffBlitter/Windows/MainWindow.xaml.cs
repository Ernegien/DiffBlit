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
using DiffBlit.Core.Config;
using DiffBlit.Core.IO;
using DiffBlit.Core.Logging;
using DiffBlit.Core.Utilities;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlitter.Windows
{
    // TODO: fallback repo URIs
    // TODO: better error-handling - failures can cause invalid ui states

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields & Properties

        private readonly ILogger _logger = new Logger(Config.LogLevel);

        // only allow one task to be ran at a time
        private readonly SemaphoreSlim _taskSync = new SemaphoreSlim(1, 1);

        private Repository _packageRepository;
        private Repository PackageRepository
        {
            get
            {
                if (_packageRepository == null)
                {
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

        private readonly BackgroundWorker _detectionWorker;
        private readonly BackgroundWorker _patchWorker;
        private readonly BackgroundWorker _snapshotWorker;
        private readonly BackgroundWorker _packageWorker;
        private readonly BackgroundWorker _updateWorker;
 
        // TODO: subscribe all workers to this and cancel upon app close?
        // CancellationTokenSource cancellation;

        #endregion

        #region Construction & Destruction

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += UnhandledException;

            InitializeComponent();

            // wire up worker responsible for version detection
            _detectionWorker = Utilities.Utility.CreateBackgroundWorker(VersionDetectionWorker_DoWork, WorkerOnProgressChanged);

            // wire up worker responsible for applying patches
            _patchWorker = Utilities.Utility.CreateBackgroundWorker(PatchWorkerDoWork, WorkerOnProgressChanged);

            // wire up worker responsible for creating packages
            _packageWorker = Utilities.Utility.CreateBackgroundWorker(PackageWorker_DoWork, WorkerOnProgressChanged);

            // wire up worker responsible for creating snapshots
            _snapshotWorker = Utilities.Utility.CreateBackgroundWorker(SnapshotWorker_DoWork, WorkerOnProgressChanged, SnapshotCompletedHandler);

            // wire up worker responsible for updating the application
            _updateWorker = Utilities.Utility.CreateBackgroundWorker(UpdateWorker_DoWork, WorkerOnProgressChanged);

            _updateWorker.RunWorkerAsync();
        }

        private void SnapshotCompletedHandler(object s, RunWorkerCompletedEventArgs e)
        {
            var snap = e.Result as Snapshot;

        }

        public void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Error(e.Exception, "Unhandled exception");

            // TODO: log error

            e.Handled = true;
        }

        #endregion

        #region Workers

        private void VersionDetectionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _logger.Info("Version detection request");

            // guard against multiple tasks running at the same time
            if (!_taskSync.Wait(0))
            {
                ShowSingleTaskOnlyWarning();
                return;
            }

            _logger.Info("Version detection started");

            try
            {
                _detectedSnapshot = null;

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
                    _logger.Info("Falling back to full hash verification");
                    _detectedSnapshot = PackageRepository.FindSnapshotFromDirectory(_contentPath, WorkerOnProgressChanged, "Detecting version");
                }

                // update the UI
                Dispatcher.Invoke(() =>
                {
                    UpdateDetectedSnapshot(_detectedSnapshot);
                });

                _logger.Info("Version detection completed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled Exception");
                throw;
            }
            finally
            {
                _detectionWorker.ReportProgress(0, "Idle");
                _taskSync.Release();

                // enable UI
                Dispatcher.Invoke(() =>
                {
                    TargetVersion.IsEnabled = true;
                });
            }
        }

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _logger.Info("Update request");

            // guard against multiple tasks running at the same time
            if (!_taskSync.Wait(0))
            {
                ShowSingleTaskOnlyWarning();
                return;
            }

            _logger.Info("Update started");
            _updateWorker.ReportProgress(0, "Checking for updates");

            try
            {
                // open the updater repo config
                _logger.Info("Checking for updates at {0}", Config.UpdaterRepoUri);
                var repo = Repository.Deserialize(new ReadOnlyFile(Config.UpdaterRepoUri).ReadAllText());

                // check for newest version available
                var newestSnapshot = repo.Snapshots.OrderByDescending(t => t.Version).FirstOrDefault();

                // get current version
                var binPath = Assembly.GetExecutingAssembly().Location;
                var binDir = System.IO.Path.GetDirectoryName(binPath) + "\\";
                var currentVersion = new Version(FileVersionInfo.GetVersionInfo(binPath).FileVersion);

                // check if an update is available
                if (newestSnapshot?.Version > currentVersion)
                {
                    // give user the choice to accept or decline
                    MessageBoxResult choice = MessageBox.Show("An update is available. Would you like to apply it?", "Update Available",
                        MessageBoxButton.YesNo, MessageBoxImage.Information);

                    // abort if declined
                    if (choice != MessageBoxResult.Yes)
                    {
                        _logger.Info("Update declined");
                        return;
                    }

                    const string backupExtension = ".diffblitterbackup";
                    try
                    {
                        foreach (var file in newestSnapshot.Files)
                        {
                            string remoteFileDirectory = Path.Combine(Path.GetDirectoryName(Config.UpdaterRepoUri) + "/", newestSnapshot.Version.ToString());
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
                        }
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

                    _logger.Info("Update completed");

                    // restart the application
                    _logger.Info("Restarting application");
                    Process.Start(binPath);
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                }
                else
                {
                    _logger.Info("Update not available");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled Exception");
                throw;
            }
            finally
            {
                _updateWorker.ReportProgress(0, "Idle");
                _taskSync.Release();

                // run detection immediately after update check on startup
                _detectionWorker.RunWorkerAsync();
            }
        }

        private void SnapshotWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _logger.Info("Snapshot creation request");

            // guard against multiple tasks running at the same time
            if (!_taskSync.Wait(0))
            {
                ShowSingleTaskOnlyWarning();
                return;
            }

            try
            {
                _logger.Info("Snapshot creation started");

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
                    _logger.Info("Snapshot creation canceled");
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

                _logger.Info("Snapshot creation completed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled Exception");
                throw;
            }
            finally
            {
                _snapshotWorker.ReportProgress(0, "Idle");
                _taskSync.Release();
            }
        }

        private void PackageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _logger.Info("Package creation request");

            // guard against multiple tasks running at the same time
            if (!_taskSync.Wait(0))
            {
                ShowSingleTaskOnlyWarning();
                return;
            }

            _logger.Info("Package creation started");

            try
            {
                _packageWorker.ReportProgress(0, "Obtaining package information");

                // TODO: get package settings
                // get all of the information necessary to create the package
                bool packageSettingsValid = false;
                string sourceDirectory = null, targetDirectory = null, packageDirectory = null, repoConfigPath = null, sourceSnapshotName = null, targetSnapshotName = null;
                Dispatcher.Invoke(() =>
                {
                    PackageSettingsWindow packageSettings = new PackageSettingsWindow();
                    packageSettings.ShowDialog();

                    packageSettingsValid = packageSettings.DialogResult.GetValueOrDefault();
                    sourceDirectory = packageSettings.SourceDirectory.Path;
                    targetDirectory = packageSettings.TargetDirectory.Path;
                    packageDirectory = packageSettings.OutputDirectory.Path;
                    repoConfigPath = packageSettings.ConfigPath.Path;
                    sourceSnapshotName = packageSettings.SourceName.Text;
                    targetSnapshotName = packageSettings.TargetName.Text;
                });

                // only continue if the information specified is valid
                if (!packageSettingsValid)
                {
                    _logger.Info("Package creation canceled");
                    return;
                }
                    
                // open the repo config
                var repo = Repository.Deserialize(File.ReadAllText(repoConfigPath));

                // generate package content
                _logger.Info("Generating package content");
                var package = new Package(repo, sourceDirectory, targetDirectory, packageDirectory, null, WorkerOnProgressChanged, "Generating package");
                package.Name = "Package Name Goes Here";    // TODO: need this in package settings
                // TODO: package description?

                // update the source snapshot info
                var sourceSnapshot = package.SourceSnapshot;
                sourceSnapshot.Name = sourceSnapshotName;
                sourceSnapshot.Version = new Version("0.0.0.0");

                // update the target snapshot info
                var targetSnapshot = package.TargetSnapshot;
                targetSnapshot.Name = targetSnapshotName;
                targetSnapshot.Version = new Version("0.0.0.0");

                // bind the new package to the repo
                repo.Packages.Add(package);

                // save repo config in package output directory
                File.WriteAllText(repoConfigPath, repo.Serialize());

                _logger.Info("Package creation completed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled Exception");
                throw;
            }
            finally
            {
                _packageWorker.ReportProgress(0, "Idle");
                _taskSync.Release();
            }
        }

        private void PatchWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            _logger.Info("Patch request");

            // guard against multiple tasks running at the same time
            if (!_taskSync.Wait(0))
            {
                ShowSingleTaskOnlyWarning();
                return;
            }

            _logger.Info("Patch started");

            try
            {
                // disable UI
                Dispatcher.Invoke(() =>
                {
                    Patch.IsEnabled = false;
                    TargetVersion.IsEnabled = false;
                });

                var version = e.Argument as Version;

                Package package = PackageRepository.FindPackage(_detectedSnapshot.Version, version);
                _logger.Info("Package found for source version {0} to target version {1}",
                    package.SourceSnapshot.Version, package.TargetSnapshot.Version);

                var packageDirectory = Utility.GetTempDirectory();

                try
                {
                    // download package contents locally
                    _logger.Info("Attempting to download package contents from {0}", Config.ContentRepoUri);
                    package.Save(Path.GetDirectoryName(Config.ContentRepoUri), packageDirectory,
                        WorkerOnProgressChanged, "Downloading package");

                    // apply package
                    _logger.Info("Applying package");
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

                _logger.Info("Patch completed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled Exception");
                throw;
            }
            finally
            {
                _patchWorker.ReportProgress(0, "Idle");
                _taskSync.Release();

                // enable UI
                Dispatcher.Invoke(() =>
                {
                    TargetVersion.IsEnabled = true;
                });
            }
        }

        #endregion

        #region UI Methods

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as MenuItem)?.Header)
            {
                case "_Exit":
                    _logger.Info("Application shutdown requested");
                    Application.Current.Shutdown();
                    break;
                case "_Settings":
                    // TODO: editable app.config via SettingsWindow
                    throw new NotImplementedException();
                    break;
                //case "_Update":
                //    _updateWorker.RunWorkerAsync();
                //    break;
                case "_Create Repository Configuration":

                    _logger.Info("Generating new repository configuration");
                    string repoConfigDirectory = Utility.ShowDirectoryPicker("Select the output directory");
                    if (repoConfigDirectory == null)
                    {
                        _logger.Info("Repo configuration generation cancelled");
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
                    _logger.Info("About clicked");
                    Process.Start("https://github.com/Ernegien/DiffBlit/tree/master/DiffBlitter");
                    break;
            }
        }

        private void TargetVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_detectedSnapshot == null)
                throw new NullReferenceException("Unknown source content version.");

            var version = TargetVersion.SelectedValue as Version;
            Patch.IsEnabled = version != null;
            Patch.Content = version == null || _detectedSnapshot.Version < version ? "Update" : "Rollback";
        }

        private void Patch_Click(object sender, RoutedEventArgs e)
        {
            _patchWorker.RunWorkerAsync(TargetVersion.SelectedValue);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _logger.Info("Application closing");
            _detectionWorker?.Dispose();
            _patchWorker?.Dispose();
            _packageWorker?.Dispose();
            _updateWorker?.Dispose();
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Status.Content = e.UserState as string;
                Progress.Value = (e.ProgressPercentage / 100.0f) * (Progress.Maximum - Progress.Minimum);
            });
        }

        private void ShowSingleTaskOnlyWarning()
        {
            _logger.Info("Another task is already running");
            MessageBox.Show("Please wait for the current operation to finish first.",
                "Hold up!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateDetectedSnapshot(Snapshot snapshot)
        {
            DetectedVersion.Content = snapshot?.ToString() ?? "Unknown";

            // build the target combobox based on what packages are applicable to the detected source
            TargetVersion.Items.Clear();
            TargetVersion.Items.Add("Select One");

            foreach (var p in PackageRepository.Packages)
            {
                if (p.SourceSnapshot.Id == snapshot?.Id)
                {
                    TargetVersion.Items.Add(p.TargetSnapshot.Version);
                }
            }

            TargetVersion.SelectedIndex = 0;
        }
        #endregion
    }
}
