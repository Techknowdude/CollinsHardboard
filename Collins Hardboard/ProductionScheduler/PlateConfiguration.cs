using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Configuration_windows;
using ImportLib;
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
        private ObservableCollection<PlateCount> _plates; 

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
            set
            {
                _numAvailablePlates = value; 
                RaisePropertyChangedEvent();
            }
        }

        private ObservableCollection<Shift> ProductionShifts { get { return ShiftHandler.ProductionInstance.Shifts; } }

        public List<PressShift> Shifts
        {
            get { return _shifts; }
            set { _shifts = value; }
        }

        public ICommand RemoveCommand { get { return new DelegateCommand(Remove);} }

        public ObservableCollection<PlateCount> Plates
        {
            get { return _plates; }
            set { _plates = value; }
        }

        public ICommand AddPlateCommand
        {
            get { return new DelegateCommand(AddPlate);}
        }

        public ICommand DeletePlateCommand
        {
            get { return new DelegateCommand(DeletePlate); }
        }

        private void DeletePlate(object obj)
        {
            PlateCount pc = obj as PlateCount;
            Plates.Remove(pc);
            UpdateAvailablePlates();
        }

        private void AddPlate()
        {
            Plates.Add(new PlateCount(Texture.GetDefault(),0,PlateChanged));
        }

        void UpdateAvailablePlates()
        {
            int availablePlates = PressManager.NumPlates;
            foreach (var plateCount in Plates)
            {
                availablePlates -= plateCount.Count;
            }

            NumAvailablePlates = availablePlates < 0 ? 0 : availablePlates;
        }

        private void PlateChanged(object sender, PropertyChangedEventArgs e)
        {
            PlateCount pc = sender as PlateCount;

            UpdateAvailablePlates();
            int availablePlates = NumAvailablePlates;

            //prevent too many plates
            if (availablePlates < 0)
            {
                if (pc != null)
                    pc.Count += availablePlates;
                NumAvailablePlates = 0;
            }
            else
            {
                NumAvailablePlates = availablePlates;
            }
        }

        public void Remove()
        {
            PressManager.Instance.Remove(this);
        }


        /// <summary>
        /// Constructor for xaml control
        /// </summary>
        public PlateConfiguration()
        {
            _shifts = new List<PressShift>();
            _plates = new ObservableCollection<PlateCount>();
        }

        public PlateConfiguration(DateTime start, DateTime end)
        {
            _shifts = new List<PressShift>();
            _plates = new ObservableCollection<PlateCount>();
            Initialize(start,end);
        }

        public void Initialize(DateTime start, DateTime end)
        {
            _startTime = start;
            _endTime = end;
            CreateShifts();
            NumAvailablePlates = PressManager.NumPlates;
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
            bool changed = false;
            int current = 0;
            var plate = _plates.FirstOrDefault(t => t.Tex == Texture.GetTexture(tex));
            if (plate == null)
            {
                plate = new PlateCount(Texture.GetTexture(tex), plates,PlateChanged);
                _plates.Add(plate);
                changed = true;
            }
            else
            {
                bool changePossible = true;

                current = plate.Count;

                // check usage if lowering
                int change = plates - current;

                if (change <= NumAvailablePlates)
                {
                    if (change > 0) // Adding plates, no need to check if this effects production
                    {
                        plate.Count = plates;
                        NumAvailablePlates += change;
                        changed = true;
                    }
                    else
                    {
                        foreach (var pressShift in Shifts)
                        {
                            var potentialOutputUnits = 30; // normally 30 units of output, but depends on the thickness in the press

                            var actualOutput =
                                pressShift.Produced.Where(m => m.MasterItem.Texture.Contains(tex))
                                    .Sum(i => i.UnitsMade*i.MasterItem.PiecesPerUnit);
                            if (actualOutput > potentialOutputUnits)
                            {
                                changePossible = false;
                                break;
                            }
                        }

                        if (changePossible)
                        {
                            plate.Count = plates;
                            NumAvailablePlates += change;
                            changed = true;
                        }
                    }
                }
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
        public bool Add(ProductMasterItem item, ref double count, ref DateTime time)
        {
            bool added = false;
            //double scheduledCount = count;
            bool undo = false;
            DateTime currentTime = DateTime.MinValue;

            // Add the items to shifts until either all items are made, or the time has past
            for (int index = 0; count > 0 && index < _shifts.Count; index++)
            {
                currentTime = _shifts[index].EndTime + PressManager.Instance.DelayTime;

                //if (currentTime > time)
                //{
                //    //undo = true;
                //    break;
                //}

                added = _shifts[index].Add(item, ref count) || added;
            }

            //// if time has past or not all items could be made
            //if (undo || count > 0)
            //{
            //    scheduledCount = scheduledCount - count;

            //    for (int index = _shifts.Count -1; index >= 0 && scheduledCount > 0; --index)
            //    {
            //        var shift = _shifts[index];
            //        shift.Remove(item, ref scheduledCount);
            //    }
            //    added = false; // did not completed
            //}

            time = currentTime;

            return added;
        }


        /// <summary>
        /// Try to add the item into the press schedule within this config. 
        /// </summary>
        /// <param name="item">Product Master Item to make</param>
        /// <param name="count">Number of units to make. Altered by the function.</param>
        /// <param name="time">The time the item must be completed.</param>
        /// <returns></returns>
        public bool AddSale(ProductMasterItem item, ref double count, ref DateTime time)
        {
            DateTime currentTime = DateTime.MinValue;

            // Add the items to shifts until either all items are made, or the time has past
            for (int index = 0; count > 0 && index < _shifts.Count; index++)
            {
                currentTime = _shifts[index].EndTime + PressManager.Instance.DelayTime;
                _shifts[index].Add(item, ref count);
            }

            var byTime = time;
            time = currentTime;

            return currentTime < byTime;
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
                if (pressShift.EndTime + PressManager.Instance.DelayTime <= end && pressShift.EndTime + PressManager.Instance.DelayTime > start)
                {
                    shift = pressShift;
                    break;
                }
            }

            return shift;
        }

        public List<PressShift> GetShiftsInRange(DateTime start, DateTime end)
        {
            List<PressShift> shifts = new List<PressShift>();

            foreach (var pressShift in _shifts)
            {
                if (pressShift.StartTime + PressManager.Instance.DelayTime <= end &&
                    pressShift.EndTime + PressManager.Instance.DelayTime >= start)
                {
                    shifts.Add(pressShift);
                }
            }

            return shifts;
        }
    }
}
