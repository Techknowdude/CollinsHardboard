using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private ObservableCollection<TrackingDay> _trackingDays = new ObservableCollection<TrackingDay>();
        private ObservableCollection<ProductMasterItem> _watches = new ObservableCollection<ProductMasterItem>();
        private static ExtendedSchedule _instance;
        public static ExtendedSchedule Instance
        {
            get { return _instance ?? (_instance = new ExtendedSchedule()); }
        }

        public static Dictionary<ProductMasterItem, double> runningTotalsDictionary = new Dictionary<ProductMasterItem, double>();

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
        }

        public ICommand UpdateCommand { get { return new DelegateCommand(Update);} }
        public ExtendedScheduleWindow Window { get; set; }

        public void Update()
        {
            runningTotalsDictionary.Clear();
            AddWatch(StaticInventoryTracker.PressMasterList[0]);
            AddWatch(StaticInventoryTracker.ProductMasterList[1]);
            TrackingDays.Clear();
            Window.DayControls.Clear();

            if(CoatingSchedule.CurrentSchedule == null) return;

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

        void Test()
        {
            ProductMasterItem item = StaticInventoryTracker.PressMasterList[0];

            CoatingSchedule newSchedule = new CoatingSchedule();
            newSchedule.AddLogic();
            CoatingScheduleDay day = newSchedule.ChildrenLogic.Last() as CoatingScheduleDay;
            day.Date = day.Date.AddDays(2);
            day.AddLogic();
            CoatingScheduleShift shift = day.ChildrenLogic.Last().ChildrenLogic.Last() as CoatingScheduleShift;
            CoatingScheduleProduct product = new CoatingScheduleProduct(item);
            shift.AddLogic(product);
            product.Units = "1";

            AddDay(new TrackingDay(day));
            AddTrackingItem(item);
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

        public Dictionary<ProductMasterItem, double> GetPreviousInventory( TrackingDay day)
        {
            Dictionary<ProductMasterItem, double> current = null;

            int index = TrackingDays.IndexOf(day);

            if (index > 0)
            {
                current = TrackingDays[index - 1].GetCounts();
            }

            return current;
        }
    }
}
