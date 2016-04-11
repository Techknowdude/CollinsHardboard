using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using ImportLib;
using ModelLib;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for TrackingSelectionWindow.xaml
    /// </summary>
    public partial class TrackingSelectionWindow : Window
    {
        private static ObservableCollection<TrackingItemControl> _trackingList = new ObservableCollection<TrackingItemControl>();
        private static ObservableCollection<ProductMasterItem> _trackingItems;

        public static ObservableCollection<ProductMasterItem> TrackingItems
        {
            get
            {
                if (_trackingItems == null)
                {
                    _trackingItems = new ObservableCollection<ProductMasterItem>();
                }
                if (_trackingItems.Count == 0)
                {
                    LoadSettings(null);
                }
                return _trackingItems;
            }
            set { _trackingItems = value; }
        }

        public TrackingSelectionWindow()
        {
            InitializeComponent();
            Closing += TrackingSelectionWindow_Closing;

            MainListView.ItemsSource = _trackingList;

            LoadSettings(this);

        }

        void TrackingSelectionWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // save changes to the tracking list
            _trackingItems.Clear(); // clear list
            foreach (var trackingItemControl in _trackingList.Where(trackingItemControl => trackingItemControl.Item != null))
            {
                _trackingItems.Add(trackingItemControl.Item);
            }
            if (!SaveSettings())
            {
                e.Cancel = true;
                if (MessageBox.Show("There was a problem saving changes. Close anyway?", "", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    e.Cancel = false;
                }
            }
        }

        public void Remove(TrackingItemControl trackingItemControl)
        {
            _trackingList.Remove(trackingItemControl);
            if (trackingItemControl.Item != null) _trackingItems.Remove(trackingItemControl.Item);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _trackingList.Add(new TrackingItemControl(null,this));
        }

        public bool SaveSettings()
        {
            try
            {

                using (BinaryWriter writer = new BinaryWriter(new FileStream("Tracking.dat", FileMode.OpenOrCreate)))
                {

                    Int32 count = _trackingList.Where(x => x.Item != null).ToList().Count;
                    writer.Write(count);

                    foreach (var trackingItemControl in _trackingList.Where(x => x.Item != null))
                    {
                            trackingItemControl.Item.Save(writer);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void LoadSettings(TrackingSelectionWindow trackingSelectionWindow)
        {
            try
            {

                using (BinaryReader reader = new BinaryReader(new FileStream("Tracking.dat", FileMode.OpenOrCreate)))
                {
                    TrackingItems.Clear();
                    _trackingList.Clear();

                    Int32 count = reader.ReadInt32();

                    for (; count > 0; count--)
                    {
                        ProductMasterItem item = ProductMasterItem.Load(reader);

                        ProductMasterItem found = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.Equals(item));

                        if (found != null)
                            item = found;

                        TrackingItems.Add(item);
                        _trackingList.Add(new TrackingItemControl(item, trackingSelectionWindow));
                    }
                }
            }
            catch (Exception)
            {
            } 
        }
    }
}
