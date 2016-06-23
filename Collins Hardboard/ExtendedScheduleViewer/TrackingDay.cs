using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CoatingScheduler;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    public class TrackingDay : ObservableObject
    {
        private TrackingDayControl _control;
        private DateTime _day;
        private ObservableCollection<TrackingShift> _shifts = new ObservableCollection<TrackingShift>();
        private CoatingScheduleDay _coatingDay;

        public ObservableCollection<TrackingShift> Shifts
        {
            get { return _shifts; }
            set
            {
                _shifts = value;
                RaisePropertyChangedEvent();
            }
        }

        public DateTime Day
        {
            get
            {
                return _day;
            }

            set
            {
                _day = value;
                RaisePropertyChangedEvent();
            }
        }

        public CoatingScheduleDay CoatingDay
        {
            get { return _coatingDay; }
            set
            {
                _coatingDay = value;
                RaisePropertyChangedEvent();
            }
        }

        public TrackingDayControl Control
        {
            get { return _control; }
            set { _control = value; }
        }

        public TrackingDay()
        {
            
        }

        public TrackingDay(CoatingScheduleDay day)
        {
            _day = day.Date;
            CoatingDay = day;
        }

        public void Update()
        {
            Control.ClearShifts();
            _shifts.Clear();

            foreach (var coatingScheduleLogic in CoatingDay.ChildrenLogic)
            {
                var line = coatingScheduleLogic as CoatingScheduleLine;
                var newShift = new TrackingShift(line);
                Control.ShiftControls.Add(new TrackingShiftControl(newShift));
                _shifts.Add(newShift);
                var inventories = ExtendedSchedule.Instance.GetPreviousInventory(this);
                newShift.PopulateSummaries(inventories);
            }
        }
        

        /// <summary>
        /// Adds the item to tracking
        /// </summary>
        /// <param name="item"></param>
        public double AddTracking(ProductMasterItem item, double current)
        {
            foreach (var trackingShift in Shifts)
            {
                current = trackingShift.AddSummary(item, current);
            }
            return current;
        }
        

        public Dictionary<ProductMasterItem, double> GetCounts()
        {
            Dictionary<ProductMasterItem, double> lastCountDictionary = null;
            var lastShift = Shifts.LastOrDefault();
            if (lastShift != null)
            {
                lastCountDictionary = lastShift.GetCounts();
            }
            return lastCountDictionary;
        }
    }
}
