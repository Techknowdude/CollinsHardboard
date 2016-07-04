using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Office.Interop.Excel;
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

        public static XlRgbColor GetExcelColor(ProductMasterItem item)
        {
            XlRgbColor color = XlRgbColor.rgbWhite;

            if (MasterBrushDictionary.ContainsKey(item))
            {
                Brush brush = MasterBrushDictionary[item];

                if(Equals(brush, Brushes.Aquamarine))
                    color = XlRgbColor.rgbAquamarine;
                else if(Equals(brush, Brushes.LightGreen))
                    color = XlRgbColor.rgbLightGreen;
                else if(Equals(brush, Brushes.DarkSalmon))
                    color = XlRgbColor.rgbDarkSalmon;
                else if(Equals(brush,Brushes.CadetBlue))
                    color = XlRgbColor.rgbCadetBlue;
                else if(Equals(brush, Brushes.BurlyWood))
                    color = XlRgbColor.rgbBurlyWood;
            }

            return color;
        }
    }
}
