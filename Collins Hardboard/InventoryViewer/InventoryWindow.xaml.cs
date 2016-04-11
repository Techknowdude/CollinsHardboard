using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using Microsoft.Win32;
using ModelLib;

namespace InventoryViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class InventoryWindow : Window
    {
        private const string datFile = "InventoryData.dat";

        private ObservableCollection<InventoryItemControl>  _itemControls = new ObservableCollection<InventoryItemControl>();

        public ObservableCollection<InventoryItemControl> ItemControls { get { return _itemControls; } }
        public InventoryWindow()
        {
            InitializeComponent();
            DataContext = this;
            ItemListView.DataContext = ItemControls;
            ItemListView.ItemsSource = ItemControls;

            foreach (var inventoryItem in StaticInventoryTracker.InventoryItems)
            {
                ItemControls.Add(new InventoryItemControl(this, inventoryItem));
            }
        }

        private void AddMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // add selection of master item or custom?
            InventoryItem item = new InventoryItem("");
            StaticInventoryTracker.InventoryItems.Add(item);
            ItemControls.Insert(0,new InventoryItemControl(this,item));
        }

        private void LoadInventoryMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ItemControls.Clear();

            foreach (var inventoryItem in StaticInventoryTracker.InventoryItems)
            {
                ItemControls.Add(new InventoryItemControl(this, inventoryItem));
            }
        }

        public void RemoveControl(InventoryItemControl inventoryItemControl)
        {
            if (
                MessageBox.Show("Are you sure you want to remove this item from inventory?", "Confirm", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                // remove from inv
                StaticInventoryTracker.InventoryItems.RemoveAt(ItemControls.IndexOf(inventoryItemControl));
                ItemControls.Remove(inventoryItemControl);
            }
        }
    }
}
