using System.Windows;
using System.Windows.Data;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : AbstractBooleanConverter<Visibility>
    {
    }
}