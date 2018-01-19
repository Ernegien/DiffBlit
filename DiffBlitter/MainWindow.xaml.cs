using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DiffBlit.Core.Config;
using DiffBlit.Core.IO;
using DiffBlit.Core.Utilities;
using Path = DiffBlit.Core.IO.Path;

// TODO: ensure only one worker runs at a time via attempted mutex grab at beginning of each DoWork event?

namespace DiffBlitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields & Properties

        private Repository _repo;
        private Snapshot _detectedSnapshot;

        private readonly string _contentPath = string.IsNullOrWhiteSpace(Config.ContentPath)
            ? Environment.CurrentDirectory
            : Config.ContentPath;

        private readonly BackgroundWorker _patchWorker;
        private readonly BackgroundWorker _packageWorker;
        private readonly BackgroundWorker _updateWorker;
        private readonly BackgroundWorker _detectionWorker;

        // TODO: subscribe all workers to this and cancel upon app close?
        // CancellationTokenSource cancellation;

        #endregion

        #region Construction & Destruction

        public MainWindow()
        {
            InitializeComponent();

            Application.Current.DispatcherUnhandledException += UnhandledException;

            // wire up worker responsible for version detection
            _detectionWorker = Utilities.Utility.CreateBackgroundWorker(VersionDetectionWorker_DoWork, WorkerOnProgressChanged, VersionDetectionWorkerOnRunWorkerCompleted);

            // wire up worker responsible for applying patches
            _patchWorker = Utilities.Utility.CreateBackgroundWorker(PatchWorkerDoWork, WorkerOnProgressChanged, VersionDetectionWorkerOnRunWorkerCompleted);

            // wire up worker responsible for creating packages
            _packageWorker = Utilities.Utility.CreateBackgroundWorker(PackageWorker_DoWork, WorkerOnProgressChanged, PackageWorkerOnRunWorkerCompleted);

            // wire up worker responsible for updating the application
            _updateWorker = Utilities.Utility.CreateBackgroundWorker(UpdateWorker_DoWork, WorkerOnProgressChanged, UpdateWorkerOnRunWorkerCompleted);
        }

        public void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO: log error

            e.Handled = true;
        }

        #endregion

        #region Worker General

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgress(e.ProgressPercentage, e.UserState as string);
        }

        #endregion

        #region Version Detection

        private void VersionDetectionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // determine current version quickly via a targeted file check if possible
            if (!string.IsNullOrWhiteSpace(Config.VersionFilePath))
            {
                var snapshots = _repo.FindSnapshotsFromFile(_contentPath, Config.VersionFilePath);

                if (snapshots.Count == 1)
                    e.Result = snapshots.First();
            }

            // otherwise fall back to full hash verification
            e.Result = _repo.FindSnapshotFromDirectory(_contentPath, WorkerOnProgressChanged, "Detecting version");
        }

        private void VersionDetectionWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            _detectedSnapshot = runWorkerCompletedEventArgs.Result as Snapshot;
            UpdateDetectedSnapshotLabel(_detectedSnapshot);
            UpdateTargetVersionComboBox();
            WorkerOnProgressChanged(this, new ProgressChangedEventArgs(0, "Idle"));
        }

        #endregion

        #region Updates

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // TODO: implement
        }

        private void UpdateWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            // TODO: implement
            WorkerOnProgressChanged(this, new ProgressChangedEventArgs(0, "Idle"));
        }

        #endregion

        #region Package Creation

        private void PackageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // TODO: replace with custom control that prompts for version and name/description info

            string sourceDirectory = Utility.ShowDirectoryPicker("Select the source content directory");
            if (sourceDirectory == null)
                return;

            string targetDirectory = Utility.ShowDirectoryPicker("Select the target content directory");
            if (targetDirectory == null)
                return;

            string packageDirectory = Utility.ShowDirectoryPicker("Select the package output directory");
            if (packageDirectory == null)
                return;

            string repoConfigPath = Utility.ShowFilePicker("Select the repo configuration file", "Config File (*.json) | *.json");
            if (repoConfigPath == null)
                return;

            // open the repo config
            var repo = Repository.Deserialize(File.ReadAllText(repoConfigPath));

            // generate package content
            var package = new Package(repo, sourceDirectory, targetDirectory, packageDirectory, null, WorkerOnProgressChanged, "Generating package");
            package.Name = "Package Name Goes Here";

            // update the source snapshot info
            var sourceSnapshot = package.SourceSnapshot;
            sourceSnapshot.Name = "Source Snapshot Name Goes Here";
            sourceSnapshot.Version = new Version("0.0.0.0");

            // update the target snapshot info
            var targetSnapshot = package.TargetSnapshot;
            targetSnapshot.Name = "Target Snapshot Name Goes Here";
            targetSnapshot.Version = new Version("0.0.0.0");

            // bind the new package to the repo
            repo.Packages.Add(package);

            // save repo config in package output directory
            File.WriteAllText(repoConfigPath, repo.Serialize());
        }

        private void PackageWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            WorkerOnProgressChanged(this, new ProgressChangedEventArgs(0, "Idle"));
        }

        #endregion
 
        #region Patch Worker Methods

        private void PatchWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var version = e.Argument as Version;
            
            Snapshot targetSnapshot = null;

            // find matching package
            foreach (var package in _repo.Packages)
            {
                if (version == package.TargetSnapshot.Version && package.SourceSnapshot.Version == _detectedSnapshot.Version)
                {
                    targetSnapshot = package.TargetSnapshot;
                    var packageDirectory = Utility.GetTempDirectory();

                    try
                    {
                        // download package contents locally
                        package.Save(Path.GetDirectoryName(Config.RepoUri), packageDirectory, WorkerOnProgressChanged, "Downloading package");

                        // apply package
                        package.Apply(Path.Combine(packageDirectory, package.Id + "\\"), _contentPath, Config.ValidateBeforePackageApply, Config.ValidateAfterPackageApply, WorkerOnProgressChanged, "Applying package");
                    }
                    finally
                    {
                        Directory.Delete(packageDirectory, true);
                    }

                    break;
                }
            }

            e.Result = targetSnapshot;
        }

        //private void PatchWorkerOnRunPatchWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        //{
        //    _detectedSnapshot = runWorkerCompletedEventArgs.Result as Snapshot;
        //    UpdateDetectedSnapshotLabel(_detectedSnapshot);
        //    UpdateTargetVersionComboBox();
        //}

        #endregion

        #region UI Events

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as MenuItem)?.Header)
            {
                case "_Exit":
                    Application.Current.Shutdown();
                    break;
                case "_Update":
                    _updateWorker.RunWorkerAsync();
                    break;
                case "_Generate Repo Config":

                    string repoConfigDirectory = Utility.ShowDirectoryPicker("Select the output directory");
                    if (repoConfigDirectory == null)
                        return;

                    File.WriteAllText(System.IO.Path.Combine(repoConfigDirectory, "repo.json"), new Repository().Serialize());

                    break;
                case "_Create Package":
                    _packageWorker.RunWorkerAsync();
                    break;
                case "_About":
                    Process.Start("https://github.com/Ernegien/DiffBlit/tree/master/DiffBlitter");
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: proper error handling and logging

            // TODO: async all of this

            // pull down the repo config
            string json = new ReadOnlyFile(Config.RepoUri).ReadAllText();
            _repo = Repository.Deserialize(json);

            _detectionWorker.RunWorkerAsync();
        }
        private void TargetVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStatus();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Update.IsEnabled = false;
            TargetVersion.IsEnabled = false;
            _patchWorker.RunWorkerAsync(TargetVersion.SelectedValue);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _patchWorker?.Dispose();
            _packageWorker?.Dispose();
            _updateWorker?.Dispose();
        }

        #endregion

        #region UI Element Update Methods

        private void UpdateDetectedSnapshotLabel(Snapshot detected)
        {
            Dispatcher.Invoke(() =>
            {
                DetectedVersion.Content = detected?.ToString() ?? "Unknown";
            });
        }

        private void UpdateTargetVersionComboBox()
        {
            TargetVersion.Items.Clear();

            // build the target combobox based on what packages are applicable to the detected source
            TargetVersion.Items.Add("Select One");
            foreach (var p in _repo.Packages)
            {
                if (p.SourceSnapshot.Id == _detectedSnapshot.Id)
                {
                    TargetVersion.Items.Add(p.TargetSnapshot.Version);
                }
            }

            TargetVersion.SelectedIndex = 0;
            TargetVersion.IsEnabled = true;
        }

        private void UpdateButtonStatus()
        {
            var version = TargetVersion.SelectedValue as Version;
            Update.IsEnabled = version != null;

            if (version != null)
            {
                Update.Content = _detectedSnapshot.Version < version ? "Update" : "Rollback";
            }
            else Update.Content = "Update";
        }

        private void UpdateProgress(int percentage, string status)
        {
            Dispatcher.Invoke(() =>
            {
                Status.Content = status;
                Progress.Value = (percentage / 100.0f) * (Progress.Maximum - Progress.Minimum);
            });
        }

        #endregion
    }
}
