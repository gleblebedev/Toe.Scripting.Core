using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Toe.Scripting.WPF.Converters
{
    public abstract class AbstractBooleanConverter<T> : IValueConverter
    {
        public T True { get; set; }
        public T False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is T))
                return false;
            if (EqualityComparer<T>.Default.Equals((T) parameter, True))
                return true;
            return false;
        }
    }
}