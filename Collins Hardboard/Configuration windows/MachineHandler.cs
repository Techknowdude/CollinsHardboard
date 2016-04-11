using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Configuration_windows.Annotations;
using Microsoft.Win32;
using ModelLib;

namespace Configuration_windows
{
    public class MachineHandler : INotifyPropertyChanged
    {
        #region Fields
        private static MachineHandler _inner;
        private ObservableCollection<Machine> _machineList;
        private ObservableCollection<string> _machineNames;
        private MachineConfigWindow _configWindow;
        private string _saveName = @".\machines.dat";
        #endregion

        #region Properties
        /// <summary>
        /// The file that holds the machine handler information
        /// </summary>
        public string SaveName
        {
            get { return _saveName; }
            set { _saveName = value; }
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static MachineHandler Instance
        {
            get
            {
                if(_inner == null)
                    _inner = new MachineHandler();
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

        public ObservableCollection<string> MachineNames
        {
            get { return _machineNames; }
            set
            {
                _machineNames = value;
                OnPropertyChanged();
            }
        }

        public MachineConfigWindow ConfigWindow
        {
            get { return _configWindow; }
            set { _configWindow = value; }
        }

        #endregion

        /// <summary>
        /// Private ctor
        /// </summary>
        private MachineHandler()
        {   
            
            _machineList = new ObservableCollection<Machine>();
            _machineNames = new ObservableCollection<string>();
            Load();
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
                    machine => machine.ConfigurationList.Any(config => config.ItemOutID == item.MasterID));
        }


        /// <summary>
        /// Adds a new blank machine to the list and registers name in name list
        /// </summary>
        /// <param name="machine"></param>
        public void AddMachine(Machine machine = null)
        {
            Machine newMachine = machine ?? Machine.CreateMachine("New Machine");
            _machineList.Add(newMachine);
            _machineNames.Add(newMachine.Name);
            newMachine.PropertyChanged += ChildMachineChanged;
            OnPropertyChanged();
        }

        private void ChildMachineChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Name")
            {
                Machine sendingMachine = sender as Machine;
                if (sendingMachine != null)
                {
                    Int32 index = _machineList.IndexOf(sendingMachine);
                    if (index != -1)
                    {
                        _configWindow.SaveFocus();
                        _machineNames[index] = sendingMachine.Name;
                        _configWindow.ReFocus();
                    }
                }
            }
        }

        /// <summary>
        /// Removes the machine at the passed index and updates the name list
        /// </summary>
        /// <param name="index">Index of the machine to remove</param>
        public void RemoveMachine(Int32 index)
        {
            if (index >= 0 && index < _machineList.Count)
            {

                var result = MessageBox.Show("Are you sure you want to remove this configuration?", "",
                    MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    _machineList[index].PropertyChanged += ChildMachineChanged;

                    _machineList.RemoveAt(index);
                    _machineNames.RemoveAt(index);
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
            catch (Exception)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = "dat";
                saveFileDialog.Filter = "Data Files (.dat)|*.dat|All Files|*";
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
        public void Load()
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(SaveName, FileMode.OpenOrCreate)))
                {
                    Int32 configCount = reader.ReadInt32();
                    for (; configCount > 0; configCount--)
                    {
                        Machine newMachine = Machine.CreateMachine(reader);
                        AddMachine(newMachine);
                    }
                }
            }
            catch (Exception)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "dat";
                openFileDialog.Filter = "Data Files (.dat)|*.dat|All Files|*";
                openFileDialog.Multiselect = false;

                bool? accept = openFileDialog.ShowDialog();
                if (accept == true)
                {
                    SaveName = openFileDialog.FileName;
                    using (BinaryReader reader = new BinaryReader(new FileStream(SaveName, FileMode.OpenOrCreate)))
                    {
                        Int32 configCount = reader.ReadInt32();
                        for (; configCount > 0; configCount--)
                        {
                            Machine newMachine = Machine.CreateMachine(reader);
                            AddMachine(newMachine);
                        }
                    }
                }
            }
        }
        protected void SaveMachines(string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
            {
                writer.Write(MachineList.Count);
                foreach (Machine machine in MachineList)
                {
                    machine.Save(writer);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
   }
}
