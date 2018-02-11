using System.Windows;
using System.Windows.Controls;
using DiffBlit.Core.Utilities;

namespace DiffBlitter.Controls
{
    /// <summary>
    /// Interaction logic for FilePickerControl.xaml
    /// </summary>
    public partial class FilePickerControl
    {
        public string Path
        {
            get => File.Text;
            set => File.Text = value;
        }
       
        public event TextChangedEventHandler PathChanged;

        public FilePickerControl()
        {
            InitializeComponent();
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            Path = Utility.ShowFilePicker() ?? Path;
        }

        private void PathChange(object sender, TextChangedEventArgs e)
        {
            PathChanged?.Invoke(sender, e);
        }
    }
}
