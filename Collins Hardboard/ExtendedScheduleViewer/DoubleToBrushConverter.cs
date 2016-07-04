using System;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Office.Interop.Excel;
using Color = System.Drawing.Color;

namespace ExtendedScheduleViewer
{
    class DoubleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double input = (double) value;

            return new SolidColorBrush(GetBrushMediaColor(input));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Converter for XAML page
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static System.Windows.Media.Color GetBrushMediaColor(double input)
        {
            if (input > 0)
                return Colors.Green;
            if (input == 0)
                return Colors.Black;

            return Colors.Red;
        }

        /// <summary>
        /// Converter for excel
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static XlRgbColor GetBrushColor(double input)
        {
            if (input > 0)
                return XlRgbColor.rgbGreen;
            if (input == 0)
                return XlRgbColor.rgbBlack;

            return XlRgbColor.rgbRed;
        }
    }

}
