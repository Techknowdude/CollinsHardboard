using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CoatingScheduler;
using Configuration_windows;
using ImportLib;
using ModelLib;
using ProductionScheduler;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    public class TrackingShift : ObservableObject
    {
        public const int STANDARD_SHIFT_LENGTH = 8;

        public String ShiftTitle
        {
            get
            {
                if (CoatingLine != null) return CoatingLine.ShiftName;
                return _shiftTitle;
            }
            set
            {
                if (CoatingLine != null) CoatingLine.ShiftName = value;
                _shiftTitle = value;
            }
        }

        public ObservableCollection<PressShift> PressShifts
        {
            get { return _pressShifts; }
            set
            {
                _pressShifts = value;
                RaisePropertyChangedEvent();
            }
        }

        public CoatingScheduleLine CoatingLine
        {
            get { return _coatingLine; }
            set
            {
                _coatingLine = value; 
                SetPressShifts();
                RaisePropertyChangedEvent();
            }
        }

        public ObservableCollection<ItemSummary> ItemSummaries
        {
            get { return _itemSummaries; }
            set { _itemSummaries = value; }
        }

        public TrackingShiftControl Control { get; set; }

        private ObservableCollection<PressShift> _pressShifts;
        private CoatingScheduleLine _coatingLine;
        private string _shiftTitle;
        private ObservableCollection<ItemSummary> _itemSummaries = new ObservableCollection<ItemSummary>();
        private DateTime date { get; set; }

        private void SetPressShifts()
        {
            PressShifts.Clear();
            DateTime lastShiftBegin = ShiftHandler.CoatingInstance.GetPreviousShiftStart(StaticFunctions.GetDayAndTime(CoatingLine.Date,
                CoatingLine.Shift.StartTime),CoatingLine.Shift);
            var thisShiftStart = StaticFunctions.GetDayAndTime(CoatingLine.Date, CoatingLine.Shift.StartTime);

            // Use previous day if this shift starts on the previous day
            if ((new TimeSpan(CoatingLine.Shift.StartTime.Hour, CoatingLine.Shift.StartTime.Minute, CoatingLine.Shift.StartTime.Second) +
                 CoatingLine.Shift.Duration).TotalHours > 24)
                thisShiftStart = thisShiftStart.AddDays(-1);

            var pShifts = PressManager.Instance.GetPressShifts(lastShiftBegin, thisShiftStart);
            foreach (var pressShift in pShifts)
            {
                PressShifts.Add(pressShift);
            }
        }

        public TrackingShift()
        {
            
        }

        public TrackingShift(CoatingScheduleLine line)
        {
            _pressShifts = new ObservableCollection<PressShift>();
            _coatingLine = line;
            date = line.Date + TimeSpan.FromHours(STANDARD_SHIFT_LENGTH); // add duration to snap to next day on GY shift
            date = new DateTime(date.Year,date.Month,date.Day);
            SetPressShifts();
        }


        public double GetProduced(ProductMasterItem item)
        {
            double modification = 0;

            modification = _coatingLine.UnitsProduced(item);

            foreach (var pressShift in _pressShifts)
            {
                foreach (var prod in pressShift.Produced.Where(i => i.MasterItem.Equals(item)))
                {
                    modification += prod.UnitsMade;
                }
            }
            return modification;
        }

        public double GetConsumed(ProductMasterItem item)
        {
            double consumed = 0;

            consumed = _coatingLine.UnitsConsumed(item);

            return consumed;
        }

        public override string ToString()
        {
            if (CoatingLine != null)
                return CoatingLine.ShiftName;
            else
            {
                return "Shift";
            }
        }

        /// <summary>
        /// Adds the summary
        /// </summary>
        /// <param name="item">Item to track</param>
        /// <param name="running">Current unit amount going into shift</param>
        /// <returns>Units in inventory after shift</returns>
        public double AddSummary(ProductMasterItem item, double running)
        {
            double added = GetProduced(item);
            double removed = GetConsumed(item);

            var newSum = new ItemSummary(item,running,added,removed);

            ItemSummaries.Add(newSum);
            Control.AddSummary(newSum);

            ExtendedSchedule.RunningTotalsDictionary[item] = newSum.RunningUnits;

            return newSum.RunningUnits;
        }

        //public Dictionary<ProductMasterItem, double> GetCounts()
        //{
        //    Dictionary<ProductMasterItem,double> lastCountsDictionary = new Dictionary<ProductMasterItem, double>();

        //    foreach (var itemSummary in ItemSummaries)
        //    {
        //        if(!lastCountsDictionary.ContainsKey(itemSummary.Item))
        //            lastCountsDictionary.Add(itemSummary.Item,itemSummary.RunningUnits);
        //        else
        //        {
        //            lastCountsDictionary[itemSummary.Item] = itemSummary.RunningUnits;
        //        }
        //    }

        //    return lastCountsDictionary;
        //}

        public void PopulateSummaries()
        {
            foreach (var watchedItem in ExtendedSchedule.Instance.Watches)
            {
                if (ExtendedSchedule.RunningTotalsDictionary != null && ExtendedSchedule.RunningTotalsDictionary.ContainsKey(watchedItem))
                {
                    ExtendedSchedule.RunningTotalsDictionary[watchedItem] = AddSummary(watchedItem, ExtendedSchedule.RunningTotalsDictionary[watchedItem]);
                }
                else
                {
                    AddSummary(watchedItem, 0);
                }
            }
        }

        public void RemoveSales()
        {
            List<SalesItem> soldYesterday = StaticInventoryTracker.SalesItems.Where(s => s.Date >= date.AddDays(-1) && s.Date < date).ToList();

            foreach (var itemSummary in ItemSummaries)
            {
                foreach (var order in soldYesterday.Where(s => s.MasterID == itemSummary.Item.MasterID))
                {
                    itemSummary.RemovedUnits += order.Units;
                }
            }

            //update the count dictionary
            foreach (var salesItem in soldYesterday)
            {
                var master =
                    StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == salesItem.MasterID);
                if (master != null)
                {
                    if (ExtendedSchedule.RunningTotalsDictionary.ContainsKey(master))
                    {
                        ExtendedSchedule.RunningTotalsDictionary[master] -= salesItem.Units;
                    }
                    else
                    {
                        ExtendedSchedule.RunningTotalsDictionary[master] = -salesItem.Units;
                    }
                }
            }
        }
    }
}