using System;
using System.Windows;

namespace UtilityWindows
{
    /// <summary>
    /// Window for updating the user on the progress of background tasks.
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public String Message
        {
            get
            {
                return LblMessage.Content.ToString();
            }
            set { LblMessage.Content = value; }
        }

        public double PercentDone
        {
            set
            {
                LblProgress.Content = value.ToString("P1");
                StatusProgressBar.Value = value * 100;
            }

        }

        public ProgressWindow()
        {
            InitializeComponent();
            StatusProgressBar.Maximum = 100;
            StatusProgressBar.Minimum = 0;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
