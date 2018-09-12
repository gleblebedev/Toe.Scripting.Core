using System;
using System.Windows;
using System.Windows.Input;

namespace Toe.Scripting.WPF
{
    public class ScriptingCommand : DependencyObject, ICommand
    {
        public static readonly DependencyProperty CanExecuteProperty = DependencyProperty.Register(
            nameof(CanExecute),
            typeof(bool),
            typeof(ScriptingCommand), new PropertyMetadata(true, HandleCanExecuteChanged)
        );

        private readonly Action _action;

        public ScriptingCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute
        {
            get => Convert.ToBoolean(GetValue(CanExecuteProperty));
            set => SetValue(CanExecuteProperty, value);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return Convert.ToBoolean(GetValue(CanExecuteProperty));
        }

        public virtual void Execute(object parameter)
        {
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;

        private static void HandleCanExecuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScriptingCommand) d).CanExecuteChanged?.Invoke(d, EventArgs.Empty);
        }
    }

    public class ScriptingCommand<T> : DependencyObject, ICommand
    {
        public static readonly DependencyProperty CanExecuteProperty = DependencyProperty.Register(
            nameof(CanExecute),
            typeof(bool),
            typeof(ScriptingCommand<T>), new PropertyMetadata(true, HandleCanExecuteChanged)
        );

        private readonly Action<T> _action;

        public ScriptingCommand(Action<T> action)
        {
            _action = action;
        }


        public bool CanExecute
        {
            get => Convert.ToBoolean(GetValue(CanExecuteProperty));
            set => SetValue(CanExecuteProperty, value);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return Convert.ToBoolean(GetValue(CanExecuteProperty));
        }

        public virtual void Execute(object parameter)
        {
            try
            {
                _action((T)parameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;

        private static void HandleCanExecuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScriptingCommand<T>) d).CanExecuteChanged?.Invoke(d, EventArgs.Empty);
        }

        //public event EventHandler CanExecuteChanged
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}
    }
}