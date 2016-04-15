
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Configuration_windows.Annotations;
using ModelLib;

namespace Configuration_windows
{
    /// <summary>
    /// Represents a physical machine in the production line.
    /// </summary>
    public sealed class Machine : INotifyPropertyChanged
    {
        #region Fields

        private String _name;
        private ObservableCollection<Configuration> _configurationList;
        private ObservableCollection<string> _linesCanRunOn;
        private ObservableCollection<string> _configurationListNames;
        private ObservableCollection<string> _lineConflicts;
        private ObservableCollection<string> _machineConflicts;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the machine
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// List of configurations the machine may use
        /// </summary>
        public ObservableCollection<Configuration> ConfigurationList
        {
            get { return _configurationList; }
            set
            {
                if (_configurationList != value)
                {
                    _configurationList = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// List of the names of all configurations
        /// </summary>
        public ObservableCollection<String> ConfigurationListNames
        {
            get { return _configurationListNames; }
            set
            {
                if (value != _configurationListNames)
                {
                    _configurationListNames = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<String> MachineConflicts
        {
            get { return _machineConflicts; }
            set { _machineConflicts = value; }
        }

        public ObservableCollection<String> LineConflicts
        {
            get { return _lineConflicts; }
            set { _lineConflicts = value; }
        }

        /// <summary>
        /// List of lines the machine can be run on
        /// </summary>
        public ObservableCollection<string> LinesCanRunOn
        {
            get { return _linesCanRunOn; }
            set
            {
                if (value != _linesCanRunOn)
                {
                    _linesCanRunOn = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        /// <summary>
        /// Factory pattern for the machines
        /// </summary>
        /// <param name="name">Name of the machine</param>
        /// <param name="configurationList">List of configurations the machine may have</param>
        /// <param name="linesCanRunOn">List of lines the machine is run on</param>
        /// <returns>New machine containing passed data</returns>
        public static Machine CreateMachine(string name = default (String), 
            ObservableCollection<Configuration> configurationList = default (ObservableCollection<Configuration>), 
            ObservableCollection<string> linesCanRunOn = default(ObservableCollection<String>), 
            ObservableCollection<String> machineConflicts = default(ObservableCollection<String>), 
            ObservableCollection<String> lineConflict = default(ObservableCollection<String>) )
        {
            return new Machine(name,configurationList,linesCanRunOn,machineConflicts,lineConflict);
        }

        /// <summary>
        /// Constructor for machines
        /// </summary>
        /// <param name="name">Name of the machine</param>
        /// <param name="configurationList">List of configurations the machine may have</param>
        /// <param name="linesCanRunOn">List of lines the machine is run on</param>
        Machine(string name, ObservableCollection<Configuration> configurationList, ObservableCollection<string> linesCanRunOn,
            ObservableCollection<string> machineConflicts,ObservableCollection<string> lineConflicts )
        {
            _name = name;
            
            _configurationList = configurationList ?? new ObservableCollection<Configuration>();
            _configurationListNames = new ObservableCollection<string>(_configurationList.Select(x => x.Name));
            
            _linesCanRunOn = linesCanRunOn ?? new ObservableCollection<string>();
            _machineConflicts = machineConflicts ?? new ObservableCollection<String>();
            _lineConflicts = lineConflicts ?? new ObservableCollection<string>();
        }

        /// <summary>
        /// Adds the passed configuration to the list and updates the list of names.
        /// </summary>
        /// <param name="newConfig">Configuration to add</param>
        public void AddConfiguration(Configuration newConfig)
        {
            if (newConfig != null)
            {
                _configurationList.Add(newConfig);
                _configurationListNames.Add(newConfig.Name);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Removes the selected configuration from the list as well as the name from the names list
        /// </summary>
        /// <param name="index">Zero based index of the configuration to remove</param>
        public void RemoveConfiguration(Int32 index)
        {
            if (index >= 0 && index < _configurationList.Count)
            {
                var result = MessageBox.Show("Are you sure you want to remove this configuration?", "",
                    MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    _configurationList.RemoveAt(index);
                    _configurationListNames.RemoveAt(index);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Removes the matching configuration from the list as well as the name fro mthe name list
        /// </summary>
        /// <param name="config">Configuration to remove</param>
        public void RemoveConfiguration(Configuration config)
        {
            if (config != null)
            {
                Int32 index = _configurationList.IndexOf(config);
                if (index != -1) // if found
                {
                    RemoveConfiguration(index);
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(ConfigurationList.Count);
            foreach (Configuration configuration in ConfigurationList)
            {
                configuration.Save(writer);
            }
            writer.Write(LinesCanRunOn.Count);
            foreach (string s in LinesCanRunOn)
            {
                writer.Write(s);
            }
            writer.Write(LineConflicts.Count);
            foreach (var lineConflict in LineConflicts)
            {
                writer.Write(lineConflict);
            }
            writer.Write(MachineConflicts.Count);
            foreach (var machineConflict in MachineConflicts)
            {
                writer.Write(machineConflict);
            }
        }

        public static Machine CreateMachine(BinaryReader reader)
        {
            ObservableCollection<Configuration> configurationList = new ObservableCollection<Configuration>();
            ObservableCollection<string> linesCanRunOn = new ObservableCollection<string>();
            ObservableCollection<String> lineConflicts = new ObservableCollection<string>();
            ObservableCollection<String> machineConflicts = new ObservableCollection<string>();

            String name = reader.ReadString();
            Int32 num;

            num = reader.ReadInt32(); // configs
            for(; num > 0; --num)
            {
                //attempt to load reference the needed config from the config list.
                var config = Configuration.CreateConfiguration(reader);
                var match = ConfigurationsHandler.GetInstance()
                    .Configurations.FirstOrDefault(x => x.Name == config.Name);

                if (match != null)
                    config = match;

                configurationList.Add( config);
            }
            num = reader.ReadInt32(); // lines to run on
            for (; num > 0; --num)
            {
                linesCanRunOn.Add(reader.ReadString());
            }

            num = reader.ReadInt32(); // line conflicts
            for (; num > 0; --num)
            {
                lineConflicts.Add(reader.ReadString());
            }

            num = reader.ReadInt32(); //machine conflicts
            for (; num > 0; --num)
            {
                machineConflicts.Add(reader.ReadString());
            }

            return CreateMachine(name,configurationList,linesCanRunOn,machineConflicts,lineConflicts);
        }

        public override string ToString()
        {
            return Name;
        }

        public Configuration GetBestConfig(ProductMasterItem nextItem)
        {
            var configs = ConfigurationList.Where(conf => conf.ItemOutID == nextItem.MasterID);
            Configuration config = configs.FirstOrDefault();

            foreach (var configuration in configs)
            {
                if (configuration.ItemsOutPerMinute > config.ItemsOutPerMinute)
                    config = configuration;
            }
            
            return config;
        }

        public void CheckConfigRefs()
        {
            for (int index = 0; index < ConfigurationList.Count; index++)
            {
                var configuration = ConfigurationList[index];
                var match = ConfigurationsHandler.GetInstance()
                    .Configurations.FirstOrDefault(x => x.Name == configuration.Name);

                if (match != null)
                    ConfigurationList[index] = match;
            }
        }
    }
}
