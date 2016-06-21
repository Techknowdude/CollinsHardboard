using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ImportLib;
using ModelLib;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PressItemControl.xaml
    /// </summary>
    public partial class PressItemControl : UserControl
    {

        #region Fields

        //private DateTime _startDate;
        private string _thickness;
        private Int32 _numShifts;
        private DateTime _endTime;
        private ProductMasterItem _product;
        private String _itemName;
        private bool _isTrial;
        private double _hours;
        #endregion

        #region Properties

        public String ItemName
        {
            get { return _itemName; }
            set { _itemName = value; }
        }
        //public DateTime StartDate
        //{
        //    get { return _startDate; }
        //    set { _startDate = value; }
        //}

        public string Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        public Int32 NumShifts
        {
            get { return _numShifts; }
            set { _numShifts = value; }
        }

        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        #endregion
        public double UnitsProduced { get { return _product.UnitsPerHour*Hours; } }

        public PressItem Item { get; set; }

        public ObservableCollection<ProductMasterItem> MasterItems => StaticInventoryTracker.ProductMasterList;

        public PressWeekControl WeekControl { get; set; }

        public bool IsTrial
        {
            get { return _isTrial; }
            set
            {
                _isTrial = value;
            }
        }

        public double Hours
        {
            get { return _hours; }
            set
            {
                _hours = value;
            }
        }

        public PressItemControl(PressItem item = null, PressWeekControl pressWeekControl = null)
        {
            WeekControl = pressWeekControl;
            try
            {
                InitializeComponent();
                DataContext = this;

                Item = item;
                ShiftNumber.Visibility = Visibility.Collapsed;
                RunTypeBox.SelectionChanged += RunTypeBox_OnSelectionChanged;

                LoadItemInfo();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        

        private void LoadItemInfo()
        {
            if (Item != null)
            {
               // StartDate = Item.StartDate;
                Thickness = Item.Thickness;
                NumShifts = Item.NumShifts;
                EndTime = Item.EndTime;
                ItemName = Item.Desctiption;
                IsTrial = Item.IsTrial;
            }
        }

        private void RunTypeBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RunTypeBox.SelectedIndex == 0)
                ShiftNumber.Visibility = Visibility.Collapsed;
            else if(RunTypeBox.SelectedIndex == 1)
                ShiftNumber.Visibility = Visibility.Visible;
        }

        private void UpButton_OnClick(object sender, RoutedEventArgs e)
        {
            WeekControl.MoveUp(this);
        }

        private void DownButton_OnClick(object sender, RoutedEventArgs e)
        {
            WeekControl.MoveDown(this);
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Are you sure?","",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                WeekControl.RemoveItem(this);
        }

        public void Save(BinaryWriter writer)
        {
            //writer.Write(StartDate.ToLongTimeString());
            writer.Write(Thickness);
            writer.Write(NumShifts);
            writer.Write(EndTime.ToLongTimeString());
            writer.Write(_product != null);
            if(_product != null)
                _product.Save(writer);
            writer.Write(ItemName);
            writer.Write(IsTrial);
        }

        public static PressItemControl Load(BinaryReader reader)
        {
            //DateTime start = DateTime.Parse(reader.ReadString());
            String thick = reader.ReadString();
            Int32 shifts = reader.ReadInt32();
            DateTime end = DateTime.Parse(reader.ReadString());
            bool isProd = reader.ReadBoolean();
            ProductMasterItem master = null;
            if(isProd)
                master = ProductMasterItem.Load(reader);

            PressItem item = new PressItem(thick, shifts, end, master); //start, 
            item.Desctiption = reader.ReadString();
            item.IsTrial = reader.ReadBoolean();

            return new PressItemControl(item);
        }

        private void TitleTextBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductMasterItem selectedItem = TitleTextBox.SelectedValue as ProductMasterItem;

            if (selectedItem == null) return;

            ItemName = selectedItem.Description;
            Thickness = StaticHelpers.StaticFunctions.ConvertDoubleToStringThickness(selectedItem.Thickness);
            _product = selectedItem;
        }
    }
}
