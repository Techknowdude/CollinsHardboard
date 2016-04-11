using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using StaticHelpers;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for MachineControl.xaml
    /// </summary>
    public partial class MachineControl : UserControl
    {
        private Machine _machine;
        public MachineControl()
        {
            InitializeComponent();
            DataContext = Machine;

            ConfigComboBox.ItemsSource = ConfigurationsHandler.GetInstance().Configurations;
            LineComboBox.ItemsSource = StaticFactoryValuesManager.CoatingLines;
            MachineConfComboBox.ItemsSource = MachineHandler.Instance.MachineList;
            LineConfComboBox.ItemsSource = StaticFactoryValuesManager.CoatingLines;
        }

        public Machine Machine
        {
            get { return _machine; }
            set
            {
                if (_machine != value)
                {
                    _machine = value;
                    DataContext = _machine;
                }
            }
        }


        private void DeleteConfigButton_OnClick(object sender, RoutedEventArgs e)
        {

            if (MachineConfigsListBox.SelectedIndex != -1 && Machine != null)
            {
                Machine.RemoveConfiguration(MachineConfigsListBox.SelectedIndex);
            }
        }

        private void AddLineButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (Machine != null && LineComboBox.SelectedIndex != -1)
            {
                Machine.LinesCanRunOn.Add(StaticFactoryValuesManager.CoatingLines[LineComboBox.SelectedIndex]);
            }
        }

        private void DeleteLineButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (Machine != null && MachineLinesListBox.SelectedIndex != -1)
            {
                    Machine.LinesCanRunOn.RemoveAt(MachineLinesListBox.SelectedIndex);
            }
        }

        private void AddConfigButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (Machine != null && ConfigComboBox.SelectedIndex != -1)
            {
                Machine.AddConfiguration(ConfigurationsHandler.GetInstance().Configurations[ConfigComboBox.SelectedIndex]);
            }      
        }

        private void AddMachineConflictButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MachineConfComboBox.SelectedIndex != -1)
            {
                Machine.MachineConflicts.Add(((Machine) MachineConfComboBox.SelectedItem).Name);
            }
        }

        private void DeleteMachineConflictButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MachineConflictBox.SelectedIndex != -1)
            {
                    Machine.MachineConflicts.RemoveAt(MachineConflictBox.SelectedIndex);
            }
        }

        private void AddLineConflictButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (LineConfComboBox.SelectedIndex != -1)
            {
                Machine.LineConflicts.Add((string) LineConfComboBox.SelectedItem);
            }
        }

        private void DeleteLineConflictButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (LineConflictBox.SelectedIndex != -1)
            {
                Machine.LineConflicts.RemoveAt(LineConflictBox.SelectedIndex);
            }
        }
    }
}
