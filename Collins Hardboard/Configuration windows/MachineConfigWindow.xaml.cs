using System;
using System.Windows;
using System.Windows.Controls;

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
        private MachineControl machineControl = new MachineControl();
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
            InitializeComponent();
            DataContext = _currentMachine;
            MainStackPanel.Children.Add(machineControl);
         
            MachineListBox.ItemsSource = _machineHandler.MachineNames;
            MachineListBox.DataContext = _machineHandler.MachineNames;
            ToggleControls();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _machineHandler.Save();
        }

        /// <summary>
        /// Toggles the controls for a machine when one is not selected
        /// </summary>
        private void ToggleControls()
        {
            machineControl.IsEnabled = ControlsEnabled;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _machineHandler.AddMachine();
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            _machineHandler.RemoveMachine(MachineListBox.SelectedIndex);
        }

        private void MachineListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MachineListBox.SelectedIndex != -1)
            {
                // update before 
                machineControl.Machine = _machineHandler.MachineList[MachineListBox.SelectedIndex];
                ControlsEnabled = true;
                _previousIndex = MachineListBox.SelectedIndex;
            }
            else
            {
                ControlsEnabled = false;
            }
            ToggleControls();   
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

}
