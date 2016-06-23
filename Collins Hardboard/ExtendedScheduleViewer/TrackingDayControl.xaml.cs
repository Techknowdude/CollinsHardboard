using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ExtendedScheduleViewer
{
    /// <summary>
    /// Interaction logic for TrackingDayControl.xaml
    /// </summary>
    public partial class TrackingDayControl : UserControl
    {
        private TrackingDay _day;
        private ObservableCollection<TrackingShiftControl> _shiftControls = new ObservableCollection<TrackingShiftControl>();

        public TrackingDay Day
        {
            get { return _day; }
            set
            {
                _day = value;
            }
        }

        public ObservableCollection<TrackingShiftControl> ShiftControls
        {
            get { return _shiftControls; }
            set { _shiftControls = value; }
        }


        public TrackingDayControl(TrackingDay day)
        {
            InitializeComponent();
            Day = day;
            DataContext = Day;
            Day.Control = this;
        }

        public void ClearShifts()
        {
            _shiftControls.Clear();   
        }

        public void AddControl(TrackingShift shift)
        {
            _shiftControls.Add(new TrackingShiftControl(shift));
        }
    }
}
