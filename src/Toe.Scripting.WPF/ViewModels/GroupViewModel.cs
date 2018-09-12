using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Toe.Scripting.WPF.ViewModels
{
    public class GroupViewModel : PositionedViewModelBase
    {
        private readonly ObservableCollection<NodeViewModel> _items =
            new ObservableCollection<NodeViewModel>();

        public Thickness Margin = new Thickness(10, 20, 10, 10);
        private ICollection<PinViewModel> _enterPins;
        private ICollection<PinViewModel> _exitPins;
        private HashSet<int> _innerNodes = new HashSet<int>();
        private NodeViewModel _errorNode;

        public GroupViewModel(ScriptViewModel script, NodeGroup nodeGroup)
        {
            Script = script;
            NodeGroup = nodeGroup;
            CollapseOrExpandCommand = new ScriptingCommand(CollapseOrExpand);
            if (nodeGroup.IsCollapsed)
                Collapse();
            else
                Expand();
        }

        private void CollapseOrExpand()
        {
            IsCollapsed = !IsCollapsed;
        }

        public ICollection<NodeViewModel> Nodes => _items;

        public ICollection<PinViewModel> EnterPins
        {
            get => _enterPins;
            set => RaiseAndSetIfChanged(ref _enterPins, value);
        }
        public ICollection<PinViewModel> ExitPins
        {
            get => _exitPins;
            set => RaiseAndSetIfChanged(ref _exitPins, value);
        }

        public ScriptViewModel Script { get; }

        public NodeGroup NodeGroup { get; }

        public string Name
        {
            get => NodeGroup.Name;
            set
            {
                if (NodeGroup.Name != value) Script.SetGroupName(NodeGroup, value);
            }
        }

        public bool IsCollapsed
        {
            get => NodeGroup.IsCollapsed;
            set
            {
                if (NodeGroup.IsCollapsed != value)
                {
                    NodeGroup.IsCollapsed = value;
                    if (NodeGroup.IsCollapsed)
                        Collapse();
                    else
                        Expand();
                    UpdateSize();
                }
            }
        }

        public ICommand CollapseOrExpandCommand { get; set; }

        public int Uid
        {
            get { return NodeGroup.Id; }
        }

        public NodeViewModel ErrorNode
        {
            get { return _errorNode; }
            set { RaiseAndSetIfChanged(ref _errorNode, value); }
        }

        public void UpdateSize()
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            foreach (var node in Nodes)
            {
                if (node.HasError)
                    ErrorNode = node;
                if (minX > node.Position.X) minX = node.Position.X;
                if (maxX < node.Position.X + node.Size.Width) maxX = node.Position.X + node.Size.Width;
                if (minY > node.Position.Y) minY = node.Position.Y;
                if (maxY < node.Position.Y + node.Size.Height) maxY = node.Position.Y + node.Size.Height;
            }

            if (minX > maxX)
                return;
            Position = new Point(minX - Margin.Left, minY - Margin.Top);
            if (IsCollapsed)
            {
                Size = new Size(Margin.Left + Margin.Right, Margin.Bottom + Margin.Top);
            }
            else
            {
                Size = new Size(maxX - minX + Margin.Left + Margin.Right, maxY - minY + Margin.Bottom + Margin.Top);
            }
        }

        public void Remove(NodeViewModel node)
        {
            if (node.SetGroup(null))
                Nodes.Remove(node);
            if (ErrorNode == node)
                ErrorNode = null;
            node.IsHidden = false;
            RefreshPins();
            UpdateSize();
        }
        public void RemoveRange(IEnumerable<NodeViewModel> nodeRange)
        {
            foreach (var node in nodeRange)
            {
                if (node.SetGroup(null))
                    Nodes.Remove(node);
                node.IsHidden = false;
            }
            RefreshPins();
            UpdateSize();
        }
        public void Add(NodeViewModel node)
        {
            if (node.SetGroup(this))
                Nodes.Add(node);
            if (IsCollapsed)
            {
                node.IsHidden = IsCollapsed;
            }
            RefreshPins();
            UpdateSize();
        }
 

        public void AddRange(IEnumerable<NodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.SetGroup(this))
                    Nodes.Add(node);
                if (IsCollapsed)
                {
                    node.IsHidden = IsCollapsed;
                }
            }
            RefreshPins();
            UpdateSize();
        }

        private void Collapse()
        {
            _innerNodes.Clear();
            var enterPins = new HashSet<PinViewModel>();
            var exitPins = new HashSet<PinViewModel>();
            foreach (var node in Nodes)
            {
                node.IsHidden = true;
                _innerNodes.Add(node.Id);
                foreach (var pin in node.AllPins)
                {
                    pin.UseNodeName = false;
                }
            }

            foreach (var link in Nodes.SelectMany(_=>_.AllLinks))
            {
                link.IsHidden = true;
            }
            foreach (var node in Nodes)
            {
                if (node.HasGroupPins)
                {
                    CollapseAllInternalPins(node, enterPins, exitPins);
                }
                else
                {
                    KeepAllExternalPins(node, enterPins, exitPins);
                }
            }

            EnterPins = enterPins.ToList();
            ExitPins = exitPins.ToList();
        }

        private void CollapseAllInternalPins(NodeViewModel node, HashSet<PinViewModel> enterPins, HashSet<PinViewModel> exitPins)
        {
            foreach (var pin in node.AllPins)
            {
                CollapseInternalPin(pin, enterPins, exitPins);
            }
        }
        private void KeepAllExternalPins(NodeViewModel node, HashSet<PinViewModel> enterPins, HashSet<PinViewModel> exitPins)
        {
            foreach (var pin in node.AllPins)
            {
                KeepExternalPin(pin, enterPins, exitPins);
            }
        }
        private bool IsInternalLink(PinViewModel pin, LinkViewModel link)
        {
            var other = (link.From == pin) ? link.To : link.From;
            var otherPin = other as PinViewModel;
            if (otherPin != null)
            {
                return _innerNodes.Contains(otherPin.Node.Id);
            }
            return false;
        }

        private void Keep(PinViewModel pin, HashSet<PinViewModel> enterPins, HashSet<PinViewModel> exitPins)
        {
            if (pin.IsInputPin)
                enterPins.Add(pin);
            else
                exitPins.Add(pin);
            if (pin.Node.HasGroupPins)
                pin.UseNodeName = true;
            foreach (var link in pin.Connections)
            {
                if (link.From == pin)
                {
                    if (_innerNodes.Contains(((PinViewModel)link.To).Node.Id))
                        continue;
                }
                else
                {
                    if (_innerNodes.Contains(((PinViewModel)link.From).Node.Id))
                        continue;
                }

                link.IsHidden = false;
            }
        }
        private void KeepExternalPin(PinViewModel pin, HashSet<PinViewModel> enterPins, HashSet<PinViewModel> exitPins)
        {
            if (pin.Connections.Count != 0)
            {
                if (pin.Connections.Any(_ => !IsInternalLink(pin, _)))
                {
                    Keep(pin, enterPins, exitPins);
                }
            }
        }
        private void CollapseInternalPin(PinViewModel pin, HashSet<PinViewModel> enterPins, HashSet<PinViewModel> exitPins)
        {
            if (pin.Connections.Count != 0)
            {
                if (pin.Connections.All(_ => IsInternalLink(pin, _)))
                {
                    return;
                }
            }

            Keep(pin, enterPins, exitPins);
        }
 
        private void Expand()
        {
            foreach (var node in Nodes)
            {
                node.IsHidden = false;
                foreach (var pin in node.AllPins)
                {
                    pin.UseNodeName = false;
                    foreach (var connection in pin.Connections)
                    {
                        connection.IsHidden = false;
                    }
                }
            }
            EnterPins = null;
            ExitPins = null;
        }

        public void RefreshPins()
        {
            if (IsCollapsed) Collapse();
        }

    }
}