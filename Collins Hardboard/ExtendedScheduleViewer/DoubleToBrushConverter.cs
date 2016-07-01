using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ExtendedScheduleViewer
{
    class DoubleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double input = (double) value;
            if(input > 0)
                    return Brushes.Green;
            if (input == 0)
                return Brushes.Black;

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
