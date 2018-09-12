namespace Toe.Scripting.WPF.ViewModels
{
    public class InputPinViewModel : PinViewModel
    {
        public InputPinViewModel(NodeViewModel node, PinWithConnection pin) : base(node, pin)
        {
        }

        public override bool IsInputPin => true;
        public override bool IsExecutionPin => false;
    }
}