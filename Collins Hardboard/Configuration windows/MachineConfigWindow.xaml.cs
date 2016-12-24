using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Configuration_windows.Annotations;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for MachineConfigWindow.xaml
    /// </summary>
    public partial class MachineConfigWindow : Window
    {
        #region Fields
        private MachineHandler _machineHandler = MachineHandler.Instance;
        private Machine _currentMachine;
        private bool _controlsEnabled = false;
        private Int32 _previousIndex = -1; // used for updating name changes
        private Int32 _lastFocus;
        private Int32 _savedFocus;

        #endregion

        #region Properties

        public bool ControlsEnabled
        {
            get { return _controlsEnabled; }
            set { _controlsEnabled = value; }
        }

        #endregion

        public MachineConfigWindow()
        {
            _machineHandler.ConfigWindow = this;
            try
            {
                InitializeComponent();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _machineHandler.Save();
        }


        public void ReFocus()
        {
            MachineListBox.SelectedIndex = _savedFocus;
        }

        public void SaveFocus()
        {
            _savedFocus = MachineListBox.SelectedIndex;
        }
    }

    public class MachineWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Configuration> AllConfigurations
        {
            get
            {
                return MachineHandler.Instance.AllConfigurations;
            }
        } 

        public ObservableCollection<string> AvailableLines
        {
            get { return StaticFactoryValuesManager.CoatingLines; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand AddMachineCommand
        {
            get { return new DelegateCommand(AddMachine); }
        }

        public ICommand DeleteMachineCommand
        {
            get { return new DelegateCommand(DeleteMachine);}
        }

        public ICommand SaveCommand { get { return new DelegateCommand(ShowSaveDialog); } }

        private void ShowSaveDialog()
        {
            MachineHandler.Instance.SaveDialog();
        }
        public ICommand LoadCommand { get { return new DelegateCommand(ShowLoadDialog); } }

        private void ShowLoadDialog()
        {
            MachineHandler.Instance.LoadDialog();
            OnPropertyChanged(nameof(MachineList));
            OnPropertyChanged(nameof(AvailableMachines));
        }

        public ObservableCollection<Machine> MachineList
        {
            get { return MachineHandler.Instance.MachineList; }
        }

        public ObservableCollection<ProductMasterItem> MasterList
        {
            get { return StaticInventoryTracker.ProductMasterList; }
        }

        public ObservableCollection<Machine> AvailableMachines
        {
            get { return MachineHandler.Instance.MachineList; }
        }
        
        public ObservableCollection<ConfigurationGroup> ExistingConfigGroups
        {
            get { return MachineHandler.Instance.AllConfigGroups; }
        }
        private void DeleteMachine(object obj)
        {
            Machine machine = obj as Machine;
            MachineHandler.Instance.RemoveMachine(machine);
        }

        private void AddMachine(object obj = null)
        {
            try
            {

                Machine machine = obj as Machine;
                MachineHandler.Instance.AddMachine(machine);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshConfigs()
        {
            OnPropertyChanged(nameof(AllConfigurations));
        }
    }
}
