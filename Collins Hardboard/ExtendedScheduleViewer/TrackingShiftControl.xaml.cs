using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ExtendedScheduleViewer
{
    /// <summary>
    /// Interaction logic for TrackingShiftControl.xaml
    /// </summary>
    public partial class TrackingShiftControl : UserControl
    {
        private TrackingShift _shift;
        private ObservableCollection<SummaryControl> _summaryControls = new ObservableCollection<SummaryControl>();

        public ObservableCollection<SummaryControl> SummaryControls
        {
            get { return _summaryControls; }
            set { _summaryControls = value; }
        }

        public TrackingShift Shift
        {
            get { return _shift; }
            set
            {
                _shift = value;
            }
        }

        public TrackingShiftControl()
        {
            InitializeComponent();
        }
        public TrackingShiftControl(TrackingShift shift)
        {
            Shift = shift;
            InitializeComponent();
            Shift.Control = this;
        }

        public void AddSummary(ItemSummary newSum)
        {
            var newControl = new SummaryControl(newSum);
            SummaryControls.Add(newControl);
        }

        public void ClearControls()
        {
            SummaryControls.Clear();
        }
    }
}
