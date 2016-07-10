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

        public ScheduleGenWindow()
        {
            InitializeComponent();

            ScheduleGenerator.Window = this;
            ControlsListView.ItemsSource = ScheduleGenerator.ControlsList;

            //Closing += ScheduleGenWindow_Closing;
            if(!ScheduleGenerator.ControlsList.Any(c => c is SalesPrediction))
                ScheduleGenerator.ControlsList.Add(new SalesPrediction(this,200));

        }

        //void ScheduleGenWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    if (!SaveSettings())
        //    {
        //        if (MessageBox.Show("Saving failed. Close anyway?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
        //            e.Cancel = true;
        //    }
        //}

        //private bool SaveSettings()
        //{
        //    return ScheduleGenerator.SaveSettings();
        //}

        public void Remove(GenControl control)
        {
            ScheduleGenerator.ControlsList.Remove(control);
        }

        public void UpdateControlOrder()
        {
            ScheduleGenerator.UpdateControlOrder();
        }

        private void GenerateSalesButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScheduleGenerator.GenerateSalesSchedule(SaleDate, StartDate, EndDate);
        }

        private void GeneratePredictionButton_OnClick(object sender, RoutedEventArgs e)
        {

            ScheduleGenerator.GeneratePredictionSchedule(SaleDate,StartDate,EndDate);
        }

        //private void AddControlButton_Click(object sender, RoutedEventArgs e)
        //{
        //    switch (NewControlComboBox.SelectedIndex)
        //    {
        //        case 0:
        //            ScheduleGenerator.ControlsList.Add(new LineControl(this));
        //            break;
        //        case 1:
        //            ScheduleGenerator.ControlsList.Add(new PurgeWiPControl(this));
        //            break;
        //        case 2:
        //            ScheduleGenerator.ControlsList.Add(new RunBeforeControl(this));
        //            break;
        //        case 3:
        //            ScheduleGenerator.ControlsList.Add(new SalesNumbersControl(this));
        //            break;
        //        case 4:
        //            ScheduleGenerator.ControlsList.Add(new SalesPrediction(this));
        //            break;
        //        case 5:
        //            ScheduleGenerator.ControlsList.Add(new TextureControl(this));
        //            break;
        //        case 6:
        //            ScheduleGenerator.ControlsList.Add(new WasteControl(this));
        //            break;
        //        case 7:
        //            ScheduleGenerator.ControlsList.Add(new WidthControl(this));
        //            break;
        //    }
        //}

        //private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (SaveSettings())
        //    {
        //        MessageBox.Show("Save succeeded.");
        //    }
        //    else
        //    {
        //        MessageBox.Show("Save failed.");
        //    }
        //}

        //private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (ScheduleGenerator.LoadSettings())
        //    {
        //        MessageBox.Show("Load successful.");
        //    }
        //    else
        //    {
        //        MessageBox.Show("Load failed. Please select file to load.");
        //        OpenFileDialog fileDialog = new OpenFileDialog();
        //        fileDialog.FileName = ScheduleGenerator.datFile;
        //        fileDialog.Multiselect = false;

        //        if (fileDialog.ShowDialog() == true)
        //        {
        //            if (ScheduleGenerator.LoadSettings(fileDialog.FileName))
        //                MessageBox.Show("Load successful.");
        //            else
        //                MessageBox.Show("Load failed.");
        //        }
        //    }

        //}
    }
}
