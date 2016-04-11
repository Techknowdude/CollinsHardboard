using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImportLib;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for ConfigurationsWindow.xaml
    /// </summary>
    public partial class ConfigurationsWindow : Window
    {
        #region Fields
        private ConfigurationsHandler configurationsHandler = ConfigurationsHandler.GetInstance();

        private Configuration currentConfiguration;
        private bool _enableConfigControls;

        #endregion

        #region Properties



        public bool EnableConfigControls
        {
            get { return _enableConfigControls; }
            set
            {
                if (value != _enableConfigControls)
                {
                    _enableConfigControls = value;
                    ToggleControls();
                }
            }
        } 
        #endregion


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            configurationsHandler.ClearDelegate(UpdateName);
            configurationsHandler.Save();
        }

        public ConfigurationsWindow()
        {
            InitializeComponent();
            ConfigListBox.DataContext = configurationsHandler.ConfigurationNames;
            ConfigListBox.ItemsSource = configurationsHandler.ConfigurationNames;
            DataContext = currentConfiguration;
            ItemInDropDown.ItemsSource =
                StaticInventoryTracker.ProductMasterList.Select(productMasterItem => productMasterItem.Description);
            ItemOutDropDown.ItemsSource =
                StaticInventoryTracker.ProductMasterList.Select(productMasterItem => productMasterItem.Description);
            configurationsHandler.ChangeName += UpdateName;
            ToggleControls();
        }

        private void UpdateName(string name)
        {
            Int32 index = ConfigListBox.SelectedIndex;
            if (index != -1)
            {
                configurationsHandler.ConfigurationNames[ConfigListBox.SelectedIndex] = name;
                ConfigListBox.SelectedIndex = index;
            }
        }

        private void ConfigListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            // set the current configuration on selection changed.
            EnableConfigControls = ConfigListBox.SelectedIndex != -1;
            currentConfiguration = ConfigListBox.SelectedIndex != -1 ? configurationsHandler.Configurations[ConfigListBox.SelectedIndex] : null;
            DataContext = currentConfiguration;
            if (currentConfiguration != null)
            {
                LoadConfigData();
            }
        }

        private void LoadConfigData()
        {
            ChangeTimeBox.Text = currentConfiguration.ChangeTime.ToString();
            ItemInDropDown.SelectedIndex = StaticInventoryTracker.ProductMasterList.IndexOf(currentConfiguration.ItemIn);
            ItemOutDropDown.SelectedIndex = StaticInventoryTracker.ProductMasterList.IndexOf(currentConfiguration.ItemOut);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            configurationsHandler.AddConfiguration();

            ConfigListBox.DataContext = configurationsHandler.ConfigurationNames;
            ConfigListBox.ItemsSource = configurationsHandler.ConfigurationNames;
        }

        private void ItemInDropDown_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemInDropDown.SelectedIndex != -1 && currentConfiguration != null)
            {
                currentConfiguration.ItemInID =
                    StaticInventoryTracker.ProductMasterList[ItemInDropDown.SelectedIndex].MasterID;
            }
        }

        private void ItemOutDropDown_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemOutDropDown.SelectedIndex != -1 && currentConfiguration != null)
            {
                currentConfiguration.ItemOutID =
                    StaticInventoryTracker.ProductMasterList[ItemOutDropDown.SelectedIndex].MasterID;
            }
        }

        private void TimeBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                currentConfiguration.ChangeTime = TimeSpan.Parse(ChangeTimeBox.Text);
                ChangeTimeBox.BorderBrush = null;
            }
            catch (Exception)
            {
                ChangeTimeBox.BorderBrush = Brushes.Red;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigListBox.SelectedIndex != -1)
            {
                configurationsHandler.RemoveConfiguration(ConfigListBox.SelectedIndex);
            }
        }


        private void ToggleControls()
        {
            ChangeTimeBox.IsEnabled = EnableConfigControls;
            NameTextBox.IsEnabled = EnableConfigControls;
            ItemInDropDown.IsEnabled = EnableConfigControls;
            ItemOutDropDown.IsEnabled = EnableConfigControls;
            NumberInUpDown.IsEnabled = EnableConfigControls;
            NumberOutUpDown.IsEnabled = EnableConfigControls;
            ConversionsUpDown.IsEnabled = EnableConfigControls;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            configurationsHandler.Save();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            configurationsHandler.Load(false);
        }
    }
}
