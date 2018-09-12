using System.Windows.Controls;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for PinView.xaml
    /// </summary>
    public partial class PinView : PinControl
    {
        public PinView()
        {
            InitializeComponent();
        }

        protected override Border Pin => _pin;
    }
}