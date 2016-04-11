using System.Windows;
using ImportLib;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ShiftWindow window = new ShiftWindow(true);
            window.Show();
            window.Focus();
        }

        private void ImportButton_OnClick(object sender, RoutedEventArgs e)
        {
            InternalImport.GetInstance().ImportMaster();
        }
    }
}
