using System.Windows.Controls;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for ExitExecutionPin.xaml
    /// </summary>
    public partial class ExitExecutionPin : PinControl
    {
        public ExitExecutionPin()
        {
            InitializeComponent();
        }

        protected override Border Pin => _pin;
    }
}