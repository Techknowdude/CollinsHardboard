using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Configuration_windows;
using ModelLib;
using StaticHelpers;

namespace ProductionScheduler
{
    [Serializable]
    public class PlateConfiguration : ObservableObject
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private List<PressShift> _shifts;
        private int _numAvailablePlates;
        private Dictionary<Texture, int> _plates; 

        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }

            set
            {
                _startTime = value;
                RaisePropertyChangedEvent();
            }
        }
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }

            set
            {
                _endTime = value;
                RaisePropertyChangedEvent();
            }
        }

        public int NumAvailablePlates
        {
            get { return _numAvailablePlates; }
            set { _numAvailablePlates = value; }
        }


        private ObservableCollection<Shift> ProductionShifts { get { return new ObservableCollection<Shift>()
        {
            Shift.ShiftFactory("TestShift",DateTime.Today.AddHours(8),new TimeSpan(8,0,0),DateTime.MinValue,DateTime.MaxValue,null,new List<DayOfWeek>() {DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Friday,DayOfWeek.Sunday}),
            Shift.ShiftFactory("TestShift",DateTime.Today.AddHours(8),new TimeSpan(16,0,0),DateTime.MinValue,DateTime.MaxValue,null,new List<DayOfWeek>() {DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Friday,DayOfWeek.Sunday}),
            Shift.ShiftFactory("TestShift",DateTime.Today.AddHours(8),new TimeSpan(24,0,0),DateTime.MinValue,DateTime.MaxValue,null,new List<DayOfWeek>() {DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Friday,DayOfWeek.Sunday}),
        }; } } // { get { return ShiftHandler.ProductionInstance.Shifts; } } 

        /// <summary>
        /// Constructor for xaml control
        /// </summary>
        public PlateConfiguration()
        {
            _shifts = new List<PressShift>();
            _plates = new Dictionary<Texture, int>();
        }

        public PlateConfiguration(DateTime start, DateTime end)
        {
            _shifts = new List<PressShift>();
            _plates = new Dictionary<Texture, int>();
            Initialize(start,end);
        }

        public void Initialize(DateTime start, DateTime end)
        {
            _startTime = start;
            _endTime = end;
            CreateShifts();
            NumAvailablePlates = PressManager.Instance.NumPlates;
        }

        private void CreateShifts()
        {
            _shifts.Clear();
            

            for(DateTime current = StartTime; current < EndTime; current = current.AddDays(1))
            {
                Shift matchingShift = null;
                foreach (var s in ProductionShifts)
                {
                    if (s.DaysList.Contains(current.DayOfWeek))
                    {
                        if (s.StartDate <= current)
                        {
                            if (s.EndDate >= current)
                            {
                                matchingShift = s;

                                PressShift newShift = new PressShift(new DateTime(current.Year, current.Month, current.Day, matchingShift.StartTime.Hour, matchingShift.StartTime.Minute, matchingShift.StartTime.Second),
                                    matchingShift.Duration,s.Name);
                                _shifts.Add(newShift);
                            }
                        }
                    }
                }
            }
        }

        public bool SetPlates(String tex, int plates)
        {
            //TODO: check if the change is possible
            bool changed = false;
            int current = 0;
            if (_plates.ContainsKey(Texture.GetTexture(tex)))
            {
                current = _plates[Texture.GetTexture(tex)];
            }
            int change = plates - current;

            if (change <= NumAvailablePlates)
            {
                _plates[Texture.GetTexture(tex)] = plates;
                NumAvailablePlates += change;
                changed = true;
            }

            return changed;
        }

        /// <summary>
        /// Try to add the item into the press schedule within this config. 
        /// </summary>
        /// <param name="item">Product Master Item to make</param>
        /// <param name="count">Number of units to make. Altered by the function.</param>
        /// <param name="time">The time the item must be completed.</param>
        /// <returns></returns>
        public bool Add(ProductMasterItem item, ref double count, DateTime time)
        {
            bool added = false;

            for (int index = 0; count > 0 && index < _shifts.Count; index++)
            {
                added = added || _shifts[index].Add(item, ref count);
            }

            return added;
        }

        public bool IsFull()
        {
            return _shifts.All(pressShift => pressShift.IsFull());
        }

        public PressShift GetShift(DateTime start, DateTime end)
        {
            PressShift shift = null;

            foreach (var pressShift in _shifts)
            {
                if (pressShift.EndTime <= end && pressShift.EndTime >= start)
                {
                    shift = pressShift;
                    break;
                }
            }

            return shift;
        }

        public bool Add(ProductMasterItem item, ref double count)
        {
            return true;
        }
    }
}
