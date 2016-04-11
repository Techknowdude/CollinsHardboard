using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ImportLib;
using ModelLib;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for RunBeforeControl.xaml
    /// </summary>
    public partial class RunBeforeControl : GenControl
    {
        private ProductMasterItem _beforeItem;
        private ProductMasterItem _afterItem;
        public static String Type { get { return "RunBeforeControl"; } }

        public ProductMasterItem BeforeItem
        {
            get { return _beforeItem; }
            set
            {
                if (_beforeItem != value)
                {
                    var item = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.Equals(value));
                    BeforeComboBox.SelectedIndex = StaticInventoryTracker.ProductMasterList.IndexOf(item);
                    _beforeItem = item ?? value;
                }
            }
        }

        public ProductMasterItem AfterItem
        {
            get { return _afterItem; }
            set
            {
                if (_afterItem != value)
                {
                    var item = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.Equals(value));
                    AfterComboBox.SelectedIndex = StaticInventoryTracker.ProductMasterList.IndexOf(item);
                    _afterItem = item ?? value;
                }
            }
        }

        public RunBeforeControl(ScheduleGenWindow window, ProductMasterItem before = null, ProductMasterItem after = null, int priority = 1) : base(window)
        {
            InitializeComponent();

            BeforeComboBox.ItemsSource = StaticInventoryTracker.ProductMasterList;
            AfterComboBox.ItemsSource = StaticInventoryTracker.ProductMasterList;

            BeforeItem = before;
            AfterItem = after;
            Priority = priority;
        }

        private void BeforeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BeforeComboBox.SelectedIndex != -1)
            {
                BeforeItem = BeforeComboBox.SelectedItem as ProductMasterItem;
            }
        }
        private void AfterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AfterComboBox.SelectedIndex != -1)
            {
                AfterItem = AfterComboBox.SelectedItem as ProductMasterItem;
            }
        }

        public override string ChildType
        {
            get { return Type; }
        }

        public override int GetCost(ProductMasterItem item)
        {
            if (item.Equals(BeforeItem))
            {
                if (ScheduleGenerator.ProductItems.Contains(AfterItem))
                    return Priority;
            }
            else if(item.Equals(AfterItem))
            {
                if (ScheduleGenerator.ScheduledItems.Contains(BeforeItem))
                    return Priority;
            }
            return 0;
        }

        public static GenControl Load(BinaryReader reader, ScheduleGenWindow window)
        {
            ProductMasterItem before = ProductMasterItem.Load(reader);
            ProductMasterItem after = ProductMasterItem.Load(reader);
            int priority = reader.ReadInt32();

            return new RunBeforeControl(window,before,after,priority);
        }

        public override bool Save(BinaryWriter writer)
        {
            try
            {
                writer.Write(Type);
                BeforeItem.Save(writer);
                AfterItem.Save(writer);
                writer.Write(Priority);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }
    }
}
