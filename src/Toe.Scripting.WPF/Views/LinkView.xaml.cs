using System;
using System.Windows.Controls;
using System.Windows.Input;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for LinkView.xaml
    /// </summary>
    public partial class LinkView : UserControl
    {
        public LinkView()
        {
            InitializeComponent();
        }

        protected LinkViewModel ViewModel => DataContext as LinkViewModel;

        private void UpdatePathLayout(object sender, EventArgs e)
        {
            //var vm = (LinkViewModel)DataContext;
            //vm.Size = new Size(ActualWidth, ActualHeight);
        }

        private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = ViewModel;
            if (!vm.IsSelected)
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                    vm.Script.AddToSelection(vm);
                else
                    vm.Script.Select(vm);
            e.Handled = true;
        }

        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SelectIfNotSelected(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel != null && !viewModel.IsSelected) viewModel.Script.Select(viewModel);
        }
    }
}