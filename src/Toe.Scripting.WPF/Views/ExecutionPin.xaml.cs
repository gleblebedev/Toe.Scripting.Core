using System.Windows.Controls;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for ExecutionPin.xaml
    /// </summary>
    public partial class ExecutionPin : PinControl
    {
        public ExecutionPin()
        {
            InitializeComponent();
        }

        protected override Border Pin => _pin;
    }
}