using System.Windows;
using System.Windows.Threading;

namespace DiffBlitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += UnhandledException;
            InitializeComponent();
        }

        public void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {

            // TODO: log error
        }
    }
}
