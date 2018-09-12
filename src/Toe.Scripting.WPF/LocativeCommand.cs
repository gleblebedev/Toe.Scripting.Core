using System;
using System.Windows;
using System.Windows.Input;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF
{
    public class LocativeCommand : ICommand
    {
        private readonly Action<Point> _action;
        private readonly MouseViewModel _mouse;
        private bool _canExecute;

        public LocativeCommand(MouseViewModel mouse, Action<Point> action)
        {
            _mouse = mouse;
            _action = action;
        }

        public bool CanExecute
        {
            get => _canExecute;
            set
            {
                if (_canExecute != value)
                {
                    _canExecute = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            _action(_mouse.Location);
        }

        public event EventHandler CanExecuteChanged;
    }
}