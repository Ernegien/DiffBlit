using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DiffBlitter.Windows
{
    /// <summary>
    /// Interaction logic for PackageSettingsWindow.xaml
    /// </summary>
    public partial class PackageSettingsWindow
    {
        public PackageSettingsWindow()
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

            if (string.IsNullOrWhiteSpace(SourceName.Text))
                ValidationText.Text += "Source content name must be specified" + Environment.NewLine;

            if (!Version.TryParse(SourceVersion.Text, out Version _))
                ValidationText.Text += "Source version format is invalid" + Environment.NewLine;

            if (!Directory.Exists(SourceDirectory.Path))
                ValidationText.Text += "Source directory does not exist" + Environment.NewLine;

            if (string.IsNullOrWhiteSpace(TargetName.Text))
                ValidationText.Text += "Target content name must be specified" + Environment.NewLine;

            if (!Version.TryParse(TargetVersion.Text, out Version _))
                ValidationText.Text += "Target version format is invalid" + Environment.NewLine;

            if (!Directory.Exists(TargetDirectory.Path))
                ValidationText.Text += "Target directory does not exist" + Environment.NewLine;

            if (!File.Exists(ConfigPath.Path))
                ValidationText.Text += "Repository configuration file does not exist" + Environment.NewLine;

            if (!Directory.Exists(OutputDirectory.Path))
                ValidationText.Text += "Output directory does not exist" + Environment.NewLine;

            bool success = ValidationText.Text.Length == 0;
            ValidationText.Visibility = success ? Visibility.Collapsed : Visibility.Visible;
            OkButton.IsEnabled = success;
        }
    }
}
