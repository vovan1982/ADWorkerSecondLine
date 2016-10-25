using System;
using System.Windows.Data;

namespace ADWorkerSecondLine.Converters
{
    public class DateTimeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new Object();
            if (value is DateTime)
            {
                if (((DateTime)value) != new DateTime(1970, 01, 01, 00, 00, 00) && ((DateTime)value) < DateTime.Now)
                    result = System.Windows.Media.Brushes.Red;
                else
                    result = System.Windows.Media.Brushes.Black;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
