using System.Windows.Controls;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PressMasterItemControl.xaml
    /// </summary>
    public partial class PressMasterItemControl : UserControl
    {
        public PressMasterItem Item { get; set; }
        public PressMasterItemControl(PressMasterItem item)
        {
            InitializeComponent();
            Item = item;
            DataContext = Item;
        }

        public PressMasterItemControl()
        {
            InitializeComponent();
            Item = DataContext as PressMasterItem;
        }
    }
}
