using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Configuration_windows;
using ImportLib;
using ModelLib;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public bool Accepted { get; private set; }

        public ProductMasterItem MasterItem { get; private set; }
        public Machine ItemMachine { get; set; }
        public Configuration Config { get; set; }

        public AddProductWindow()
        {
            InitializeComponent();
            ProductComboBox.ItemsSource = StaticInventoryTracker.ProductMasterList;
            MasterItem = null;
            ItemMachine = null;
            Config = null;
            Accepted = false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MasterItem == null)
            {
                MessageBox.Show("Please select a product before adding to schedule");
            }
            else
            {
                if (ItemMachine == null)
                {
                    if (
                        MessageBox.Show("Add product without assigning a machine?",
                            "This will make automation inaccurate", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Accepted = true;
                        Close();
                    }
                }
                else
                {
                    if (Config == null)
                    {
                        if (
                            MessageBox.Show("Add product without assigning a configuration?",
                                "This will make automation inaccurate", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Accepted = true;
                            Close();
                        }
                    }
                    else
                    {
                        Accepted = true;
                        Close();
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Close();
        }

        private void MachineComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MachineComboBox.SelectedIndex != -1)
            {
                ItemMachine = MachineComboBox.SelectedItem as Machine;

                ConfigComboBox.ItemsSource =
                    ItemMachine.ConfigurationList.Where(config => config.ItemOutID == MasterItem.MasterID);
            }
            else
            {
                ItemMachine = null;

                ConfigComboBox.SelectedIndex = -1;

                Accepted = false;
            }
        }

        private void ConfigComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConfigComboBox.SelectedIndex != -1)
            {
                Config = ConfigComboBox.SelectedItem as Configuration;

                Accepted = true;
            }
            else
            {
                Config = null;

                Accepted = false;
            }
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductComboBox.SelectedIndex != -1)
            {
                MasterItem = ProductComboBox.SelectedItem as ProductMasterItem;
                MachineComboBox.ItemsSource = MachineHandler.Instance.MachineList.Where(machine =>
                    machine.ConfigurationList.Any(config => config.ItemOutID == MasterItem.MasterID));
            }
            else
            {
                MasterItem = null;
             
                // reset other boxes
                MachineComboBox.SelectedIndex = -1;

                Accepted = false;
            }
        }
    }
}
