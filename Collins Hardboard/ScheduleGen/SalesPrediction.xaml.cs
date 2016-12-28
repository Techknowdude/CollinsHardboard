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
    /// Interaction logic for SalesPrediction.xaml
    /// </summary>
    public partial class SalesPrediction : GenControl
    {
        public SalesDurationEnum SalesDuration { get; set; }

        public static String Type { get { return "SalesPrediction"; } }
        public SalesPrediction(ScheduleGenWindow window, int priority = 0, SalesDurationEnum duration = default(SalesDurationEnum)) :base(window)
        {
            InitializeComponent();
            Priority = priority;
            SalesDuration = duration;
            switch(SalesDuration) // set box to correct index
            {
                case SalesDurationEnum.Last12Months:
                    PastSalesComboBox.SelectedIndex = 0;
                    break;

                case SalesDurationEnum.Last3Months:
                    PastSalesComboBox.SelectedIndex = 1;
                    break;

                case SalesDurationEnum.Last6Months:
                    PastSalesComboBox.SelectedIndex = 2;
                    break;

                case SalesDurationEnum.LastMonth:
                    PastSalesComboBox.SelectedIndex = 3;
                    break;

                case SalesDurationEnum.LastYear:
                    PastSalesComboBox.SelectedIndex = 4;
                    break;
            }
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
            var inv = ScheduleGenerator.Instance.CurrentInventory.FirstOrDefault(x => x.MasterID == item.MasterID);
            if (item.TurnType == "U")
            {
                if (inv != null && item.MinSupply > inv.Units)
                    return Priority;
            }
            else
            {
                var forecast =
                    StaticInventoryTracker.ForecastItems.FirstOrDefault(x => x.MasterID == item.MasterID);
                if (inv != null && forecast != null)
                {
                    double available;
                    switch (SalesDuration)
                    {
                        case SalesDurationEnum.LastMonth:
                            available = forecast.AvgOneMonth;
                            break;
                        case SalesDurationEnum.Last3Months:
                            available = forecast.AvgThreeMonths;
                            break;
                        case SalesDurationEnum.Last6Months:
                            available = forecast.AvgSixMonths;
                            break;
                        case SalesDurationEnum.Last12Months:
                            available = forecast.AvgTwelveMonths;
                            break;
                        case SalesDurationEnum.LastYear:
                            available = forecast.AvgPastYear;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    if (available < item.MinSupply)
                        return Priority; 
                }
            }
            return 0;
        }

        public override bool Save(BinaryWriter writer)
        {
            try
            {
                writer.Write(Type);
                writer.Write(Priority);
                writer.Write((int) SalesDuration);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static SalesPrediction Load(BinaryReader reader, ScheduleGenWindow window)
        {
            int priority = reader.ReadInt32();
            SalesDurationEnum duration = (SalesDurationEnum) reader.ReadInt32();
            return new SalesPrediction(window,priority,duration);
        }

        private void PastSalesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = PastSalesComboBox.SelectedIndex;
            if (index != -1)
            {
                SalesDuration = (SalesDurationEnum) index;
            }
        }

        public enum SalesDurationEnum
        {
            LastMonth,
            Last3Months,
            Last6Months,
            Last12Months,
            LastYear
        }
    }
}
