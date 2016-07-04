using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModelLib;
using ProductionScheduler;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    /// <summary>
    /// Interaction logic for ExtendedScheduleWindow.xaml
    /// </summary>
    public partial class ExtendedScheduleWindow : Window
    {
        ExtendedSchedule schedule = ExtendedSchedule.Instance;
        private ObservableCollection<TrackingDayControl> _dayControls = new ObservableCollection<TrackingDayControl>();
        private ObservableCollection<ProductMasterItem> _watchItems;

        public ObservableCollection<ProductMasterItem> WatchItems => ExtendedSchedule.Instance.Watches;

        private ListView watchView;

        public ObservableCollection<TrackingDayControl> DayControls
        {
            get { return _dayControls; }
            set { _dayControls = value; }
        }

        public ExtendedScheduleWindow()
        {
            InitializeComponent();
            watchView = TrackingNameListView;
            schedule.Window = this;
            PressManager.Instance.GetHashCode(); // init
            schedule.Update();
        }

        public ExtendedSchedule Schedule
        {
            get { return schedule; }
            set
            {
                schedule = value;
                schedule.Window = this;
            }
        }

        public void ClearWindow()
        {
            
        }

        public void AddDayControl(TrackingDay day)
        {
            DayControls.Add(new TrackingDayControl(day));
        }

        public ICommand ChangeWatchCommand { get { return new DelegateCommand(ShowWatchWindow);} }

        private void ShowWatchWindow()
        {
            WatchWindow window = new WatchWindow();
            window.Show();
        }
    }
}
