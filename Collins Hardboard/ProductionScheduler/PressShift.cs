using System;
using System.Collections.ObjectModel;
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
        private ObservableCollection<PressMasterItem> _produced;
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
        public ObservableCollection<PressMasterItem> Produced
        {
            get { return _produced; }
            set
            {
                _produced = value;
                RaisePropertyChangedEvent();
            }
        }
        public DateTime EndTime { get { return StartTime + Duration; } }
        public ObservableCollection<ProductMasterItem> PressItems => StaticInventoryTracker.PressMasterList; 


        public PressShift(DateTime start = default(DateTime), TimeSpan duration = default(TimeSpan), string name = "")
        {
            _produced = new ObservableCollection<PressMasterItem>();
            StartTime = start;
            Duration = duration;
            Name = name;
        }


        public bool Full { get { return GetAvailableHours() <= 0; } }

        public ICommand AddCommand
        {
            get { return new DelegateCommand(Add); }
        }

        private void Add()
        {
            double zero = 0;
            Add(SelectedItem, ref zero);
        }

        public ProductMasterItem SelectedItem { get; set; }

        public ICommand RemoveCommand
        {
            get { return new DelegateCommand(Remove); }
        }

        private void Remove(object arg)
        {
            PressMasterItem item = arg as PressMasterItem;
            if (item != null)
            {
                _produced.Remove(item);
            }
        }

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

        public void Remove(ProductMasterItem item, ref double scheduledCount)
        {
            PressMasterItem pressItem = Produced.FirstOrDefault(p => p.MasterItem.Equals( item ));

            if (pressItem != null)
            {
                if (scheduledCount >= pressItem.UnitsMade)
                {
                    Produced.Remove(pressItem);
                }

                scheduledCount -= pressItem.UnitsMade;
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

        public void AddProductionToInventory()
        {
            foreach (var pressMasterItem in Produced)
            {
                double units = pressMasterItem.UnitsMade;
                ProductMasterItem item = pressMasterItem.MasterItem;

                InventoryItem inventoryItem =
                    StaticInventoryTracker.AllInventoryItems.FirstOrDefault(x => x.MasterID == item.MasterID && x.Grade == "WiP");
                if (inventoryItem != null)
                {
                    inventoryItem.AddUnits(units);
                }
                else
                {
                    StaticInventoryTracker.AddInventory(item.ProductionCode, item.PiecesPerUnit, units, "WiP",
                        item.MasterID);
                }
            }
        }
    }
}
