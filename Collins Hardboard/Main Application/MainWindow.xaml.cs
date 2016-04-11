using System.Windows;
using CoatingScheduler;
using Configuration_windows;
using ImportLib;
using ProductionScheduler;
using WarehouseManager;
using InventoryViewer;
using ScheduleGen;
using StaticHelpers;

namespace Main_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CoatingSchedule schedule = new CoatingSchedule();

        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;

            LoadDataFiles();
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Save any changes before closing?", "", MessageBoxButton.YesNoCancel);
            if(result == MessageBoxResult.Yes)
                SaveDataFiles();
            else if (result == MessageBoxResult.Cancel)
                e.Cancel = true;
        }

        private void SaveDataFiles()
        {
            StaticInventoryTracker.SaveDefaults();
            StaticFactoryValuesManager.SaveValues();
            if(schedule != null)
                schedule.Save();
        }

        private void LoadDataFiles()
        {
            StaticInventoryTracker.LoadDefaults();
            StaticFactoryValuesManager.LoadValues();
            var loadschedule = CoatingSchedule.Load();
            if (loadschedule != null)
                schedule = loadschedule;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ForcastWindow forcastWindow = new ForcastWindow();
            forcastWindow.Show();
            forcastWindow.Focus();
        }

        private void BtnWiPInventory_Click(object sender, RoutedEventArgs e)
        {
            WiPInventoryWindow wiPInventoryWindow = new WiPInventoryWindow();
            wiPInventoryWindow.Show();
            wiPInventoryWindow.Focus();
        }

        private void BtnOpenCoatingSchedule_Click(object sender, RoutedEventArgs e)
        {
            // prompt for schedule to open here...
            
            CoatingScheduleWindow window = new CoatingScheduleWindow(schedule);
            //schedule.Connect(window);
            window.Show();
            window.Focus();
        }

        private void btnImportInternalInventory_Click(object sender, RoutedEventArgs e)
        {
            InternalImport.GetInstance().ImportInventory();
        }

        private void btnImportInternalMaster_Click(object sender, RoutedEventArgs e)
        {
            InternalImport.GetInstance().ImportMaster();
        }

        private void btnImportInternalSales_Click(object sender, RoutedEventArgs e)
        {
            InternalImport.GetInstance().ImportSales();
        }

        private void ConfigMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigurationsWindow window = new ConfigurationsWindow();
            window.Show();
            window.Focus();
        }

        private void MachinesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            MachineConfigWindow window = new MachineConfigWindow();
            window.Show();
            window.Focus();
        }

        private void CoatingShiftMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ShiftWindow shiftWindow = new ShiftWindow(true);
            shiftWindow.Show();
            shiftWindow.Focus();
        }

        private void ProductionShiftMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ShiftWindow shiftWindow = new ShiftWindow(false);
            shiftWindow.Show();
            shiftWindow.Focus();
        }

        private void BtnOpenPressSchedule_Click(object sender, RoutedEventArgs e)
        {
            PressScheduleWindow window = new PressScheduleWindow();
            window.Show();
            window.Focus();
        }

        private void BtnOpenInventory_Click(object sender, RoutedEventArgs e)
        {
            InventoryWindow window = new InventoryWindow();
            window.Show();
            window.Focus();
        }

        private void PlantSettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            PlantSettingsWindow window = new PlantSettingsWindow();
            window.Show();
            window.Focus();
        }

        private void BtnOpenGenerator_Click(object sender, RoutedEventArgs e)
        {
            ScheduleGenWindow window = new ScheduleGenWindow();

            window.Show();
            window.Focus();   
        }

        private void Efficiency_Click(object sender, RoutedEventArgs e)
        {
            EfficiencyViewer window = new EfficiencyViewer();
            window.Show();
            window.Focus();
        }

        private void btnImportInternalForecast_Click(object sender, RoutedEventArgs e)
        {
            InternalImport.GetInstance().ImportSalesForecast();
        }

        private void BtnImportInternalAll_OnClick(object sender, RoutedEventArgs e)
        {
            InternalImport.GetInstance().ImportAll();
        }
    }
}
