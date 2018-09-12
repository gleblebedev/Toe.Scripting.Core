using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for ConnectorView.xaml
    /// </summary>
    public partial class ConnectorView : UserControl
    {
        private bool _dragged;
        private Canvas _parent;
        private Point _prevPos;
        private bool _wasSelected;

        public ConnectorView()
        {
            InitializeComponent();
            LayoutUpdated += UpdateSize;
        }


        protected NodeViewModel ViewModel => DataContext as NodeViewModel;

        protected void StartDragging(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null)
                return;
            _wasSelected = ViewModel.IsSelected;
            if (!_wasSelected)
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                    ViewModel.Script.AddToSelection(ViewModel);
                else
                    ViewModel.Script.Select(ViewModel);

            _dragged = false;
            _parent = _parent?? ViewUtils.FindCanvasParent(this);
            _prevPos = e.GetPosition(_parent);
            ((UIElement) sender).CaptureMouse();
            ((UIElement) sender).MouseMove += Drag;
            ((UIElement) sender).MouseLeftButtonUp += StopDragging;
            e.Handled = true;
        }

        protected void StopDragging(object sender, MouseButtonEventArgs e)
        {
            if (_dragged)
                if (!_wasSelected)
                    ViewModel.Script.RemoveSelection(ViewModel);
            ((UIElement) sender).MouseMove -= Drag;
            ((UIElement) sender).MouseLeftButtonUp -= StopDragging;
            ((UIElement) sender).ReleaseMouseCapture();
            e.Handled = true;
        }


        protected void Drag(object sender, MouseEventArgs e)
        {
            if (!_dragged)
            {
                _dragged = true;
                ViewModel.Script.StartDragging();
            }

            var pos = e.GetPosition(_parent);
            ViewModel.Script.MoveSelectedNodes(pos - _prevPos);
            _prevPos = pos;
        }

        protected void UpdateSize(object sender, EventArgs e)
        {
            var vm = DataContext as PositionedViewModelBase;
            if (vm != null) vm.Size = new Size(ActualWidth, ActualHeight);
        }
    }
}