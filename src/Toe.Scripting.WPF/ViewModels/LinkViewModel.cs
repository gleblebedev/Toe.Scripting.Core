using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Toe.Scripting.WPF.Model;

namespace Toe.Scripting.WPF.ViewModels
{
    public class LinkViewModel : PositionedViewModelBase
    {
        public enum LinkState
        {
            Data,
            Execution,
            Selected
        }


        private PositionedViewModelBase _from;
        private ConnectionPoints _points;
        private LinkState _state;
        private PositionedViewModelBase _to;

        public LinkViewModel(ScriptViewModel script, PositionedViewModelBase from, PositionedViewModelBase to,
            PinWithConnection pin, bool isExecution)
        {
            IsExecution = isExecution;
            Script = script;
            HostPin = pin;
            From = from;
            To = to;
            IsExecution = isExecution;
            PropertyChanged += (s, a) => UpdateState();
            MenuItems.Add(new MenuItemViewModel {Header = "Delete", Command = DeleteThis});
        }

        public IList<MenuItemViewModel> MenuItems { get; } = new ObservableCollection<MenuItemViewModel>();

        public ScriptViewModel Script { get; }

        public bool IsExecution { get; }

        public PinWithConnection HostPin { get; }

        public LinkState State
        {
            get => _state;
            set => RaiseAndSetIfChanged(ref _state, value);
        }

        public PositionedViewModelBase From
        {
            get => _from;
            set
            {
                if (_from != value)
                {
                    if (_from != null)
                    {
                        _from.PropertyChanged -= HandlePointsUpdate;
                        var link = _from as PinViewModel;
                        link?.Connections.Remove(this);
                    }

                    _from = value;
                    if (_from != null)
                    {
                        _from.PropertyChanged += HandlePointsUpdate;
                        var link = _from as PinViewModel;
                        link?.Connections.Add(this);
                    }

                    UpdatePoints();
                    RaisePropertyChanged();
                }
            }
        }

        public PositionedViewModelBase To
        {
            get => _to;
            set
            {
                if (_to != value)
                {
                    if (_to != null)
                    {
                        _to.PropertyChanged -= HandlePointsUpdate;
                        var link = _to as PinViewModel;
                        link?.Connections.Remove(this);
                    }

                    _to = value;
                    if (_to != null)
                    {
                        _to.PropertyChanged += HandlePointsUpdate;
                        var link = _to as PinViewModel;
                        link?.Connections.Add(this);
                    }

                    UpdatePoints();
                    RaisePropertyChanged();
                }
            }
        }

        public ConnectionPoints Points
        {
            get => _points;
            set => RaiseAndSetIfChanged(ref _points, value);
        }

        private void DeleteThis()
        {
            if (!Script.HasSelection)
                Script.Select(this);
            Script.DeleteSelected();
        }

        private void UpdateState()
        {
            if (IsSelected)
            {
                State = LinkState.Selected;
                return;
            }

            if (IsExecution)
                State = LinkState.Execution;
            else
                State = LinkState.Data;
        }

        private void HandlePointsUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Position))
                return;
            UpdatePoints();
        }

        private void UpdatePoints()
        {
            if (From == null || To == null)
            {
                Points = null;
                return;
            }

            var p1 = new Point(From.Position.X + From.Size.Width, From.Position.Y + From.Size.Height / 2);
            var p2 = new Point(To.Position.X, To.Position.Y + To.Size.Height / 2);

            Position = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            var max = new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
            Size = new Size(max.X - Position.X, max.Y - Position.Y);

            Points = new ConnectionPoints(
                new Point(p1.X - Position.X, p1.Y - Position.Y),
                new Point(p2.X - Position.X, p2.Y - Position.Y)
            );
        }
    }
}