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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImportLib;
using ModelLib;

namespace InventoryViewer
{
    /// <summary>
    /// Interaction logic for ItemEfficiencyControl.xaml
    /// </summary>
    public partial class ItemEfficiencyControl : UserControl
    {
        public ItemEfficiencyControl(InventoryChange inventoryChange)
        {
            InitializeComponent();

            var productMasterItem = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == inventoryChange.MasterId);
            if (productMasterItem != null)
                NameTextBox.Text = productMasterItem.Description;
            else
            {
                NameTextBox.Text = "Not Found";
            }

            EfficiencyTextBox.Text = inventoryChange.Efficiency.ToString("P");
        }
    }
}
