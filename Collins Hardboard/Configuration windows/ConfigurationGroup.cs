using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Configuration_windows.Annotations;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace Configuration_windows
{
    [Serializable]
    public class ConfigurationGroup : INotifyPropertyChanged
    {
        
        #region INotifyPropertyChanged implementation
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }

        private TimeSpan changeTime;
        private string name;
        private ObservableCollection<Configuration> configurations;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Configuration> Configurations
        {
            get
            {
                return configurations;
            }

            set
            {
                configurations = value;
                OnPropertyChanged();
            }
        }

        public ConfigurationGroup(string name, ObservableCollection<Configuration> configurations, TimeSpan changeTimeSpan)
        {
            ChangeTime = changeTimeSpan;
            Name = name;
            Configurations = configurations ?? new ObservableCollection<Configuration>();
        }
        
        public ConfigurationGroup() : this(String.Empty, new ObservableCollection<Configuration>(), TimeSpan.Zero)
        {
            
        }

        public void AddConfig(Configuration config)
        {
            Configurations.Add(config);
        }

        public void RemoveConfig(Configuration config)
        {
            var index = Configurations.IndexOf(config);
            RemoveConfig(index);
        }

        public void RemoveConfig(int index)
        {
            if(index < 0 || index >= Configurations.Count)
                throw new ArgumentOutOfRangeException("index","ConfigurationGroup::RemoveConfig index of " + index + " is out of range. Current count is: " + Configurations.Count);

            Configurations.RemoveAt(index);
        }

        #region Serialization
        public void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream,changeTime);
            formatter.Serialize(stream,name);
            formatter.Serialize(stream,configurations.Count);
            foreach (var configuration in configurations)
            {
                configuration.Save(stream,formatter);       
            }
        }

        public static ConfigurationGroup Load(Stream stream, BinaryFormatter formatter)
        {
            TimeSpan changeTime = (TimeSpan) formatter.Deserialize(stream);
            string name = (string) formatter.Deserialize(stream);
            int numConfig = (int)formatter.Deserialize(stream);
            ObservableCollection<Configuration> configs = new ObservableCollection<Configuration>();
            for (int i = 0; i < numConfig; i++)
            {
                configs.Add(Configuration.Load(stream,formatter));
            }

            return new ConfigurationGroup(name,configs,changeTime);   
        }
        #endregion

        public static ConfigurationGroup Create(string newConfigurationGroup)
        {
            return new ConfigurationGroup(newConfigurationGroup, null,TimeSpan.Zero);
        }

        public bool CanMake(ProductMasterItem item)
        {
            bool canMake = false;

            foreach (var configuration in Configurations)
            {
                if (configuration.CanMake(item))
                {
                    canMake = true;
                    break;
                }
            }
            return canMake;
        }

        public bool CanMake(int masterID)
        {
            ProductMasterItem item =
                StaticInventoryTracker.ProductMasterList.FirstOrDefault(prod => prod.MasterID == masterID);
            if (item != null)
            {
                return CanMake(item);
            }
            return false;
        }

        public ICommand RemoveConfigCommand
        {
            get { return new DelegateCommand(RemoveConfig);}
        }
        public ICommand AddConfigCommand
        {
            get { return new DelegateCommand(AddNewConfig); }
        }

        private void AddNewConfig(object obj)
        {
            Configuration newConfig = obj as Configuration ?? Configuration.CreateConfiguration();

            Configurations.Add(newConfig);
            MachineHandler.Instance.RefreshConfigurations();
        }


        // Public Property - XmlIgnore as it doesn't serialize anyway
        [XmlIgnore]
        public TimeSpan ChangeTime
        {
            get
            {
                return changeTime;
            }

            set
            {
                changeTime = value;
            }
        }

        /// Pretend property for serialization. Do not use.
        [Browsable(false)]
        [XmlElement("ChangeTime")]
        public long ChangeTimeTicks
        {
            get { return changeTime.Ticks; }
            set { changeTime = new TimeSpan(value); }
        }

        public void RemoveConfig(object obj)
        {
            var config = obj as Configuration;
            if (config != null)
            {
                var result = MessageBox.Show("Are you sure you want to remove this configuration?", "",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Configurations.Remove(config);
                    MachineHandler.Instance.RefreshConfigurations();
                }
            }
        }

    }
}
