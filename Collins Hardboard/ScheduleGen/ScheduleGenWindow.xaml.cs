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
        public GenerationSettings GenerationSettings { get; set; }

        public ScheduleGenWindow()
        {
            GenerationSettings = new GenerationSettings();

            InitializeComponent();

            ScheduleGenerator.Instance.Window = this;
        }


        private void GeneratePredictionButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if(GenerationSettings != null)
                    ScheduleGenerator.Instance.GenerateSchedule(GenerationSettings);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Schedule generator encountered an error:\n"+exception.Message);
            }
        }
    }
}
