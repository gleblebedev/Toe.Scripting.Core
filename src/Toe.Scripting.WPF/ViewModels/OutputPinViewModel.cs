namespace Toe.Scripting.WPF.ViewModels
{
    public class OutputPinViewModel : PinViewModel
    {
        public OutputPinViewModel(NodeViewModel node, Pin pin) : base(node, pin)
        {
        }

        public override bool IsInputPin => false;
        public override bool IsExecutionPin => false;
    }
}