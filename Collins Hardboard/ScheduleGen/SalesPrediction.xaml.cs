using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ImportLib;
using ModelLib;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for SalesPrediction.xaml
    /// </summary>
    public partial class SalesPrediction : UserControl
    {
        public SalesDurationEnum SalesDuration { get; set; }

        public static String Type { get { return "SalesPrediction"; } }
        public SalesPrediction(ScheduleGenWindow window, int priority = 0, SalesDurationEnum duration = default(SalesDurationEnum))
        {
            InitializeComponent();
            SalesDuration = duration;
            switch(SalesDuration) // set box to correct index
            {
                case SalesDurationEnum.Last12Months:
                    PastSalesComboBox.SelectedIndex = 0;
                    break;

                case SalesDurationEnum.Last3Months:
                    PastSalesComboBox.SelectedIndex = 1;
                    break;

                case SalesDurationEnum.Last6Months:
                    PastSalesComboBox.SelectedIndex = 2;
                    break;

                case SalesDurationEnum.LastMonth:
                    PastSalesComboBox.SelectedIndex = 3;
                    break;

                case SalesDurationEnum.LastYear:
                    PastSalesComboBox.SelectedIndex = 4;
                    break;
            }
        }

        private void PastSalesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = PastSalesComboBox.SelectedIndex;
            if (index != -1)
            {
                SalesDuration = (SalesDurationEnum) index;
            }
        }

    }
}
