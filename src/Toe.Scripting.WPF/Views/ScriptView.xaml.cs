using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for ScriptView.xaml
    /// </summary>
    public partial class ScriptView : UserControl
    {
        private Vector _panDistance;
        private bool _isSelecting;
        private Point _mousePosition;
        private Point _selectionStartPos;

        public ScriptView()
        {
            DataContextChanged += Update;
            InitializeComponent();
            _contextMenu.Closed += MenuClosed;
            _contextMenu.Opened += MenuOpened;
            Loaded += (s, a) => SetFocus();
        }


        public ScriptViewModel ViewModel
        {
            get => DataContext as ScriptViewModel;
            set => DataContext = value;
        }

        private void MenuOpened(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel != null) viewModel.IsContextMenuOpen = true;
            _searchBar.Focus();
            if (_searchBar.Text != null)
                _searchBar.Select(0, _searchBar.Text.Length);
        }

        private void MenuClosed(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel != null)
            {
                viewModel.IsContextMenuOpen = false;
                viewModel.SearchPinFilter = null;
            }

            SetFocus();
        }

        private void SetFocus()
        {
            Focusable = true;
            Focus();
        }

        private void Update(object sender, DependencyPropertyChangedEventArgs e)
        {
            MenuItem itemPaste = new MenuItem { Header = "Paste here", Command= ViewModel.PasteCommand };
            MenuItem itemAddPreset = new MenuItem { Header = "Add Preset" };

            var lastItems = new List<MenuItem>() {itemPaste };

            var viewModel = ViewModel;
            if (viewModel == null)
                return;
            viewModel.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == "FilteredFactories")  BuildMenu(lastItems);

                if (a.PropertyName == nameof(viewModel.IsContextMenuOpen))
                {
                    if (viewModel.IsContextMenuOpen)
                    {
                        _contextMenu.DataContext = viewModel;
                    }
                    _contextMenu.IsOpen = viewModel.IsContextMenuOpen;
                }

                itemPaste.IsEnabled = ViewModel.CanPaste();
            };
            BuildMenu(lastItems);
        }

        private void BuildMenu(List<MenuItem> lastItems)
        {
            while (_contextMenu.Items.Count > 1) _contextMenu.Items.RemoveAt(_contextMenu.Items.Count - 1);
            var viewModel = ViewModel;
            if (viewModel == null)
                return;
            BuildMenu(_contextMenu.Items, viewModel.FilteredFactories);

            if (viewModel.SearchPinFilter == null) {
                _contextMenu.Items.Add(new Separator());
                foreach (var item in lastItems)
                {
                    _contextMenu.Items.Add(item);
                }
            }
        }


        private void BuildMenu(ItemCollection contextMenu, FactoryCategoryViewModel viewModelFactories)
        {
            if (viewModelFactories == null)
                return;
            foreach (var factoryCategoryViewModel in viewModelFactories.Categories)
            {
                var item = new MenuItem {Header = factoryCategoryViewModel.Name};
                contextMenu.Add(item);
                BuildMenu(item.Items, factoryCategoryViewModel);
            }

            foreach (var factoryCategoryViewModel in viewModelFactories.Factories)
            {
                var item = new MenuItem {Header = factoryCategoryViewModel.Name};
                contextMenu.Add(item);
                item.Command = factoryCategoryViewModel.CreateCommand;
            }
        }

        private void StartPanning(object sender, MouseButtonEventArgs e)
        {
            ViewUtils.FindFocusableParent(this)?.Focus();
            _mousePosition = e.MouseDevice.GetPosition(_outerCanvas);
            ViewModel.IsPanning = true;
            _panDistance = new Vector(0,0);
        }

        private void StopPanning(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsPanning = false;
            if (_panDistance.Length > 2)
            {
                e.Handled = true;
            }
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            var vm = ViewModel;
            if (vm == null)
                return;
            {
                var pos = e.MouseDevice.GetPosition(_outerCanvas);
                if (ViewModel.IsPanning)
                {
                    vm.Position = new Point(vm.Position.X + pos.X - _mousePosition.X,
                        vm.Position.Y + pos.Y - _mousePosition.Y);
                    _panDistance += Vector.Subtract(new Vector(_mousePosition.X, _mousePosition.Y), new Vector(pos.X, pos.Y));
                }
                _mousePosition = pos;
            }

            if (_isSelecting)
            {
                var pos = e.GetPosition((IInputElement) _selectionRect.Parent);
                var topLeft = new Point(Math.Min(pos.X, _selectionStartPos.X), Math.Min(pos.Y, _selectionStartPos.Y));
                var bottomRight = new Point(Math.Max(pos.X, _selectionStartPos.X),
                    Math.Max(pos.Y, _selectionStartPos.Y));
                Canvas.SetLeft(_selectionRect, topLeft.X);
                Canvas.SetTop(_selectionRect, topLeft.Y);
                _selectionRect.Width = bottomRight.X - topLeft.X;
                _selectionRect.Height = bottomRight.Y - topLeft.Y;
            }

            if (_innerCanvas != null)
            {
                ViewModel.Mouse.Location = e.MouseDevice.GetPosition(_innerCanvas);
                var absPos = e.GetPosition(null);
                _contextMenu.PlacementRectangle = new Rect(absPos.X, absPos.Y, 0, 0);
            }
        }

        private void RearrangeIfNeeded(object sender, EventArgs e)
        {
            var vm = ViewModel;
            if (vm != null)
                if (vm.NeedsArragement)
                    vm.Rearrange();
        }

        private void StopMultiselect(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel == null)
                return;
            _isSelecting = false;
            if (_selectionRect.Width == 0 && _selectionRect.Height == 0)
            {
                viewModel.ClearSelection();
            }
            else
            {
                var leftTop = new Point(Canvas.GetLeft(_selectionRect) - Canvas.GetLeft(_innerCanvas),
                    Canvas.GetTop(_selectionRect) - Canvas.GetTop(_innerCanvas));
                var bottomRight = new Point(leftTop.X + _selectionRect.Width, leftTop.Y + _selectionRect.Height);

                var m = _scale.Matrix;
                m.Invert();
                var p1 = m.Transform(leftTop);
                var p2 = m.Transform(bottomRight);

                var rect = new Rect(p1, p2);

                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                    viewModel.AddToSelection(rect);
                else
                    viewModel.Select(rect);
            }

            _selectionRect.Visibility = Visibility.Collapsed;
        }

        private void StartSelection(object sender, MouseButtonEventArgs e)
        {
            ViewUtils.FindFocusableParent(this)?.Focus();
            _isSelecting = true;
            _selectionRect.Visibility = Visibility.Visible;
            _selectionStartPos = e.GetPosition((IInputElement) _selectionRect.Parent);
            Canvas.SetLeft(_selectionRect, _selectionStartPos.X);
            Canvas.SetTop(_selectionRect, _selectionStartPos.Y);
            _selectionRect.Width = 0;
            _selectionRect.Height = 0;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ViewModel.DeleteSelected();
                e.Handled = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    if (ViewModel.HasSelection)
                    {
                        ViewModel.CopySelected();
                        e.Handled = true;
                        return;
                    }
                }

                if (e.Key == Key.X)
                {
                    if (ViewModel.HasSelection)
                    {
                        ViewModel.CutSelected();
                        e.Handled = true;
                        return;
                    }
                }

                if (e.Key == Key.V)
                {
                    ViewModel.Paste();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Z)
                {
                    if (ViewModel.UndoCommand.CanExecute)
                    {
                        ViewModel.UndoCommand.Execute(null);
                        e.Handled = true;
                    }
                    return;
                }

            }

            OnPreviewKeyDown(e);
        }

        private void Zoom(object sender, MouseWheelEventArgs e)
        {
            const float ScaleFactor = 1.2f;
            Point p = e.GetPosition(_innerCanvas);

            System.Windows.Media.Matrix m = _innerCanvas.RenderTransform.Value;
            if (e.Delta > 0)
                m.ScaleAtPrepend(ScaleFactor, ScaleFactor, p.X, p.Y);
            else
                m.ScaleAtPrepend(1 / ScaleFactor, 1 / ScaleFactor, p.X, p.Y);

            ViewModel.ScaleMatrix = m;
          
            e.Handled = true;
        }
    }
}