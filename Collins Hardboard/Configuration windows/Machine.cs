
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using Configuration_windows.Annotations;
using ModelLib;
using StaticHelpers;

namespace Configuration_windows
{
    /// <summary>
    /// Represents a physical machine in the production line.
    /// </summary>
    [Serializable]
    public class Machine : INotifyPropertyChanged
    {
        #region Fields

        private String _name;
        private ObservableCollection<ConfigurationGroup> _configurationList;
        private ObservableCollection<string> _linesCanRunOn;
        private ObservableCollection<string> _lineConflicts;
        private ObservableCollection<string> _machineConflicts;
        #endregion

        #region Properties

        protected Machine()
        {
            
        }

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
                    MachineHandler.Instance.UpdateName(this, value);
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// List of configurations the machine may use
        /// </summary>
        public ObservableCollection<ConfigurationGroup> ConfigurationList
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


        #region ICommands for GUI

        public ICommand RemoveMachineConflictCommand
        {
            get { return new DelegateCommand(RemoveMachineConflict); }
        }


        public ICommand AddMachineConflictCommand
        {
           get { return new DelegateCommand(AddMachineConflict);}
        }


        public ICommand AddExistingCommand
        {
            get { return new DelegateCommand(AddExistingConfigGroup);}
        }

        public ICommand AddLineRunCommand
        {
            get { return new DelegateCommand(AddRunOnLine); }
        }
        public ICommand RemoveLineRunCommand
        {
            get { return new DelegateCommand(RemoveRunOnLine); }
        }

        public ICommand AddLineConflictCommand
        {
            get { return new DelegateCommand(AddLineConflict); }
        }
        public ICommand RemoveLineConflictCommand
        {
            get { return new DelegateCommand(RemoveLineConflict); }
        }
        
        public ICommand RemoveConfigGroupCommand
        {
            get { return new DelegateCommand(RemoveConfigGroup); }
        }

        public ICommand AddNewGroupCommand
        {
            get { return new DelegateCommand(AddNewGroup);}
        }

        private void AddNewGroup()
        {
            ConfigurationList.Add(new ConfigurationGroup("New Group",null, TimeSpan.Zero));
        }

        private void RemoveConfigGroup(object obj)
        {
            ConfigurationGroup configGroup = obj as ConfigurationGroup;
            if (configGroup != null)
            {
                var result = MessageBox.Show("Are you sure you want to remove this Configuration Group?", "",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    ConfigurationList.Remove(configGroup);
                }
            }
        }

        #endregion

        #region CommandDelegates
        private void RemoveMachineConflict(object obj)
        {
            string machine = obj as string;
            if (machine != null)
            {
                MachineConflicts.Remove(machine);
            }
        }

        private void AddMachineConflict(object obj)
        {
            var machine = obj as Machine;
            if (machine != null && !MachineConflicts.Contains(machine.Name))
            {
                MachineConflicts.Add(machine.Name);
            }
        }

        private void RemoveLineConflict(object obj)
        {
            string line = obj as string;
            if (line != null)
            {
                LineConflicts.Remove(line);
            }
        }

        private void AddLineConflict(object obj)
        {
            string line = obj as string;
            if (line != null && !LineConflicts.Contains(line))
            {
                LineConflicts.Add(line);
            }
        }

        private void RemoveRunOnLine(object obj)
        {
            string line = obj as string;
            if (line != null)
            {
                LinesCanRunOn.Remove(line);
            }
        }

        private void AddRunOnLine(object obj)
        {
            string line = obj as string;
            if (line != null && !LinesCanRunOn.Contains(line))
            {
                LinesCanRunOn.Add(line);
            }
        }

        private void AddExistingConfigGroup(object obj)
        {
            if (obj != null)
            {
                ConfigurationGroup group = obj as ConfigurationGroup;
                
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// Factory pattern for the machines
        /// </summary>
        /// <param name="name">Name of the machine</param>
        /// <param name="configurationList">List of configurations the machine may have</param>
        /// <param name="linesCanRunOn">List of lines the machine is run on</param>
        /// <returns>New machine containing passed data</returns>
        public static Machine CreateMachine(string name = default (String), 
            ObservableCollection<ConfigurationGroup> configurationList = default (ObservableCollection<ConfigurationGroup>), 
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
        public Machine(string name, ObservableCollection<ConfigurationGroup> configurationList, ObservableCollection<string> linesCanRunOn,
            ObservableCollection<string> machineConflicts,ObservableCollection<string> lineConflicts )
        {
            _name = name;
            
            _configurationList = configurationList ?? new ObservableCollection<ConfigurationGroup>();
            
            _linesCanRunOn = linesCanRunOn ?? new ObservableCollection<string>();
            _machineConflicts = machineConflicts ?? new ObservableCollection<String>();
            _lineConflicts = lineConflicts ?? new ObservableCollection<string>();
        }

        /// <summary>
        /// Adds the passed configuration to the list and updates the list of names.
        /// </summary>
        /// <param name="newConfig">Configuration to add</param>
        public void AddConfiguration(ConfigurationGroup newConfig)
        {
            if (newConfig != null)
            {
                _configurationList.Add(newConfig);
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
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _configurationList.RemoveAt(index);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Removes the matching configuration from the list as well as the name fro mthe name list
        /// </summary>
        /// <param name="config">Configuration to remove</param>
        public void RemoveConfiguration(ConfigurationGroup config)
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

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream, MachineConflicts.ToList());
            formatter.Serialize(stream, LinesCanRunOn.ToList());
            formatter.Serialize(stream, LineConflicts.ToList());
            formatter.Serialize(stream, Name);
            formatter.Serialize(stream, ConfigurationList.Count);
            foreach (var configurationGroup in ConfigurationList)
            {
                configurationGroup.Save(stream,formatter);
            }

        }

        public static Machine Load(Stream stream, IFormatter formatter)
        {
            ObservableCollection<string> machineConflicts = new ObservableCollection<string>();
            ObservableCollection<string> lineConflicts = new ObservableCollection<string>();
            ObservableCollection<string> linesRun = new ObservableCollection<string>();
            string name;
            ObservableCollection<ConfigurationGroup> configurationGroups = new ObservableCollection<ConfigurationGroup>();
            foreach (var mconf in ((List<string>)formatter.Deserialize(stream)))
            {
                machineConflicts.Add(mconf);
            }

            return (Machine) formatter.Deserialize(stream);
        }

        public override string ToString()
        {
            return Name;
        }

        public Configuration GetBestConfig(ProductMasterItem nextItem)
        {
            //TODO: fix tis
            //var configs = ConfigurationList.Where(conf => conf.ItemOutID == nextItem.MasterID);
            //Configuration config = configs.FirstOrDefault();

            //foreach (var configuration in configs)
            //{
            //    config = config.GetFastestConfig(configuration, nextItem);
            //}

            return null;//config;
        }

        public void CheckConfigRefs()
        {
            List<ConfigurationGroup> configs = new List<ConfigurationGroup>();
            configs.AddRange(ConfigurationList);

            foreach (var configuration in configs)
            {
                //TODO Fix this
                //var match = ConfigurationsHandler.GetInstance()
                //    .Configurations.FirstOrDefault(x => x.ItemOutID == configuration.ItemOutID);

                //if (match != null)
                //{
                //    int index = ConfigurationList.IndexOf(configuration);
                //    ConfigurationList[index] = match;
                //    // update name if needed.
                //    ConfigurationListNames[index] = match.Name;
                //}
                //else
                //{
                //    ConfigurationList.Remove(configuration);
                //}
            }
        }
    }
}
