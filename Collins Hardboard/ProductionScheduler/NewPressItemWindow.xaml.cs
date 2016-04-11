using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for NewPressItemWindow.xaml
    /// </summary>
    public partial class NewPressItemWindow : Window
    {
        public PressItem CreatedItem { get; set; }
        public ProductMasterItem MasterItem { get; set; }

        private ObservableCollection<ProductMasterItem> MasterList { get { return StaticInventoryTracker.ProductMasterList; } } 
        public NewPressItemWindow()
        {
            InitializeComponent();
            MasterComboBox.ItemsSource = MasterList;
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (EndPicker.Value != null) //StartPicker.Value != null 
            {
                Int32 numShifts = 0;
                if (ShiftUpDown.Value != null)
                {
                    numShifts = (int) ShiftUpDown.Value;
                }

                CreatedItem = PressItem.CreatePressItem(ThicknessTextBox.Text,
                        numShifts, (DateTime) EndPicker.Value, MasterItem);
                CreatedItem.Desctiption = NameTextBox.Text;

                DialogResult = true;
                Close();
            }
            else
            {
                //if (StartPicker.Value == null)
                //    MessageBox.Show("Start value is invalid.");
                if (EndPicker.Value == null)
                    MessageBox.Show("End value is invalid.");
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreatedItem = null;
            Close();
        }

        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MasterComboBox.SelectedIndex != -1)
            {
                ProductMasterItem selectedItem = MasterComboBox.SelectedItem as ProductMasterItem;
                if (selectedItem != null)
                {
                    LoadMasterInfo(selectedItem);
                }
            }
            else
            {
                MasterItem = null;
            }
        }

        private void LoadMasterInfo(ProductMasterItem selectedItem)
        {
            NameTextBox.Text = selectedItem.Description;
            ThicknessTextBox.Text = StaticFunctions.ConvertDoubleToStringThickness(selectedItem.Thickness);
            MasterItem = selectedItem;
        }
    }
}
