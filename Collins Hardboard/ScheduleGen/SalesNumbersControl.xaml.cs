using System;
using System.IO;
using System.Linq;
using System.Windows;
using ImportLib;
using ModelLib;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for SalesNumbersControl.xaml
    /// </summary>
    public partial class SalesNumbersControl : GenControl
    {
        private const double priorityToUnits = .05d;
        public static String Type { get { return "SalesNumbers"; } }
        public SalesNumbersControl(ScheduleGenWindow window, int priority = 1)
            : base(window)
        {
            InitializeComponent();

            Priority = priority;
        }


        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }

        public override string ChildType
        {
            get { return Type; }
        }

        public override int GetCost(ProductMasterItem item)
        {
            var sales = StaticInventoryTracker.SalesItems.Where(x => (x.Units- x.Fulfilled) < 0.01 && x.MasterID == item.MasterID && x.Date < ScheduleGenerator.Instance.GetSalesRange()); // get all not filled ordered

            int currentPriority = 0;

            foreach (var salesItem in sales)
            {
                currentPriority += (int)(Priority*salesItem.Units*priorityToUnits); // prioritize 
            }

            return currentPriority;
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

        public static SalesNumbersControl Load(BinaryReader reader,ScheduleGenWindow window)
        {
            int priority = reader.ReadInt32();
            
            return new SalesNumbersControl(window,priority);
        }
    }
}
