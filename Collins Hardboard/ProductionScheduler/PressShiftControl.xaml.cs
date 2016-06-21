using System.Windows.Controls;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PressShiftControl.xaml
    /// </summary>
    public partial class PressShiftControl : UserControl
    {
        public PressShift Shift { get; set; }

        public PressShiftControl(PressShift shift)
        {
            InitializeComponent();
            Shift = shift;
            DataContext = Shift;
        }
    }
}
