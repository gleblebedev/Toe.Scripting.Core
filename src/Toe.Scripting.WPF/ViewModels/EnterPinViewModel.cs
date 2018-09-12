namespace Toe.Scripting.WPF.ViewModels
{
    public class EnterPinViewModel : PinViewModel
    {
        public EnterPinViewModel(NodeViewModel node, Pin pin) : base(node, pin)
        {
        }

        public override bool IsInputPin => true;
        public override bool IsExecutionPin => true;
    }
}