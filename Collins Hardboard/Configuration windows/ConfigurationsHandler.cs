using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Configuration_windows.Annotations;
using Microsoft.Win32;

namespace Configuration_windows
{
    public delegate void OnNameChange(String name);

    /// <summary>
    /// Singleton for the list of configurations
    /// </summary>
    public class ConfigurationsHandler : INotifyPropertyChanged
    {
        #region Fields
        ObservableCollection<Configuration> _configurations = new ObservableCollection<Configuration>(); 
        ObservableCollection<String> _configurationNames = new ObservableCollection<String>();
        private OnNameChange _changeName;
        private string _saveName = @".\configurations.dat";

        #endregion

        #region Properties

        public String SaveName
        {
            get { return _saveName; }
            set { _saveName = value; }
        }

        public OnNameChange ChangeName
        {
            get { return _changeName; }
            set
            {
                if (_changeName != value)
                {
                    _changeName = value;
                    foreach (Configuration configuration in _configurations)
                    {
                        configuration.NameChanged = ChangeName;
                    }
                }
            }
        }

        /// <summary>
        /// List of configurations
        /// </summary>
        public ObservableCollection<Configuration> Configurations
        {
            get { return _configurations; }
            set
            {
                if (value != _configurations)
                {
                    _configurations = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// List of names to use for the dropdown
        /// </summary>
        public ObservableCollection<String> ConfigurationNames
        {
            get { return _configurationNames; }
            
        }

        #endregion

        /// <summary>
        /// Singleton pattern
        /// </summary>
        private static ConfigurationsHandler _inner;
        public static ConfigurationsHandler GetInstance()
        {
            if (_inner == null)
            {
                _inner = new ConfigurationsHandler();
                _inner.Load();
            }
            return _inner;
        }

        /// <summary>
        /// Contructor fofr config handler
        /// </summary>
        private ConfigurationsHandler()
        {
            
        }

        /// <summary>
        /// Adds the configuration to the list
        /// </summary>
        /// <param name="config"></param>
        public void AddConfiguration(Configuration config)
        {
            if (!_configurations.Contains(config))
            {
                _configurations.Add(config);
                _configurationNames.Add(config.Name);
                config.NameChanged = ChangeName;
            }
            OnPropertyChanged();
        }

        /// <summary>
        /// Removes the configuration from the list
        /// </summary>
        /// <param name="index">Index of config to remove</param>
        public void RemoveConfiguration(Int32 index)
        {
            if (_configurations.Count > 0 && _configurations.Count > index)
            {
                var result = MessageBox.Show("Are you sure you want to remove this configuration?", "",
                    MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    _configurations.RemoveAt(index);
                    _configurationNames.RemoveAt(index);
                    OnPropertyChanged();
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

        /// <summary>
        /// Adds a default configuration to the list.
        /// Used by the configuration window.
        /// </summary>
        public void AddConfiguration()
        {
            AddConfiguration(Configuration.CreateConfiguration("New configuration"));
        }

        /// <summary>
        /// Removes a configuration window from the delegate list.
        /// This is part of the multi-window support
        /// </summary>
        /// <param name="updateChange"></param>
        public void ClearDelegate(OnNameChange updateChange)
        {
            ChangeName -= updateChange;
        }

        /// <summary>
        /// Saves the configuration list
        /// </summary>
        public void Save()
        {
            try
            {
                SaveConfigurations(SaveName);
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
                    SaveConfigurations(SaveName);
                }
            }
        }

        private void SaveConfigurations(string saveName)
        {

            using (BinaryWriter writer = new BinaryWriter(new FileStream(saveName, FileMode.OpenOrCreate)))
            {
                writer.Write(Configurations.Count);
                foreach (Configuration configuration in Configurations)
                {
                    configuration.Save(writer);
                }
            }
        }

        /// <summary>
        /// Loads the configuration list
        /// </summary>
        public void Load(bool autoLoad = true)
        {
            string fileName = SaveName;
            try
            {
                if (!autoLoad)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.DefaultExt = "dat";
                    openFileDialog.Filter = "Data Files (.dat)|*.dat|All Files|*";
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = "Open Configuration file";

                    bool? accept = openFileDialog.ShowDialog();
                    if (accept == true)
                    {
                        fileName = openFileDialog.FileName;
                    }
                }
                using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.OpenOrCreate)))
                {
                    Configurations.Clear();
                    ConfigurationNames.Clear();

                    Int32 configCount = reader.ReadInt32();
                    for(; configCount > 0; configCount--)
                    {
                        Configuration newConfiguration = Configuration.CreateConfiguration(reader);
                        AddConfiguration(newConfiguration);
                    }
                }
            }
            catch (Exception)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "dat";
                openFileDialog.Filter = "Data Files (.dat)|*.dat|All Files|*";
                openFileDialog.Title = "Open Configuration file";
                openFileDialog.Multiselect = false;

                bool? accept = openFileDialog.ShowDialog();
                if (accept == true)
                {
                    try
                    {
                        fileName = openFileDialog.FileName;
                        using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.OpenOrCreate)))
                        {
                            Configurations.Clear();
                            ConfigurationNames.Clear();

                            Int32 configCount = reader.ReadInt32();
                            for (; configCount > 0; configCount--)
                            {
                                Configuration newConfiguration = Configuration.CreateConfiguration(reader);
                                AddConfiguration(newConfiguration);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Configurations import failed.");
                    }
                }
            }

            MachineHandler.RecheckMachineConfigRefs();
        }
    }
}
