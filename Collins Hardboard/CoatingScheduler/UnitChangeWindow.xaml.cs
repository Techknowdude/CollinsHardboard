using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for UnitChangeWindow.xaml
    /// </summary>
    public partial class UnitChangeWindow : Window
    {
        public String Item { get; set; }
        public String Expected { get; set; }

        public String UnitsText
        {
            get { return Units.ToString("N"); }
            set
            {
                double data;
                if (Double.TryParse(value, out data))
                    Units = data;
            }
        }

        public String MessageText { get
        {
            return String.Format("Expected to make {0} of {1}. How many units were made?", Expected, Item);
        } }

        public double Units { get; set; }
        public UnitChangeWindow(string item,string expected)
        {
            Item = item;
            Expected = expected;

            DataContext = this;

            InitializeComponent();

            Units = 0;

        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
