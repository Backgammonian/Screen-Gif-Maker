using System.Windows;

namespace GifMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as MainViewModel).Close();
        }
    }
}
