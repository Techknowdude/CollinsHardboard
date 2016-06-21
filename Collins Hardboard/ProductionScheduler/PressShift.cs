using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ProductionScheduler
{
    [Serializable]
    public class PressShift : ObservableObject
    {
        private DateTime _startTime;
        private TimeSpan _duration;
        private List<PressMasterItem> _produced;
        private string _name;

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
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChangedEvent();
            }
        }
        public TimeSpan Duration
        {
            get
            {
                return _duration;
            }

            set
            {
                _duration = value;
                RaisePropertyChangedEvent();
            }
        }
        public List<PressMasterItem> Produced
        {
            get { return _produced; }
            set
            {
                _produced = value;
                RaisePropertyChangedEvent();
            }
        }
        public DateTime EndTime { get { return StartTime + Duration; } }

        public List<ProductMasterItem> PressItems { get
        {
            return StaticInventoryTracker.ProductMasterList.Where(x => x.MadeIn == "Press").ToList();
        } } 

        public PressShift(DateTime start = default(DateTime), TimeSpan duration = default(TimeSpan), string name = "")
        {
            _produced = new List<PressMasterItem>();
            StartTime = start;
            Duration = duration;
            Name = name;
        }


        public bool Full { get { return GetAvailableHours() <= 0; } }

        public ICommand AddCommand
        {
            get { return new DelegateCommand(() =>
            {
                double zero = 0;
                if (SelectedItem != null)
                    Add(SelectedItem, ref zero);
            }); }
        }

        public ProductMasterItem SelectedItem { get; set; }

        public bool IsFull()
        {
            return GetAvailableHours() <= 0;
        }

        public bool Add(ProductMasterItem item, ref double count)
        {
            bool added = false;
            double numToAdd = count;
            if (!IsFull())
            {
                // get the number that can be added.
                double hoursAvailable = GetAvailableHours();
                double maxUnitsToMake = hoursAvailable*item.UnitsPerHour;

                // clamp
                numToAdd = numToAdd > maxUnitsToMake ? maxUnitsToMake : numToAdd;

                PressMasterItem pressItem = Produced.FirstOrDefault(p => p.MasterItem == item);

                if (pressItem == null)
                {
                    Produced.Add(new PressMasterItem(item,count));
                }
                else
                {
                    pressItem.UnitsMade += numToAdd;
                }
                RaisePropertyChangedEvent("Produced");
                count -= numToAdd;
                added = true;
            }

            return added;
        }

        public void Remove(ProductMasterItem item)
        {
            PressMasterItem pressItem = Produced.FirstOrDefault(p => p.MasterItem == item);

            if (pressItem != null)
            {
                Produced.Remove(pressItem);
                RaisePropertyChangedEvent("Produced");
            }
        }

        double GetAvailableHours()
        {
            double totalHours = Duration.Hours + Duration.Minutes/(double)60 + Duration.Seconds/(double)(60*60);

            foreach (var itemPair in Produced)
            {
                var item = itemPair.MasterItem;
                var units = itemPair.UnitsMade;
                totalHours -= units/item.UnitsPerHour;
            }

            return totalHours;
        }
    }
}
