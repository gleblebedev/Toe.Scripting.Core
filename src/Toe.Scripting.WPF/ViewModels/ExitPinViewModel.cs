namespace Toe.Scripting.WPF.ViewModels
{
    public class ExitPinViewModel : PinViewModel
    {
        public ExitPinViewModel(NodeViewModel node, PinWithConnection pin) : base(node, pin)
        {
        }

        public override bool IsInputPin => false;
        public override bool IsExecutionPin => true;
    }
}