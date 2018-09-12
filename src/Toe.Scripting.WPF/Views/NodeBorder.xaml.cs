﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    ///     Interaction logic for NodeBorder.xaml
    /// </summary>
    public partial class NodeBorder : UserControl
    {
        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register("NodeContent", typeof(object), typeof(NodeBorder),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CanRenameProperty =
            DependencyProperty.Register("CanRename", typeof(bool), typeof(NodeBorder),
                new PropertyMetadata(false));

        private bool _dragged;
        private Canvas _parent;
        private Point _prevPos;
        private bool _wasSelected;

        public NodeBorder()
        {
            InitializeComponent();
            LayoutUpdated += UpdateSize;
        }

        public object NodeContent
        {
            get => GetValue(AdditionalContentProperty);
            set => SetValue(AdditionalContentProperty, value);
        }

        public bool CanRename
        {
            get => (bool) GetValue(CanRenameProperty);
            set => SetValue(CanRenameProperty, value);
        }

        protected NodeViewModel ViewModel => DataContext as NodeViewModel;

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
            {
                ViewModel.Script.StopDragging();
                if (!_wasSelected)
                    ViewModel.Script.RemoveSelection(ViewModel);
            }

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
            var viewModel = ViewModel;
            if (viewModel != null) viewModel.Size = new Size(ActualWidth, ActualHeight);
        }

        private void SelectIfNotSelected(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel != null && !viewModel.IsSelected) viewModel.Script.Select(viewModel);
        }
    }
}