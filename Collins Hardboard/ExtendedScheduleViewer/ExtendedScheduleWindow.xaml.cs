using System.Collections.ObjectModel;
using System.Windows;

namespace ExtendedScheduleViewer
{
    /// <summary>
    /// Interaction logic for ExtendedScheduleWindow.xaml
    /// </summary>
    public partial class ExtendedScheduleWindow : Window
    {
        ExtendedSchedule schedule = ExtendedSchedule.Instance;
        private ObservableCollection<TrackingDayControl> _dayControls = new ObservableCollection<TrackingDayControl>();

        public ObservableCollection<TrackingDayControl> DayControls
        {
            get { return _dayControls; }
            set { _dayControls = value; }
        }

        public ExtendedScheduleWindow()
        {
            InitializeComponent();
            schedule.Window = this;
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

        public void AddDayControl(TrackingDay day)
        {
            DayControls.Add(new TrackingDayControl(day));
        }
    }
}
