using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Input;
using Configuration_windows.Annotations;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace Configuration_windows
{
    /// <summary>
    /// Contains configuration data
    /// </summary>
    [Serializable]
    public class Configuration : INotifyPropertyChanged
    {
        #region Commands

        public ICommand RemoveInputCommand
        {
            get { return new DelegateCommand(RemoveInput); }
        }
        public ICommand RemoveOutputCommand
        {
            get { return new DelegateCommand(RemoveOutput); }
        }
        public ICommand AddNewInputCommand
        {
            get { return new DelegateCommand(AddNewInput); }
        }

        private void AddNewInput()
        {
            InputItems.Add(new ConfigItem());
        }

        public ICommand AddNewOutputCommand
        {
            get { return new DelegateCommand(AddNewOutput); }
        }

        private void AddNewOutput()
        {
            OutputItems.Add(new ConfigItem());
        }

        private void RemoveInput(object obj)
        {
            var item = obj as ConfigItem;
            if (item != null)
            {
                InputItems.Remove(item);
            }
        }

        private void RemoveOutput(object obj)
        {
            var item = obj as ConfigItem;
            if (item != null)
            {
                OutputItems.Remove(item);
            }
        }

        #endregion

        private const string DefaultConfigName = "New Configuration";

        #region Fields
        private String _name = String.Empty;
        private double _piecesOutPerMinute = double.MinValue;

        private ObservableCollection<ConfigItem> _inputItems;
        private ProductMasterItem _itemOut;
        private ObservableCollection<ConfigItem> _outputItems;
        #endregion

        #region Properties
        /// <summary>
        /// Number of conversions this configuration does each hour.
        /// </summary>
        public double PiecesOutPerMinute
        {
            get { return _piecesOutPerMinute; }
            set
            {
                if (_piecesOutPerMinute != value)
                {
                    _piecesOutPerMinute = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Name and description of the configuration
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                // only allow the config name to be set once, as this is how the schedule links to the config
                if (value != _name && _name.Equals(DefaultConfigName) && MachineHandler.Instance.IsConfigNameUnique(value))
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ConfigItem> InputItems
        {
            get { return _inputItems; }
            set
            {
                if (_inputItems != value)
                {
                    _inputItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ConfigItem> OutputItems
        {
            get { return _outputItems; }
            set { _outputItems = value; }
        }

        #endregion

        /// <summary>
        /// Factory for Configuration
        /// </summary>
        /// <returns>A new Configuration</returns>
        public static Configuration CreateConfiguration()
        {
            return new Configuration();
        }

        // For serialization only
        protected Configuration()
        {
            _name = DefaultConfigName;
            InputItems = new ObservableCollection<ConfigItem>();
            OutputItems = new ObservableCollection<ConfigItem>();
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Saves the configuration using the binary writer
        /// </summary>
        public void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream,Name);
            formatter.Serialize(stream,PiecesOutPerMinute);
            formatter.Serialize(stream,InputItems.Count);
            foreach (var configItem in InputItems)
            {
                configItem.Save(stream,formatter);
            }
            formatter.Serialize(stream,OutputItems.Count);
            foreach (var configItem in OutputItems)
            {
                configItem.Save(stream,formatter);
            }
        }

        /// <summary>
        /// Create a new configuration from a file
        /// </summary>
        /// <returns>New Configuration with the data in the input stream</returns>
        public static Configuration Load(Stream stream, IFormatter formatter)
        {
            ObservableCollection<ConfigItem> inItems = new ObservableCollection<ConfigItem>();
            ObservableCollection<ConfigItem> outItems = new ObservableCollection<ConfigItem>();

            string name = (string) formatter.Deserialize(stream);
            double piecesOut = (double) formatter.Deserialize(stream);
            int numInItems = (int)formatter.Deserialize(stream);

            for (int i = 0; i < numInItems; i++)
            {
                inItems.Add(ConfigItem.Load(stream, formatter));
            }
            int numOutItems = (int)formatter.Deserialize(stream);
            for (int i = 0; i < numOutItems; i++)
            {
                outItems.Add(ConfigItem.Load(stream, formatter));
            }

            return new Configuration() {_inputItems = inItems, _outputItems = outItems, Name = name, PiecesOutPerMinute = piecesOut,};
        }

        public bool CanMake(ProductMasterItem item)
        {
            return item != null && OutputItems.Any(output => output.MasterID == item.MasterID);
        }

        public void AddIngredient(ConfigItem make)
        {
            InputItems.Add(make);
        }

        public void AddOutput(int masterID, int pieces)
        {
            OutputItems.Add(new ConfigItem(masterID,pieces));
        }

        public void AddIngredient(int inputID, int inputPieces)
        {
            ConfigItem makeItem = ConfigItem.Create(inputID,inputPieces);
            AddIngredient(makeItem);
        }

        public bool RemoveIngredient(int inputID)
        {
            ConfigItem target = InputItems.FirstOrDefault(item => item.MasterID == inputID);
            return InputItems.Remove(target);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Configuration;
            if (other == null)
            {
                return base.Equals(obj);
            }
            else
            {
                bool inOutsMatch = true;
                // lists are not null and same count
                if (OutputItems != null && other.OutputItems != null)
                {
                    if (OutputItems.Count == other.OutputItems.Count)
                    {
                        for (int index = 0; index < OutputItems.Count && inOutsMatch; index++)
                        {
                            inOutsMatch = OutputItems[index].Equals(other.OutputItems[index]);
                        }
                    }
                    else
                        inOutsMatch = false;
                }
                else if(OutputItems == null || other.OutputItems == null)
                {
                    inOutsMatch = false;
                }

                if(inOutsMatch && InputItems != null && other.InputItems != null)
                {
                    // list have same data
                    if(InputItems.Count == other.InputItems.Count)
                    {
                        for (int index = 0; index < _inputItems.Count && inOutsMatch; index++)
                        {
                            inOutsMatch = _inputItems[index].Equals(other._inputItems[index]);
                        }
                    }
                }
                else if (InputItems == null || other.InputItems == null)
                {
                    inOutsMatch = false;
                }
                return inOutsMatch 
                       && other.Name == Name;
            }
        }

        /// <summary>
        /// Calculates the best configuration between this and the other.
        /// </summary>
        /// <param name="otherConfiguration">Other to compare to</param>
        /// <param name="nextItem">Item to make</param>
        /// <returns>fastest config or null if neither can make</returns>
        public Configuration GetFastestConfig(Configuration otherConfiguration, ProductMasterItem nextItem)
        {
            if (otherConfiguration == null) return this;

            var myOutput = OutputItems.FirstOrDefault(o => o.MasterID == nextItem.MasterID);
            var otherOutput = otherConfiguration.OutputItems.FirstOrDefault(o => o.MasterID == nextItem.MasterID);

            // neither can make
            if (otherOutput == null && myOutput == null) return null;

            // I can't make
            if (myOutput == null) return otherConfiguration;

            // Other can't make
            if (otherOutput == null) return this;

            double myRate = PiecesOutPerMinute;
            double otherRate = otherConfiguration.PiecesOutPerMinute;

            return myRate > otherRate ? this : otherConfiguration;
        }

        /// <summary>
        /// Calculates the hours needed to make an item.
        /// </summary>
        /// <param name="item">Item to make</param>
        /// <param name="units">Units to make</param>
        /// <returns>hours needed or -1 if unable to make</returns>
        public double HoursToMake(ProductMasterItem item, double units)
        {
            return HoursToMake(item.MasterID,item.PiecesPerUnit,units);
        }

        /// <summary>
        /// Calculates the hours needed to make an item.
        /// </summary>
        /// <param name="madeID">Item to make</param>
        /// <param name="madePiecesPerUnit">Pieces per unit to make</param>
        /// <param name="units">Units to make</param>
        /// <returns>hours needed or -1 if unable to make</returns>
        public double HoursToMake(int madeID, double madePiecesPerUnit, double units)
        {
            var output = OutputItems.FirstOrDefault(o => o.MasterID == madeID);
            if (output == null) return -1;

            return ((units * madePiecesPerUnit) * ((_piecesOutPerMinute * output.Pieces) / 60));
        }

        /// <summary>
        /// Calculates the units that can be output for the duration.
        /// </summary>
        /// <param name="item">Item to make</param>
        /// <param name="hours">Time frame</param>
        /// <returns>-1 if the <see cref="Configuration"/> can't make the item.</returns>
        public double UnitsToMakeInHours(ProductMasterItem item, double hours)
        {
            var output = OutputItems.FirstOrDefault(o => o.MasterID == item.MasterID);
            if (output == null) return -1;

            return (output.Pieces*hours*60)/item.PiecesPerUnit;
        }

        /// <summary>
        /// Returns the units consumed to make the item\
        /// </summary>
        /// <param name="consumedItem"></param>
        /// <param name="unitsMade"></param>
        /// <param name="createdItem"></param>
        /// <returns></returns>
        public double GetUnitsConsumed(ProductMasterItem consumedItem, double unitsMade, ProductMasterItem createdItem)
        {
            var createdPieces = unitsMade*createdItem.PiecesPerUnit;
            var consumedInput = _inputItems.FirstOrDefault(input => input.MasterID == consumedItem.MasterID);
            var createdOutput = OutputItems.FirstOrDefault(output => output.MasterID == createdItem.MasterID);
            if (consumedInput != null && createdOutput != null)
            {
                var operations = (createdPieces/createdOutput.Pieces);
                return operations*consumedInput.Pieces/consumedItem.PiecesPerUnit;
            }

            // Item not consumed, or produced
            return 0;
        }

        public double GetUnitsConsumed(ProductMasterItem item, string units, int masterID)
        {
            double consumed = 0;
            double checkedUnits = 0;
            if (double.TryParse(units, out checkedUnits))
            {
                var master = StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == masterID);
                if (master != null)
                {
                    consumed = GetUnitsConsumed(item, checkedUnits, master);
                }
            }

            return consumed;
        }
    }
}
