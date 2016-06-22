using System.Windows.Controls;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PressPlateConfigurationControl.xaml
    /// </summary>
    public partial class PressPlateConfigurationControl : UserControl
    {
        public PlateConfiguration PlateConfiguration { get; set; }

        public PressPlateConfigurationControl(PlateConfiguration configuration = null)
        {
            InitializeComponent();
            if (configuration != null)
                PlateConfiguration = configuration;
            DataContext = PlateConfiguration;
        }
    }
}
