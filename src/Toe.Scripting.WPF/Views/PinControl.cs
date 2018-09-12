using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Views
{
    public abstract class PinControl : UserControl
    {
        private Canvas _canvas;
        private LinkViewModel _newLink;

        protected abstract Border Pin { get; }

        protected Canvas Canvas
        {
            get { return _canvas ?? (_canvas = ViewUtils.FindCanvasParent(this)); }
        }

        public PinControl()
        {
            LayoutUpdated += HandleLayoutUpdated;
        }

        public PinViewModel ViewModel => DataContext as PinViewModel;

        private IEnumerable<UIElement> Parents
        {
            get
            {
                UIElement current = this;
                for (;;)
                {
                    current = VisualTreeHelper.GetParent(current) as UIElement;
                    if (current == null)
                        break;
                    yield return current;
                }
            }
        }

        protected void HandleLayoutUpdated(object sender, EventArgs e)
        {
            //Debug.WriteLine("LayoutUpdated "+RuntimeHelpers.GetHashCode(this));
            UpdatePositionAndSize();
        }

        protected void UpdatePositionAndSize()
        {
            var vm = ViewModel;
            if (vm != null && Pin != null && !vm.Node.Script.IsPanning)
            {
                if (Parents.Any(_=>_.Visibility != Visibility.Visible))
                    return;
                ViewModel.Size = new Size(Pin.ActualWidth, Pin.ActualHeight);
                var canvas = Canvas;
                if (canvas == null)
                    return;
                var point = Pin.TranslatePoint(new Point(0, 0), canvas);
                vm.Position = point;
            }
        }

        public void StartConnection(object sender, MouseButtonEventArgs e)
        {
            ViewUtils.FindFocusableParent(this)?.Focus();
            if (ViewModel == null)
                return;
            CaptureMouse();
            LostMouseCapture += StopConnection;
            MouseUp += StopConnection;
            MouseMove += UpdateConnection;
            _newLink = new LinkViewModel(ViewModel.Node.Script, new PositionedViewModelBase
                {
                    Position = new Point(ViewModel.Position.X + ViewModel.Size.Width,
                        ViewModel.Position.Y + ViewModel.Size.Height / 2)
                }, new PositionedViewModelBase
                {
                    Position = new Point(ViewModel.Position.X, ViewModel.Position.Y + ViewModel.Size.Height / 2)
                }
                , null,
                ViewModel.IsExecutionPin) {IsSelected = true};
            ViewModel.Node.Script.Items.Add(_newLink);
            ViewModel.StartConnection();
            e.Handled = true;
        }

        private void UpdateConnection(object sender, MouseEventArgs e)
        {
            var canvas = Canvas;
            if (canvas == null)
                return;
            var point = e.GetPosition(canvas);
            if (ViewModel.IsInputPin)
                _newLink.From.Position = point;
            else
                _newLink.To.Position = point;
        }

        private void StopConnection(object sender, MouseEventArgs e)
        {
            if (_newLink == null)
                return;
            PinViewModel fromPin = null;
            PinViewModel toPin = null;
            var pos = ViewModel.IsInputPin ? _newLink.From.Position : _newLink.To.Position;
            foreach (var pin in ViewModel.Node.Script.SelectedPins)
            {
                var rect = new Rect(pin.Position, pin.Size);
                if (rect.Contains(pos))
                {
                    if (ViewModel.IsInputPin)
                    {
                        fromPin = pin;
                        toPin = ViewModel;
                    }

                    else
                    {
                        toPin = pin;
                        fromPin = ViewModel;
                    }

                    ViewModel.Node.Script.SearchPinFilter = null;
                    goto done;
                }
            }

            //if no pin 
            ViewModel.CreateNodeMenu(e.GetPosition(Canvas));

            done:;
            MouseMove -= UpdateConnection;
            MouseUp -= StopConnection;
            LostMouseCapture -= StopConnection;
            ReleaseMouseCapture();
            ViewModel.Node.Script.Items.Remove(_newLink);
            ViewModel.Node.Script.Connect(fromPin, toPin, ViewModel.IsExecutionPin);
            _newLink = null;
        }
    }
}