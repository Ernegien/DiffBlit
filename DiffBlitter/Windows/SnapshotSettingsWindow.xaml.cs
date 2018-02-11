using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DiffBlitter.Windows
{
    /// <summary>
    /// Interaction logic for SnapshotSettingsWindow.xaml
    /// </summary>
    public partial class SnapshotSettingsWindow
    {
        public SnapshotSettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Validate();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private void Validate()
        {
            if (!IsLoaded)
                return;

            ValidationText.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(SnapshotName.Text))
                ValidationText.Text += "Snapshot content name must be specified" + Environment.NewLine;

            if (!Version.TryParse(SnapshotVersion.Text, out Version _))
                ValidationText.Text += "Snapshot version format is invalid" + Environment.NewLine;

            if (!Directory.Exists(SnapshotDirectory.Path))
                ValidationText.Text += "Snapshot directory does not exist" + Environment.NewLine;

            if (!File.Exists(ConfigPath.Path))
                ValidationText.Text += "Repository configuration file does not exist" + Environment.NewLine;

            bool success = ValidationText.Text.Length == 0;
            ValidationText.Visibility = success ? Visibility.Collapsed : Visibility.Visible;
            OkButton.IsEnabled = success;
        }
    }
}
