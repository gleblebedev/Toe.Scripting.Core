using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToBrushConverter : AbstractBooleanConverter<Brush>
    {
    }
}