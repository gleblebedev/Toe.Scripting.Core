using System.Windows.Data;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanNotConverter : AbstractBooleanConverter<bool>
    {
        public BooleanNotConverter()
        {
            True = false;
            False = true;
        }
    }
}