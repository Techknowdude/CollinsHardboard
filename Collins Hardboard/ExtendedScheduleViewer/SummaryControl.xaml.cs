using System.Windows.Controls;

namespace ExtendedScheduleViewer
{
    /// <summary>
    /// Interaction logic for SummaryControl.xaml
    /// </summary>
    public partial class SummaryControl : UserControl
    {
        public ItemSummary Summary { get; set; }

        public SummaryControl()
        {
            InitializeComponent();
            Summary = new ItemSummary(null,100,30,20);
            DataContext = Summary;
        }

        public SummaryControl(ItemSummary summary)
        {
            InitializeComponent();
            Summary = summary;
            DataContext = Summary;
        }
    }
}
