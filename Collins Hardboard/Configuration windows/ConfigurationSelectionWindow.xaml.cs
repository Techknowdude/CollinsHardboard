using System;
using System.Windows;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for ConfigurationSelectionWindow.xaml
    /// </summary>
    public partial class ConfigurationSelectionWindow : Window
    {
        public bool Accepted { get; set; }
        public Int32 Index { get { return ConfigComboBox.SelectedIndex; } }

        public ConfigurationSelectionWindow()
        {
            InitializeComponent();
            Accepted = false;
            ConfigComboBox.ItemsSource = ConfigurationsHandler.GetInstance().ConfigurationNames;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ConfigComboBox.SelectedIndex != -1)
            {
                Accepted = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a configuration.");
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
