using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for TimeSelectWindow.xaml
    /// </summary>
    public partial class TimeSelectWindow : Window
    {
        public bool Accepted { get; set; }
        public DateTime Start { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSelectWindow(DateTime start, TimeSpan duration)
        {
            InitializeComponent();
            Start = start;
            Duration = duration;
            Accepted = false;

            StartTextBox.Text = start.ToString("hh:mm:ss tt");
            DurationTextBox.Text = duration.ToString("hh':'mm':'ss");
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            DateTime time;
            try
            {
                time = DateTime.ParseExact(StartTextBox.Text, "hh:mm:ss tt", CultureInfo.CurrentCulture);
                Start = new DateTime(Start.Year,Start.Month,Start.Day,time.Hour,time.Minute,time.Second);
            }
            catch
            {
                MessageBox.Show("Start time is invalid. Must be in form \"hh:mm:ss tt\". Example: 06:35:45 AM");
                return;
            }

            TimeSpan duration;
            if (!TimeSpan.TryParse(DurationTextBox.Text, out duration))
            {
                MessageBox.Show("Duration is invalid. Must be in form \"hh:mm:ss\". Example: 06:35:45");
                return;
            }
            Duration = duration;

            Accepted = true;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
