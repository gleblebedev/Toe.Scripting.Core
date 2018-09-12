namespace Toe.Scripting.WPF.ViewModels
{
    public class UndoActionViewModel : ViewModelBase
    {
        private readonly Script _undoState;

        public UndoActionViewModel(Script scriptState)
        {
            _undoState = scriptState;
        }

        public void Undo(ScriptViewModel script)
        {
            script.SetScript(_undoState.Clone());
        }
    }
}