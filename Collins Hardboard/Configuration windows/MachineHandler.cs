using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;
using Configuration_windows.Annotations;
using Microsoft.Win32;
using ModelLib;

namespace Configuration_windows
{
    public class MachineHandler : INotifyPropertyChanged
    {
        #region Fields
        ObservableCollection<Configuration> _allConfigurations;

        private bool _hasLoaded = false;
        private static bool _instanceLoaded = false;
        [NonSerialized]
        private static MachineHandler _inner;
        private ObservableCollection<Machine> _machineList;
        [NonSerialized]
        private MachineConfigWindow _configWindow;
        [NonSerialized]
        private string _saveName = @"machines.dat";
        [NonSerialized]
        XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<Machine>));
        #endregion

        #region Properties

        /// <summary>
        /// Flag for if the machine handler has loaded a config file into memory
        /// </summary>
        public bool IsLoaded
        {
            get { return _hasLoaded && AllConfigurations.Count > 0; }
        }

        /// <summary>
        /// The file that holds the machine handler information
        /// </summary>
        public string SaveName
        {
            get { return _saveName; }
            set { _saveName = value; }
        }
        protected static object instanceLock = new object();
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static MachineHandler Instance
        {
            get
            {
                if (_inner == null)
                {
                    lock (instanceLock)
                    {
                        if (_inner == null)
                        {
                            var newHandler = new MachineHandler();
                            _instanceLoaded = true;
                            if (_inner == null)
                            {
                                _inner = newHandler;
                                newHandler.Load();
                            }
                        }
                    }
                }

                return _inner;
            }
        }

        public ObservableCollection<Machine> MachineList
        {
            get { return _machineList; }
            set
            {
                _machineList = value;
                OnPropertyChanged();
            }
        }

        public MachineConfigWindow ConfigWindow
        {
            get { return _configWindow; }
            set { _configWindow = value; }
        }

        public List<ConfigurationGroup> AllConfigGroups
        {
            get
            {
                List<ConfigurationGroup> all = new List<ConfigurationGroup>();
                foreach (var machine in MachineList)
                {
                    all.AddRange(machine.ConfigurationList);
                }
                return all;
            }
        }

        public ObservableCollection<Configuration> AllConfigurations
        {
            get { return _allConfigurations; }
            set { _allConfigurations = value; }
        }
        //public ObservableCollection<Configuration> AllConfigurations
        //{
        //    get { return _allConfigurations; }
        //    set { _allConfigurations = value; }
        //}

        #endregion

        /// <summary>
        /// Private ctor
        /// </summary>
        private MachineHandler()
        {
            _machineList = new ObservableCollection<Machine>();
            _allConfigurations = new ObservableCollection<Configuration>();
        }

        /// <summary>
        /// Gets the first available machine that produces the passed item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Machine GetAvailableMachine(ProductMasterItem item)
        {
            return
                MachineList.FirstOrDefault(
                    machine => machine.ConfigurationList.Any(config => config.CanMake(item)));
        }


        /// <summary>
        /// Adds a new blank machine to the list and registers name in name list
        /// </summary>
        /// <param name="machine"></param>
        public void AddMachine(Machine machine = null)
        {
            Machine newMachine = machine;
            if (newMachine == null)
                newMachine = Machine.CreateMachine("New Machine");
            _machineList.Add(newMachine);
            //newMachine.PropertyChanged += ChildMachineChanged;
            OnPropertyChanged();
        }

        //private void ChildMachineChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        //{
        //    if (propertyChangedEventArgs.PropertyName == "Name")
        //    {
        //        Machine sendingMachine = sender as Machine;
        //        if (sendingMachine != null)
        //        {
        //            Int32 index = _machineList.IndexOf(sendingMachine);
        //            if (index != -1)
        //            {
        //                _configWindow.SaveFocus();
        //                _configWindow.ReFocus();
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Removes the machine at the passed index and updates the name list
        /// </summary>
        /// <param name="index">Index of the machine to remove</param>
        public void RemoveMachine(Int32 index)
        {
            if (index >= 0 && index < _machineList.Count)
            {

                var result = MessageBox.Show("Are you sure you want to remove this Machine?", "",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _machineList.RemoveAt(index);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Saves the machine list to a binary file
        /// </summary>
        public void Save()
        {
            try
            {
                SaveMachines(SaveName);
            }
            catch (Exception exception)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = "dat";
                saveFileDialog.Filter = "Data Files (.dat)|*.dat|All Files|*";
                saveFileDialog.Title = "Save machine data file";
                bool? accept = saveFileDialog.ShowDialog();
                if (accept == true)
                {
                    SaveName = saveFileDialog.FileName;
                    SaveMachines(SaveName);
                }
            }
        }

        /// <summary>
        /// Loads the machine list
        /// </summary>
        public void Load(bool quietMode = false)
        {
            string fileName = SaveName;
            try
            {
                using (Stream stream = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    MachineList.Clear();
                    MachineList = (ObservableCollection<Machine>)formatter.Deserialize(stream);
                    _hasLoaded = true;
                    RefreshConfigurations();
                }
            }
            catch (Exception exception)
            {
                // don't show the dialog if calling from the instance usage.
                if (quietMode) return;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "dat";
                openFileDialog.Filter = "Data Files (.dat)|*.dat|All Files|*";
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "Open machine data file";

                bool? accept = openFileDialog.ShowDialog();
                if (accept == true)
                {
                    fileName = openFileDialog.FileName;
                    try
                    {

                        using (Stream stream = new FileStream(fileName, FileMode.OpenOrCreate))
                        {
                            MachineList.Clear();
                            MachineList = (ObservableCollection<Machine>)formatter.Deserialize(stream);
                            RefreshConfigurations();
                            _hasLoaded = true;
                        }

                    }
                    catch (Exception inException)
                    {
                        MessageBox.Show("Unable to load Machine configuration. Error: " + inException.Message, "ERROR", MessageBoxButton.OK);
                    }
                }
            }
        }
        protected void SaveMachines(string fileName)
        {
            File.Create(fileName).Close();
            using (Stream stream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Machine>));
                serializer.Serialize(stream, MachineList);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool LoadDialog()
        {
            bool loaded = false;
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "dat";
                openFileDialog.Filter = "Data Files (.dat)|*.dat|All Files|*";
                openFileDialog.Multiselect = false;

                bool? accept = openFileDialog.ShowDialog();
                if (accept == true)
                {
                    //SaveName = openFileDialog.FileName;
                    using (Stream stream = new FileStream(openFileDialog.FileName, FileMode.OpenOrCreate))
                    {
                        MachineList = (ObservableCollection<Machine>)formatter.Deserialize(stream);
                        _hasLoaded = true;
                        RefreshConfigurations();
                        loaded = true;
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("An error occured while loading the file: " + exception.Message);
            }
            return loaded;
        }

        public void SaveDialog()
        {
            try
            {
                SaveMachines(SaveName);
                MessageBox.Show("Save successful");
            }
            catch (Exception exception)
            {
                MessageBox.Show("Save failed. Is the machine.dat file open?");
            }
        }

        public static void RecheckMachineConfigRefs()
        {
            foreach (var machine in Instance.MachineList)
            {
                machine.CheckConfigRefs();
            }
        }

        public void RemoveMachine(Machine machine)
        {
            int index = MachineList.IndexOf(machine);
            if (index < 0 || machine == null) return;

            RemoveMachine(index);
        }

        public void UpdateName(Machine machine, string value)
        {
            if (PropertyChanged != null) PropertyChanged(machine, new PropertyChangedEventArgs("Name"));
        }

        public void RefreshConfigurations()
        {
            List<Configuration> checkConfigurations = new List<Configuration>(AllConfigurations);
            foreach (var machine in MachineList)
            {
                foreach (var configurationGroup in machine.ConfigurationList)
                {
                    foreach (var configuration in configurationGroup.Configurations)
                    {
                        checkConfigurations.Remove(configuration);
                        if (!AllConfigurations.Contains(configuration))
                        {
                            AllConfigurations.Add(configuration);
                        }
                    }
                }
            }
            // configs that are not used by anything.
            foreach (var checkConfiguration in checkConfigurations)
            {
                AllConfigurations.Remove(checkConfiguration);
            }
            if (ConfigWindow != null)
            {
                ConfigWindow.ViewModel.RefreshConfigs();
            }
        }

        public bool IsConfigNameUnique(string name)
        {
            return Instance != null && !AllConfigurations.Any(config => config.Name.Equals(name));
        }
    }
}
