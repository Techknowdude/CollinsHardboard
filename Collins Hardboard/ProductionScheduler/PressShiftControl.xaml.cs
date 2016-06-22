using System.Windows;
using System.Windows.Controls;
using ImportLib;

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
            if (shift != null)
            {
                Shift = shift;
                DataContext = Shift;
            }
            else
            {
                Shift = DataContext as PressShift;
            }

            ProductSelectionBox.ItemsSource = StaticInventoryTracker.PressMasterList;
        }

        public PressShiftControl()
        {
            InitializeComponent();
            Shift = DataContext as PressShift;
            if(Shift == null)
                Shift = Content as PressShift;
            

            ProductSelectionBox.ItemsSource = StaticInventoryTracker.PressMasterList;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Shift?.AddCommand.Execute(null);
        }
    }
}
