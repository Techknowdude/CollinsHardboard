using System;
using System.Windows;
using StaticHelpers;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for LineSelectionWindow.xaml
    /// </summary>
    public partial class LineSelectionWindow : Window
    {
        public bool Accepted { get; set; }
        public Int32 Index
        {
            get { return LineComboBox.SelectedIndex; }
        }

        public LineSelectionWindow()
        {
            InitializeComponent();
            Accepted = false;
            LineComboBox.ItemsSource = StaticFactoryValuesManager.CoatingLines;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (LineComboBox.SelectedIndex != -1)
            {
                Accepted = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a line.");
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
