using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(LinkViewModel.LinkState), typeof(Brush))]
    public class LinkStateToBrushConverter : IValueConverter
    {
        public Brush Data { get; set; }
        public Brush Execution { get; set; }
        public Brush Selected { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LinkViewModel.LinkState))
                return Data;
            switch ((LinkViewModel.LinkState) value)
            {
                case LinkViewModel.LinkState.Data:
                    return Data;
                case LinkViewModel.LinkState.Execution:
                    return Execution;
                case LinkViewModel.LinkState.Selected:
                    return Selected;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LinkViewModel.LinkState.Data;
        }
    }
}