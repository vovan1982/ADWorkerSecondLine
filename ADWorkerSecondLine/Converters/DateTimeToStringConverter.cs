using System;
using System.Windows.Data;

namespace ADWorkerSecondLine.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new Object();
            if (value is DateTime)
            {
                if (((DateTime)value) != new DateTime(1970, 01, 01, 00, 00, 00))
                    if (((DateTime)value).Date == new DateTime(1601, 02, 12))
                        result = "Требуется изменить пароль";
                    else
                        result = ((DateTime)value).ToString("dd.MM.yyyy HH:mm:ss");
                else
                    result = "Отключено";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
