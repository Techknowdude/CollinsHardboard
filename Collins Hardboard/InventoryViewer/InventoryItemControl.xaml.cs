using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace InventoryViewer
{
    /// <summary>
    /// Interaction logic for InventoryItemControl.xaml
    /// </summary>
    public partial class InventoryItemControl : UserControl
    {

        private InventoryWindow _parentWindow;

        #region Properties

        public InventoryItem InvItem { get; set; }

        public double Units
        {
            get { return InvItem.Units; }
            set { InvItem.Units = value; }
        }

        public int ID
        {
            get { return InvItem.MasterID; }
            set { InvItem.MasterID = value; }
        }

        public String Grade
        {
            get { return InvItem.Grade; }
            set { InvItem.Grade = value; }
        }

        #endregion
        
        public InventoryItemControl(InventoryWindow parent, InventoryItem inventoryItem)
        {
            InitializeComponent();
            UnitsTextBox.TextChanged += UnitsTextBox_TextChanged;

            InvItem = inventoryItem;
            _parentWindow = parent;

            MasterComboBox.ItemsSource = StaticInventoryTracker.ProductMasterList;
            GradeComboBox.ItemsSource = StaticFactoryValuesManager.GradesList;
            UpdateControlInfo();
        }

        

        private void UpdateControlInfo()
        {
            UnitsTextBox.Text = Units.ToString();

            // get master item
            ProductMasterItem master =
                StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == InvItem.MasterID);
            MasterComboBox.SelectedIndex = StaticInventoryTracker.ProductMasterList.IndexOf(master);

            int gradeIndex = StaticFactoryValuesManager.GradesList.IndexOf(Grade);
            if (gradeIndex == -1)
                gradeIndex = StaticFactoryValuesManager.GradeAbbrList.IndexOf(Grade);
            GradeComboBox.SelectedIndex = gradeIndex;

        }

        void UnitsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double temp;
            if (Double.TryParse(UnitsTextBox.Text, out temp))
            {
                UnitsTextBox.BorderBrush = Brushes.Blue;
                Units = temp;
            }
            else
                UnitsTextBox.BorderBrush = Brushes.Red;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            _parentWindow.RemoveControl(this);
        }

        private void MasterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MasterComboBox.SelectedIndex != -1)
            {
                ID = ((ProductMasterItem) MasterComboBox.SelectedItem).MasterID;
            }
        }

        private void GradeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Grade = (string) GradeComboBox.SelectedItem;
        }
    }
}
