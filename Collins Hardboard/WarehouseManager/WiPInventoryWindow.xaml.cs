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
        private const string datFile = "WiPData.dat";

        public static ObservableCollection<WiPItemControl> WiPItems
        {
            get { return _wiPItems; }
            set { _wiPItems = value; }
        }

        private static bool loaded = false;

        public WiPInventoryWindow()
        {
            // set view model
            var vm = StaticInventoryTracker.Instance;
            DataContext = vm;

            InitializeComponent();
            WiPItemView.ItemsSource = WiPItems;
            Closing += OnClosing;

            if (!loaded && !LoadWiPData())
            {
                loaded = true;
                ImportWiPFromTracker();
            }
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
            if (MessageBox.Show("Would you like to save any changes before closing?", "", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                if (!SaveWiPData())
                {
                    MessageBox.Show("Save failed. Canceling window close.");
                    cancelEventArgs.Cancel = true;
                }
            }
        }

        private void AddWiPButton_OnClick(object sender, RoutedEventArgs e)
        {
            WiPItems.Add(new WiPItemControl(new InventoryItem(""),this));
        }

        private bool LoadWiPData()
        {
            bool done = TryLoadWip(datFile);
            if (!done)
            {
                if (MessageBox.Show("Default data load failed. Open other WiP data?", "", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    OpenFileDialog fileName = new OpenFileDialog();
                    fileName.DefaultExt = ".dat";
                    fileName.Multiselect = false;
                    fileName.Title = "Open WiP data file";

                    if (fileName.ShowDialog() == true)
                    {
                        done = TryLoadWip(fileName.FileName);
                    }
                }
            }

            return done;
        }

        private bool TryLoadWip(String fileName)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
                {
                    WiPItems.Clear();
                    int number = reader.ReadInt32();

                    for (; number > 0; --number)
                    {
                        InventoryItem item = InventoryItem.LoadItem(reader);

                        WiPItems.Add(new WiPItemControl(item,this));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private bool SaveWiPData()
        {
            bool succeeded = false;
            if (MessageBox.Show("Save as default?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                succeeded = TrySaveWiP(datFile);
            }
            else
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.DefaultExt = ".dat";
                saveDialog.Title = "Save WiP Data";

                if (saveDialog.ShowDialog() == true)
                {
                    succeeded = TrySaveWiP(saveDialog.FileName);
                }
            }
            return succeeded;
        }

        private bool TrySaveWiP(string wipdataDat)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream(wipdataDat, FileMode.OpenOrCreate)))
                {
                    writer.Write(WiPItems.Count);

                    foreach (var wiPItemControl in WiPItems)
                    {
                        wiPItemControl.InvItem.SaveItem(writer);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SaveItem_OnClick(object sender, RoutedEventArgs e)
        {
            SaveWiPData();
        }

        private void LoadItem_OnClick(object sender, RoutedEventArgs e)
        {
            LoadWiPData();
        }

        private void ImportItem_OnClick(object sender, RoutedEventArgs e)
        {
            ImportWiPFromTracker();
        }

        public void Remove(WiPItemControl wiPItemControl)
        {
            WiPItems.Remove(wiPItemControl);
        }
    }
}
