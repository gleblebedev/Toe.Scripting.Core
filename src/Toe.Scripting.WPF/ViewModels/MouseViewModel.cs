using System.Windows;

namespace Toe.Scripting.WPF.ViewModels
{
    public class MouseViewModel : ViewModelBase
    {
        private Point _location;

        public Point Location
        {
            get => _location;
            set => RaiseAndSetIfChanged(ref _location, value);
        }
    }
}