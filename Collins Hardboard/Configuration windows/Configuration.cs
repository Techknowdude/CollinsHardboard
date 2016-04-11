using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Configuration_windows.Annotations;
using ImportLib;
using ModelLib;

namespace Configuration_windows
{
    /// <summary>
    /// Contains configuration data
    /// </summary>
    public class Configuration : INotifyPropertyChanged
    {
        #region Fields

        private String _name = String.Empty;
        private TimeSpan _changeTime;
        private double _itemsOutPerMinute = double.MinValue;
        private Int32 _itemsIn;
        private Int32 _itemInId = -1;
        private Int32 _itemOutId = -1;
        private Int32 _itemsOut;

        private OnNameChange _nameChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Name and description of the configuration
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                    if (_nameChanged != null)
                    {
                        _nameChanged(value);
                    }
                }
            }
        }

        /// <summary>
        /// The number of pieces that is used in the conversion
        /// </summary>
        public Int32 ItemsIn
        {
            get { return _itemsIn; }
            set
            {
                if (value != _itemsIn)
                {
                    _itemsIn = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The product master ID for the input items
        /// </summary>
        public Int32 ItemOutID
        {
            get { return _itemOutId; }
            
            set
            {
                if (value != _itemOutId)
                {
                    _itemOutId = value;
                    ItemOut = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == value);
                    OnPropertyChanged();                
                }
            }
        }

        /// <summary>
        /// The number of pieces that is used in the conversion
        /// </summary>
        public Int32 ItemsOut
        {
            get { return _itemsOut; }
            set
            {
                if (value != _itemsOut)
                {
                    _itemsOut = value;
                    OnPropertyChanged();
                }
            }
          
        }

        /// <summary>
        /// The product master ID for the input items
        /// </summary>
        public Int32 ItemInID
        {
            get { return _itemInId; }
            set
            {
                if (value != _itemInId)
                {
                    _itemInId = value;
                    ItemIn = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Reference to the master item for the input
        /// </summary>
        public ProductMasterItem ItemIn { get; set; }

        /// <summary>
        /// Reference to the master item for the input
        /// </summary>
        public ProductMasterItem ItemOut { get; set; }

        /// <summary>
        /// The time required to setup this configuration
        /// </summary>
        public TimeSpan ChangeTime
        {
            get { return _changeTime; }
            set
            {
                if (value != _changeTime)
                {
                    _changeTime = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of convertions this configuration does each hour.
        /// </summary>
        public double ItemsOutPerMinute
        {
            get { return _itemsOutPerMinute; }
            set
            {
                if (_itemsOutPerMinute != value)
                {
                    _itemsOutPerMinute = value;
                    OnPropertyChanged();
                }
            }
        }

        public OnNameChange NameChanged
        {
            get { return _nameChanged; }
            set { _nameChanged = value; }
        }

        #endregion

        /// <summary>
        /// Factory for Configuration
        /// </summary>
        /// <returns>A new Configuration</returns>
        public static Configuration CreateConfiguration(String name = default(String), Int32 inputID = default(int),
            Int32 inputPieces = 1, Int32 outputID = default(int),
            Int32 outputPieces = default(int), double convertionsPerMin = default(double),
            TimeSpan changeTime = default(TimeSpan))
        {
            return new Configuration(name,inputID,inputPieces,outputID,outputPieces,convertionsPerMin,changeTime);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        Configuration(String name = default(String), Int32 inputID = default(int), 
            Int32 inputPieces = default(int), Int32 outputID = default(int), 
            Int32 outputPieces = default(int), double itemsOutPerMinute = default(double), 
            TimeSpan changeTime = default(TimeSpan))
        {
            Name = name;
            ItemsIn = inputPieces;
            ItemInID = inputID;
            ItemsOut = outputPieces;
            ItemOutID = outputID;
            ItemsOutPerMinute = itemsOutPerMinute;
            ChangeTime = changeTime;
        }

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
        /// <param name="writer">BinaryWriter to write to</param>
        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(ItemsIn);
            writer.Write(ItemInID);
            writer.Write(ItemsOut);
            writer.Write(ItemOutID);
            writer.Write(ItemsOutPerMinute);
            writer.Write(ChangeTime.Hours);
            writer.Write(ChangeTime.Minutes);
            writer.Write(ChangeTime.Seconds);
        }

        /// <summary>
        /// Create a new configuration from a file
        /// </summary>
        /// <param name="reader">input stream containing the configuration data.</param>
        /// <returns>New Configuration with the data in the input stream</returns>
        public static Configuration CreateConfiguration(BinaryReader reader)
        {
            String Name = reader.ReadString();
            Int32 itemsIn = reader.ReadInt32();
            Int32 itemInID = reader.ReadInt32();
            Int32 itemsOut = reader.ReadInt32();
            Int32 itemOutID = reader.ReadInt32();
            double conversions = reader.ReadDouble();
            Int32 hours = reader.ReadInt32();
            Int32 minutes = reader.ReadInt32();
            Int32 seconds = reader.ReadInt32();
            TimeSpan changeTime = new TimeSpan(hours,minutes,seconds);

            return new Configuration(Name,itemInID,itemsIn,itemOutID,itemsOut,conversions,changeTime);
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
                return other.ChangeTime == ChangeTime && other.ItemIn == ItemIn && other.ItemInID == ItemInID &&
                       other.ItemOut == ItemOut
                       && other.ItemOutID == ItemOutID && other.ItemsIn == ItemsIn && other.ItemsOut == ItemsOut &&
                       other.ItemsOutPerMinute == ItemsOutPerMinute
                       && other.Name == Name;
            }
        }
    }
}
