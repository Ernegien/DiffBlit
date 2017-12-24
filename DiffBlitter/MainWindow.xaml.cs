using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DiffBlit.Core.Config;
using DiffBlit.Core.IO;
using DiffBlit.Core.Utilities;

namespace DiffBlitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Repository _repo;
        private Snapshot _detectedSnapshot;
        private string _contentPath = string.IsNullOrWhiteSpace(Config.ContentPath) ? Environment.CurrentDirectory : Config.ContentPath;

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += UnhandledException;
            InitializeComponent();
        }

        public void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {

            // TODO: log error
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as MenuItem)?.Header)
            {
                case "_Exit":
                    Application.Current.Shutdown();
                    break;
                case "_Generate Repo Config":
                    GenerateRepoConfig();
                    break;
                case "_Create Package":
                    CreatePackage();
                    break;
                case "_About":
                    Process.Start("https://github.com/Ernegien/DiffBlit/tree/master/DiffBlitter");
                    break;
            }
        }

        private void GenerateRepoConfig()
        {
            string repoConfigDirectory = Utility.ShowDirectoryPicker("Select the output directory");
            if (repoConfigDirectory == null)
                return;

            File.WriteAllText(System.IO.Path.Combine(repoConfigDirectory, "repo.json"), new Repository().Serialize());
        }

        private void CreatePackage()
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
            var package = new Package(repo, sourceDirectory, targetDirectory, packageDirectory);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: proper error handling and logging

            return;

            // pull down the repo config
            string json = new ReadOnlyFile(Config.RepoUri).ReadAllText();
            _repo = Repository.Deserialize(json);

            // determine current version quickly via a targeted file check if possible

            if (!string.IsNullOrWhiteSpace(Config.VersionFilePath))
            {
                var snapshots = _repo.FindSnapshotsFromFile(_contentPath, Config.VersionFilePath);

                if (snapshots.Count == 1)
                    _detectedSnapshot = snapshots.First();
            }

            // otherwise fall back to full hash verification
            if (_detectedSnapshot == null)
            {
                _detectedSnapshot = _repo.FindSnapshotFromDirectory(_contentPath);
            }

            // update status
            DetectedVersion.Content = _detectedSnapshot?.ToString() ?? "Unknown";
            if (_detectedSnapshot == null)
            {
                Status.Content = "Unable to detect content version.";
                return;
            }
 
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

        private void TargetVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTargetVersionsComboBox();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            var version = TargetVersion.SelectedValue as Version;

            // TODO: find matching package
            foreach (var package in _repo.Packages)
            {
                if (version == package.TargetSnapshot.Version && package.SourceSnapshot.Version == _detectedSnapshot.Version)
                {
                    // download package contents locally
                    var packageDirectory = Utility.GetTempDirectory();
                    package.Save(packageDirectory); // TODO: specify base path

                    // apply package
                    package.Apply(packageDirectory, _contentPath);
                }
            }

            UpdateTargetVersionsComboBox();
        }

        private void UpdateTargetVersionsComboBox()
        {
            var version = TargetVersion.SelectedValue as Version;
            Update.IsEnabled = version != null;

            if (version != null)
            {
                Update.Content = _detectedSnapshot.Version < version ? "Update" : "Rollback";
            }
            else Update.Content = "Update";
        }
    }
}
