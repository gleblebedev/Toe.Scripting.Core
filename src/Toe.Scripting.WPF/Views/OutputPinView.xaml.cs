using System.Windows.Controls;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for OutputPinView.xaml
    /// </summary>
    public partial class OutputPinView : PinControl
    {
        public OutputPinView()
        {
            InitializeComponent();
        }

        protected override Border Pin => _pin;
    }
}