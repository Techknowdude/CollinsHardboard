using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for TrackingItemControl.xaml
    /// </summary>
    public partial class TrackingItemControl : UserControl
    {
        public TrackingSelectionWindow ParentWindow { get; set; }
        public ProductMasterItem Item { get; set; }

        public TrackingItemControl(ProductMasterItem item, TrackingSelectionWindow parentWindow)
        {
            InitializeComponent();

            ItemDropDown.ItemsSource = StaticInventoryTracker.ProductMasterList;
            ParentWindow = parentWindow;

            Item = item;
            if (Item != null)
            {
                ItemDropDown.SelectedItem = Item;

                ProductMasterItem found = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.Equals(Item));

                if (found != null)
                {
                    Int32 index = StaticInventoryTracker.ProductMasterList.IndexOf(found);
                    if (index != -1)
                    {
                        ItemDropDown.SelectedIndex = index;
                    }
                }
            }
        }


        private void ItemDropDown_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemDropDown.SelectedIndex != -1)
            {
                Item = (ProductMasterItem) e.AddedItems[0];
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }
    }
}
