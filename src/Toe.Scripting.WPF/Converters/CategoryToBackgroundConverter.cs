using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(NodeCategory), typeof(Brush))]
    public class CategoryToBackgroundConverter : IValueConverter
    {
        public static readonly Brush Default = new SolidColorBrush(Color.FromRgb(0x25, 0x3F, 0x4E));
        public Brush Value { get; set; } = Default;

        public Brush Parameter { get; set; } = Default;
        public Brush Event { get; set; } = Default;

        public Brush Procedure { get; set; } = Default;

        public Brush Function { get; set; } = Default;
        public Brush Converter { get; set; } = Default;
        public Brush Result { get; set; } = Default;
        public Brush Unknown { get; set; } = Default;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return NodeCategory.Unknown;
            switch ((NodeCategory) value)
            {
                case NodeCategory.Unknown:
                    return Unknown;
                case NodeCategory.Function:
                    return Function;
                case NodeCategory.Procedure:
                    return Procedure;
                case NodeCategory.Event:
                    return Event;
                case NodeCategory.Parameter:
                    return Parameter;
                case NodeCategory.Converter:
                    return Converter;
                case NodeCategory.Value:
                    return Value;
                case NodeCategory.Result:
                    return Result;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}