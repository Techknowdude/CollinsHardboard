using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Configuration_windows.Annotations;
using ImportLib;
using ModelLib;

namespace Configuration_windows
{

    [Serializable]
    public class ConfigItem : INotifyPropertyChanged
    {
        // For serialization
        public ConfigItem() : this(-1,0)
        {
            
        }

        private int _masterID;
        private double _pieces;

        public ProductMasterItem MasterItem 
        {
            get
            {
                return StaticInventoryTracker.ProductMasterList.FirstOrDefault(master => master.MasterID == MasterID);
            }
        }

        public int MasterID
        {
            get { return _masterID; }
            set
            {
                if (_masterID != value)
                {
                    _masterID = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MasterItem));
                }
            }
        }

        public double Pieces
        {
            get { return _pieces; }
            set
            {
                if (_pieces != value)
                {
                    _pieces = value;
                    OnPropertyChanged();
                }
            }
        }

        public ConfigItem(int id, int pieces)
        {
            MasterID = id;
            Pieces = pieces;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream, this);
        }

        public static ConfigItem Load(Stream stream, IFormatter formatter)
        {
            return (ConfigItem)formatter.Deserialize(stream);
        }

        public static ConfigItem Create(int itemId, int pieces)
        {
            return new ConfigItem(itemId,pieces);
        }
    }
}
