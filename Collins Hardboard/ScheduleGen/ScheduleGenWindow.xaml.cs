using System;
using System.Linq;
using System.Windows;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for ScheduleGenWindow.xaml
    /// </summary>
    public partial class ScheduleGenWindow : Window
    {
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        public DateTime SaleDate { get; set; } = DateTime.Today.AddDays(14);
        public ScheduleGenerator ScheduleGen { get { return ScheduleGenerator.Instance; } }

        public ScheduleGenWindow()
        {
            InitializeComponent();

            ScheduleGenerator.Instance.Window = this;
        }

        public void Remove(GenControl control)
        {
            ScheduleGenerator.Instance.ControlsList.Remove(control);
        }

        public void UpdateControlOrder()
        {
            ScheduleGenerator.Instance.UpdateControlOrder();
        }
        
        private void GeneratePredictionButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            ScheduleGenerator.Instance.GeneratePredictionSchedule(SaleDate,StartDate,EndDate);

            }
            catch (Exception exception)
            {
                MessageBox.Show("Schedule generator encountered an error:\n"+exception.Message);
            }
        }
    }
}
