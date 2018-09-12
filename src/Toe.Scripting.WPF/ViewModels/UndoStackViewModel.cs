using System.Collections.ObjectModel;

namespace Toe.Scripting.WPF.ViewModels
{
    public class UndoStackViewModel : ViewModelBase
    {
        private int _index;

        public ObservableCollection<UndoActionViewModel> Stack { get; set; } =
            new ObservableCollection<UndoActionViewModel>();

        public bool CanUndo => _index > 0;

        public bool CanRedo => _index < Stack.Count-1;

        public void Enqueue(UndoActionViewModel undoActionViewModel)
        {
            while (Stack.Count > _index) Stack.RemoveAt(Stack.Count - 1);
            Stack.Add(undoActionViewModel);
            _index = Stack.Count;
        }

        public void Undo(ScriptViewModel script)
        {
            if (_index > 0)
            {
                if (_index >= Stack.Count)
                {
                    Stack.Add(new UndoActionViewModel(script.Script));
                }
                --_index;
                Stack[_index].Undo(script);
            }
        }

        public void Redo(ScriptViewModel script)
        {
            if (_index < Stack.Count-1)
            {
                ++_index;
                Stack[_index].Undo(script);
            }
        }

        public void Clear()
        {
            Stack.Clear();
            _index = 0;
        }
    }
}