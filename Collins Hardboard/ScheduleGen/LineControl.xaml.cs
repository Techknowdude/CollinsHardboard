using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for LineControl.xaml
    /// </summary>
    public partial class LineControl: GenControl
    {
        private ProductMasterItem _masterItem;
        private string _coatingLine;

        #region Properties

        public static String Type { get { return "LineControl"; } }

        public String CoatingLine
        {
            get { return _coatingLine; }
            set
            {
                var line = StaticFactoryValuesManager.CoatingLines.FirstOrDefault(x => x == value);
                if (line != null)
                {
                    LineComboBox.SelectedIndex = StaticFactoryValuesManager.CoatingLines.IndexOf(line);
                }
                _coatingLine = line ?? value;
            }
        }

        public ProductMasterItem MasterItem
        {
            get { return _masterItem; }
            set
            {
                if (StaticInventoryTracker.ProductMasterList.Any(x => x.Equals(value)))
                    _masterItem = StaticInventoryTracker.ProductMasterList.First(x => x.Equals(value));
                else
                    _masterItem = value;
            }
        }

        #endregion

        public LineControl(ScheduleGenWindow parent, String line = "", ProductMasterItem master = null, int priority = 1) : base(parent)
        {
            InitializeComponent();
            LineComboBox.ItemsSource = StaticFactoryValuesManager.CoatingLines;
            ItemComboBox.ItemsSource = StaticInventoryTracker.ProductMasterList;

            CoatingLine = line;
            MasterItem = master;
            Priority = priority;

            int index = StaticInventoryTracker.ProductMasterList.IndexOf(MasterItem);
            if (index != -1)
                ItemComboBox.SelectedIndex = index;
        }

        private void LineComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LineComboBox.SelectedIndex != -1)
            {
                CoatingLine = LineComboBox.SelectedItem as String;
            }
        }

        private void ItemComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemComboBox.SelectedIndex != -1)
            {
                MasterItem = ItemComboBox.SelectedItem as ProductMasterItem;
            }
        }

        public override bool Save(BinaryWriter writer)
        {
            try
            {
                writer.Write(Type);
                writer.Write(CoatingLine);
                MasterItem.Save(writer);
                writer.Write(Priority);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public override string ChildType
        {
            get { return Type; }
        }

        public override int GetCost(ProductMasterItem item)
        {
            if (ScheduleGenerator.Instance.CurrentLine == CoatingLine && item.Equals(MasterItem))
                return 0;

            return Priority;
        }

        public static GenControl Load(BinaryReader reader, ScheduleGenWindow window)
        {
            String line = reader.ReadString();
            ProductMasterItem item = ProductMasterItem.Load(reader);
            int priority = reader.ReadInt32();

            return new LineControl(window,line,item,priority);
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }
    }
}
