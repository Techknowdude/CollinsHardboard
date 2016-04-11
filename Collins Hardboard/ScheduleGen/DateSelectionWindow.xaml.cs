using System;
using System.Windows;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for DateSelectionWindow.xaml
    /// </summary>
    public partial class DateSelectionWindow : Window
    {
        public DateTime Start { get { return (DateTime) StartDatePicker.SelectedDate; } }
        public DateTime End { get { return (DateTime) EndDatePicker.SelectedDate; } }
        public DateTime Sales { get { return (DateTime) SalesDatePicker.SelectedDate; } }
        public bool Accepted { get; set; }
        public bool? UseSalesDueDate { get { return CbxScheduleByDueDate.IsChecked; } }

        public DateSelectionWindow()
        {
            InitializeComponent();
            Accepted = false;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate != null && EndDatePicker.SelectedDate != null && SalesDatePicker.SelectedDate != null)
            {
                Accepted = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a starting, ending date, and sales period.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
