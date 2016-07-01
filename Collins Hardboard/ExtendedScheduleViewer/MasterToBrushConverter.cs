using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;
using ModelLib;

namespace ExtendedScheduleViewer
{
    class MasterToBrushConverter : IValueConverter
    {
        private static int brushCounter = 0;
        private static readonly Brush[] BrushesList = new [] {Brushes.Aquamarine,Brushes.LightGreen,Brushes.DarkSalmon,Brushes.CadetBlue,Brushes.BurlyWood};
        private static Dictionary<ProductMasterItem,Brush> MasterBrushDictionary = new Dictionary<ProductMasterItem, Brush>(); 
        private static Brush GetNextBrush()
        {
            if (brushCounter >= BrushesList.Length)
            {
                brushCounter = 0;
            }

            return BrushesList[brushCounter++];
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ProductMasterItem master = value as ProductMasterItem;
            if (master != null)
            {
                if (!MasterBrushDictionary.ContainsKey(master))
                {
                    MasterBrushDictionary[master] = GetNextBrush();
                }

                return MasterBrushDictionary[master];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
