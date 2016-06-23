using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CoatingScheduler;
using Configuration_windows;
using ModelLib;
using ProductionScheduler;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    public class TrackingShift : ObservableObject
    {

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

        private void SetPressShifts()
        {
            PressShifts.Clear();
            DateTime lastShiftEnd = ShiftHandler.CoatingInstance.GetPreviousShiftEnd(StaticFunctions.GetDayAndTime(CoatingLine.Date,
                CoatingLine.Shift.StartTime),CoatingLine.Shift);
            var timeAfterShift = StaticFunctions.GetDayAndTime(CoatingLine.Date, CoatingLine.Shift.StartTime) +
                                 CoatingLine.Shift.Duration;
            var pShifts = PressManager.Instance.GetPressShifts(lastShiftEnd, timeAfterShift);
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
            SetPressShifts();
        }

        public double GetProduced(ProductMasterItem item)
        {
            double modification = 0;

            modification = _coatingLine.UnitsProduced(item);

            foreach (var pressShift in _pressShifts)
            {
                foreach (var prod in pressShift.Produced.Where(i => i.MasterItem == item))
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
            if (ExtendedSchedule.runningTotalsDictionary.ContainsKey(item))
            {
                running = ExtendedSchedule.runningTotalsDictionary[item];
            }

            double added = GetProduced(item);
            double removed = GetConsumed(item);

            var newSum = new ItemSummary(item,running,added,removed);

            ItemSummaries.Add(newSum);
            Control.AddSummary(newSum);

            ExtendedSchedule.runningTotalsDictionary[item] = newSum.RunningUnits;

            return newSum.RunningUnits;
        }

        public Dictionary<ProductMasterItem, double> GetCounts()
        {
            Dictionary<ProductMasterItem,double> lastCountsDictionary = new Dictionary<ProductMasterItem, double>();

            foreach (var itemSummary in ItemSummaries)
            {
                if(!lastCountsDictionary.ContainsKey(itemSummary.Item))
                    lastCountsDictionary.Add(itemSummary.Item,itemSummary.RunningUnits);
                else
                {
                    lastCountsDictionary[itemSummary.Item] = itemSummary.RunningUnits;
                }
            }

            return lastCountsDictionary;
        }

        public void PopulateSummaries(Dictionary<ProductMasterItem, double> lastCountDictionary)
        {
            foreach (var watchedItem in ExtendedSchedule.Instance.Watches)
            {
                if (lastCountDictionary!= null && lastCountDictionary.ContainsKey(watchedItem))
                {
                    lastCountDictionary[watchedItem] = AddSummary(watchedItem, lastCountDictionary[watchedItem]);
                }
                else
                {
                    AddSummary(watchedItem, 0);
                }
            }
        }
        
    }
}