using System;
using System.Windows.Data;

namespace ADWorkerSecondLine.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new Object();
            if (value is bool)
            {
                if ((bool)value)
                    result = "Да";
                else
                    result = "Нет";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
