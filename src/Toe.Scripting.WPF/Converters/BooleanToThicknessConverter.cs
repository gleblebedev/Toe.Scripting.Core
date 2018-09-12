using System.Windows;
using System.Windows.Data;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Thickness))]
    public class BooleanToThicknessConverter : AbstractBooleanConverter<Thickness>
    {
    }
}