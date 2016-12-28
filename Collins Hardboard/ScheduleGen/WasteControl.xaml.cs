using System;
using System.IO;
using System.Windows;
using ModelLib;
using StaticHelpers;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for WasteControl.xaml
    /// </summary>
    public partial class WasteControl : GenControl
    {
        public static String Type { get { return "WasteControl"; } }

        public String WasteMin
        {
            get { return StaticFactoryValuesManager.WasteMin.ToString("N"); }
            set
            {
                double data;
                if (Double.TryParse(value, out data))
                    StaticFactoryValuesManager.WasteMin = data;
            }
        }

        public String WasteMax
        {
            get { return StaticFactoryValuesManager.WasteMax.ToString("N"); }
            set
            {
                double data;
                if (Double.TryParse(value, out data))
                    StaticFactoryValuesManager.WasteMax = data;
            }
        }


        public WasteControl(ScheduleGenWindow parent, int priority = 1) : base(parent)
        {
            InitializeComponent();

            Priority = priority;

            UpdateControlInfo();
        }

        private void UpdateControlInfo()
        {
            WasteMinBox.Text = WasteMin;
            WasteMaxBox.Text = WasteMax;
            PriorityTextBox.Text = PriorityText;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null) ParentWindow.Remove(this);
        }

        public override string ChildType
        {
            get { return Type; }
        }


        public override int GetCost(ProductMasterItem item)
        {
            double avgWaste = (StaticFactoryValuesManager.WasteMin + StaticFactoryValuesManager.WasteMax)/2;

            if (ScheduleGenerator.Instance.CurrentWaste < avgWaste && item.Waste > 0)
                return Priority;

            if (ScheduleGenerator.Instance.CurrentWaste > avgWaste && item.Waste < 0)
                return Priority;

            return 0;
        }

        public override bool Save(BinaryWriter writer)
        {
            try
            {
                writer.Write(Type);
                writer.Write(Priority);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static WasteControl Load(BinaryReader reader, ScheduleGenWindow parent)
        {
            int priority = reader.ReadInt32();

            return new WasteControl(parent,priority);
        }
    }
}
