using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;
using CoatingScheduler;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    [Serializable]
    public class ExtendedSchedule
    {
        private const string saveFile = "extendedScheduleWatchList.dat";
        [NonSerialized]
        private ObservableCollection<TrackingDay> _trackingDays = new ObservableCollection<TrackingDay>();
        private ObservableCollection<ProductMasterItem> _watches = new ObservableCollection<ProductMasterItem>();
        [NonSerialized]
        private static ExtendedSchedule _instance;
        public static ExtendedSchedule Instance
        {
            get { return _instance ?? (_instance = new ExtendedSchedule()); }
        }

        public static Dictionary<int, double> RunningTotalsDictionary = new Dictionary<int, double>();

        public ObservableCollection<ProductMasterItem> Watches
        {
            get { return _watches; }
            set { _watches = value; }
        }

        public ObservableCollection<TrackingDay> TrackingDays
        {
            get { return _trackingDays; }
            set { _trackingDays = value; }
        }

        private ExtendedSchedule()
        {
            Load();
        }

        public ICommand UpdateCommand { get { return new DelegateCommand(Update);} }
        public ExtendedScheduleWindow Window { get; set; }

        public ICommand ExportCommand
        {
            get { return new DelegateCommand(ExportToExcel);}
        }

        private void ExportToExcel()
        {
            ExtendedScheduleExcelExporter exporter = new ExtendedScheduleExcelExporter(this);
            exporter.Export();
        }

        public void Update()
        {
            RunningTotalsDictionary.Clear();
            TrackingDays.Clear();
            Window.DayControls.Clear();

            // update inventory data
            foreach (var inventoryItem in StaticInventoryTracker.AllInventoryItems)
            {
                if (Watches.Any(w => w.MasterID == inventoryItem.MasterID))
                {
                    ProductMasterItem keyMasterItem = null;

                    keyMasterItem = StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                                m => m.MasterID == inventoryItem.MasterID);

                    // if master item exists.
                    if (keyMasterItem != null)
                        RunningTotalsDictionary[keyMasterItem.MasterID] = inventoryItem.Units;
                }
            }
            // update WiP data
            foreach (var inventoryItem in StaticInventoryTracker.WiPItems)
            {
                if (Watches.Any(w => w.MasterID == inventoryItem.MasterID))
                {
                    ProductMasterItem keyMasterItem = null;

                    keyMasterItem = StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                                m => m.MasterID == inventoryItem.MasterID);

                    // if master item exists.
                    if (keyMasterItem != null)
                        RunningTotalsDictionary[keyMasterItem.MasterID] = inventoryItem.Units;
                }
            }


            if (CoatingSchedule.CurrentSchedule == null) return;

            foreach (var coatingScheduleLogic in CoatingSchedule.CurrentSchedule.ChildrenLogic)
            {
                var day = coatingScheduleLogic as CoatingScheduleDay;
                AddDay(new TrackingDay(day));
            }

            //foreach (var productMasterItem in Watches)
            //{
            //    double currentInv = 0;
            //    var inv = StaticInventoryTracker.InventoryItems.FirstOrDefault(x => x.MasterID == productMasterItem.MasterID);
            //    if (inv != null)
            //        currentInv = inv.Units;

            //    foreach (var trackingDay in TrackingDays)
            //    {
            //        currentInv = trackingDay.AddTracking(productMasterItem, currentInv);
            //    }
            //}

        }
        

        public void AddTrackingItem(ProductMasterItem item)
        {
            if (!Watches.Contains(item))
            {
                double currentInv = 0;
                var inv = StaticInventoryTracker.InventoryItems.FirstOrDefault(x => x.MasterID == item.MasterID);
                if (inv != null)
                    currentInv = inv.Units;

                Watches.Add(item);
                foreach (var trackingDay in TrackingDays)
                {
                    currentInv = trackingDay.AddTracking(item,currentInv);
                }
            }
        }

        public void AddDay(TrackingDay day)
        {
            Window.AddDayControl(day);
            TrackingDays.Add(day);
            day.Update();
        }

        public void RemoveDay(TrackingDay day)
        {
            TrackingDays.Remove(day);
        }

        public static void AddWatch(ProductMasterItem item)
        {
            if (!Instance.Watches.Contains(item))
            {
                Instance.Watches.Add(item);
            }
        }

        //public Dictionary<ProductMasterItem, double> GetPreviousInventory( TrackingDay day)
        //{
        //    Dictionary<ProductMasterItem, double> current = null;

        //    int index = TrackingDays.IndexOf(day);

        //    if (index > 0)
        //    {
        //        current = TrackingDays[index - 1].GetCounts();
        //    }

        //    return current;
        //}
        public void Save()
        {
            try
            {

            using (FileStream stream = File.OpenWrite(saveFile))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, Watches.Count);
                foreach (var watch in Watches)
                {
                    formatter.Serialize(stream, watch);
                }
            }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Load()
        {
            try
            {

            using (FileStream stream = File.OpenRead(saveFile))
            {
                BinaryFormatter formatter = new BinaryFormatter();

               Watches.Clear();

                int watchCount = (int)formatter.Deserialize(stream);

                for (; watchCount > 0; watchCount--)
                {
                    ProductMasterItem watch = (ProductMasterItem)formatter.Deserialize(stream);
                    Watches.Add(watch);
                }
                

            }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
