using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for GroupView.xaml
    /// </summary>
    public partial class GroupView : UserControl
    {
        private bool _dragged;
        private Canvas _parent;
        private Point _prevPos;
        private bool _wasSelected;

        public GroupView()
        {
            InitializeComponent();
        }

        protected GroupViewModel ViewModel => DataContext as GroupViewModel;

        protected void StartDragging(object sender, MouseButtonEventArgs e)
        {
            ViewUtils.FindFocusableParent(this)?.Focus();
            if (ViewModel == null)
                return;
            _wasSelected = ViewModel.IsSelected;
            if (!_wasSelected)
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                    ViewModel.Script.AddToSelection(ViewModel);
                else
                    ViewModel.Script.Select(ViewModel);

            _dragged = false;
            _parent = _parent??ViewUtils.FindCanvasParent(this);
            _prevPos = e.GetPosition(_parent);
            var uiElement = ((UIElement) sender);
            uiElement.CaptureMouse();
            uiElement.MouseMove += Drag;
            uiElement.MouseLeftButtonUp += StopDragging;
            uiElement.LostMouseCapture += StopDragging;
            e.Handled = true;
        }

        protected void StopDragging(object sender, MouseEventArgs e)
        {
            if (_dragged)
            {
                if (!_wasSelected)
                    ViewModel.Script.RemoveSelection(ViewModel);
                ViewModel.Script.StopDragging();
            }
            var uiElement = ((UIElement)sender);
            uiElement.MouseMove -= Drag;
            uiElement.MouseLeftButtonUp -= StopDragging;
            uiElement.LostMouseCapture -= StopDragging;
            uiElement.ReleaseMouseCapture();
            e.Handled = true;
        }


        protected void Drag(object sender, MouseEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel == null)
            {
                return;
            }

            if (!_dragged)
            {
                _dragged = true;
                viewModel.Script.StartDragging();
            }

            var pos = e.GetPosition(_parent);
            viewModel.Script.MoveSelectedNodes(pos - _prevPos);
            _prevPos = pos;
        }
    }
}