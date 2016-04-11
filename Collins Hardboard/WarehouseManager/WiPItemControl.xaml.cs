using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace WarehouseManager
{
    /// <summary>
    /// Interaction logic for WiPItemControl.xaml
    /// </summary>
    public partial class WiPItemControl : UserControl
    {
        #region Fields

        private double _units;
        private bool _purge;
        private InventoryItem _invItem;
        private String _grade;

        #endregion

        #region Properties

        public double Units
        {
            get
            {
                if (InvItem != null)
                    return InvItem.Units;
                return _units; }
            set
            {
                if (InvItem != null)
                    InvItem.Units = value;
                _units = value;
            }
        }

        public bool Purge
        {
            get { return _purge; }
            set
            {
                if (InvItem != null)
                    InvItem.IsPurged = value;
                _purge = value;
            }
        }

        public InventoryItem InvItem
        {
            get { return _invItem; }
            set { _invItem = value; }
        }

        public WiPInventoryWindow ParentControl { get; set; }

        public string Grade
        {
            get { return _grade; }
            set
            {
                if (InvItem != null)
                    InvItem.Grade = value;
                _grade = value;
            }
        }

        #endregion

       

        public WiPItemControl(InventoryItem item, WiPInventoryWindow parent)
        {
            InitializeComponent();

            ParentControl = parent;
            DataContext = this;
            Grade = item.Grade;
            Units = item.Units;
            Purge = item.IsPurged;
            InvItem = item;
            UnitsTextBox.BorderBrush = Brushes.Blue;
            MasterComboBox.ItemsSource = StaticInventoryTracker.ProductMasterList;
            GradeComboBox.ItemsSource = StaticFactoryValuesManager.GradesList;

            UpdateControlData();
        }

        public void UpdateControlData()
        {
            UnitsTextBox.Text = Units.ToString();
            PurgeCheckBox.IsChecked = Purge;

            ProductMasterItem master =
                StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == InvItem.MasterID);
            if(master != null)
                MasterComboBox.SelectedIndex = StaticInventoryTracker.ProductMasterList.IndexOf(master);

            GradeComboBox.SelectedIndex = StaticFactoryValuesManager.GradesList.IndexOf(Grade);

        }

        private void UnitsTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            double check;
            if (Double.TryParse(UnitsTextBox.Text, out check))
            {
                UnitsTextBox.BorderBrush = Brushes.Blue;
                Units = check;
            }
            else
            {
                UnitsTextBox.BorderBrush = Brushes.Red;
            }
        }


        private void MasterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MasterComboBox.SelectedIndex != -1)
            {
                var productMasterItem = MasterComboBox.SelectedItem as ProductMasterItem;
                if (productMasterItem != null)
                {
                    InvItem.MasterID = productMasterItem.MasterID;
                    InvItem.ProductCode = productMasterItem.ProductionCode;
                    InvItem.PiecesPerUnit = productMasterItem.PiecesPerUnit;
                }
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentControl.Remove(this);
        }

        private void GradeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GradeComboBox.SelectedIndex == -1) Grade = String.Empty;
            else
            {
                Grade = GradeComboBox.SelectedItem as String;
            }
        }
    }
}
