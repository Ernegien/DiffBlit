using System.Windows;
using System.Windows.Controls;
using DiffBlit.Core.Utilities;

namespace DiffBlitter.Controls
{
    /// <summary>
    /// Interaction logic for DirectoryPickerControl.xaml
    /// </summary>
    public partial class DirectoryPickerControl
    {
        public string Path
        {
            get => Directory.Text;
            set => Directory.Text = value;
        }
       
        public event TextChangedEventHandler PathChanged;

        public DirectoryPickerControl()
        {
            InitializeComponent();
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            Path = Utility.ShowDirectoryPicker() ?? Path;
        }

        private void DirectoryChange(object sender, TextChangedEventArgs e)
        {
            PathChanged?.Invoke(sender, e);
        }
    }
}
