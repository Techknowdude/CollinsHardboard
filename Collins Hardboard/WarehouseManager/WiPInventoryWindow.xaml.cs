using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using ImportLib;
using Microsoft.Win32;
using ModelLib;

namespace WarehouseManager
{
    /// <summary>
    /// Interaction logic for WiPInventoryWindow.xaml
    /// </summary>
    public partial class WiPInventoryWindow : Window
    {
        private static ObservableCollection<WiPItemControl> _wiPItems = new ObservableCollection<WiPItemControl>();

        public static ObservableCollection<WiPItemControl> WiPItems
        {
            get { return _wiPItems; }
            set { _wiPItems = value; }
        }

        public WiPInventoryWindow()
        {

            InitializeComponent();
            WiPItemView.ItemsSource = WiPItems;
            Closing += OnClosing;

            ImportWiPFromTracker();
        }

        public static void UpdateControls()
        {
            foreach (var wip in WiPItems)
            {
                wip.UpdateControlData();
            }
        }

        private void ImportWiPFromTracker()
        {
            WiPItems.Clear();
            foreach (var inventoryitem in StaticInventoryTracker.WiPItems)
            {
                WiPItems.Add(new WiPItemControl(inventoryitem,this));
            }
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            StaticInventoryTracker.SaveDefaults();
        }

        private void AddWiPButton_OnClick(object sender, RoutedEventArgs e)
        {
            var inv = new InventoryItem("");
            StaticInventoryTracker.WiPItems.Add(inv);
            WiPItems.Add(new WiPItemControl(inv,this));
        }
        
        public void Remove(WiPItemControl wiPItemControl)
        {
            WiPItems.Remove(wiPItemControl);
            StaticInventoryTracker.WiPItems.Remove(wiPItemControl.InvItem);
        }
    }
}
