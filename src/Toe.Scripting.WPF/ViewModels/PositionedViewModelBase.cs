using System.Windows;

namespace Toe.Scripting.WPF.ViewModels
{
    public class PositionedViewModelBase : ViewModelBase
    {
        private bool _isSelected;
        private Point _position;
        private Size _size;
        private bool _isHidden;

        public bool IsHidden {
            get { return _isHidden; }
            set { RaiseAndSetIfChanged(ref _isHidden, value); }
        }

        public Point Position
        {
            get => _position;
            set
            {
                if (RaiseAndSetIfChanged(ref _position, value))
                    HandlePositionUpdate();
            }
        }

        public Size Size
        {
            get => _size;
            set
            {
                if (RaiseAndSetIfChanged(ref _size, value))
                    HandleSizeUpdate();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }

        protected virtual void HandlePositionUpdate()
        {
        }

        protected virtual void HandleSizeUpdate()
        {
        }

        public Rect GetRect()
        {
            return new Rect(Position, Size);
        }
    }
}