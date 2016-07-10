using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ImportLib;
using ModelLib;

namespace WarehouseManager
{
    /// <summary>
    /// Interaction logic for PendingSalesWindow.xaml
    /// </summary>
    public partial class PendingSalesWindow : Window
    {
        public ObservableCollection<SalesItem> SalesItems
        {
            get { return StaticInventoryTracker.SalesItems; }
            set { StaticInventoryTracker.SalesItems = value; }
        } 

        public PendingSalesWindow()
        {
            InitializeComponent();
        }
    }
}
