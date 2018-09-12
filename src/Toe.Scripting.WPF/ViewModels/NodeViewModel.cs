using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Toe.Scripting.WPF.ViewModels
{
    public class NodeViewModel : PositionedViewModelBase
    {
        private bool _canRename;
        private GroupViewModel _group;
        private string _name = "Node Name";
        private string _value;
        private string _error;
        private bool _hasGroupPins;

        public NodeViewModel()
        {
        }

        public NodeViewModel(ScriptViewModel script, ScriptNode node)
        {
            Script = script;
            Node = node;
            _name = node.Name;
            _value = node.Value;
            foreach (var pin in node.InputPins) InputPins.Add(new InputPinViewModel(this, pin));
            foreach (var pin in node.OutputPins) OutputPins.Add(new OutputPinViewModel(this, pin));
            foreach (var pin in node.EnterPins) EnterPins.Add(new EnterPinViewModel(this, pin));
            foreach (var pin in node.ExitPins) ExitPins.Add(new ExitPinViewModel(this, pin));
            MenuItems.Add(new MenuItemViewModel {Header = "Copy", Command = CopyThis});
            MenuItems.Add(new MenuItemViewModel { Header = "Duplicate", Command = DuplicateThis });
            MenuItems.Add(new MenuItemViewModel { Header = "Cut", Command =  CutThis});
            MenuItems.Add(new MenuItemViewModel {Header = "Delete", Command = DeleteThis});
            //MenuItems.Add(new MenuItemViewModel { Header = "-" });
            MenuItems.Add(new MenuItemViewModel { Header = "Group", Command = GroupThis });
            MenuItems.Add(new MenuItemViewModel { Header = "Ungroup", Command = UngroupThis });
        }

        public IList<MenuItemViewModel> MenuItems { get; } = new ObservableCollection<MenuItemViewModel>();

        public ScriptNode Node { get; }

        public string Value
        {
            get => _value;
            set
            {
                if (RaiseAndSetIfChanged(ref _value, value)) Script.SetValue(Node, _value);
            }
        }

        public bool HasGroupPins
        {
            get => _hasGroupPins;
            set => RaiseAndSetIfChanged(ref _hasGroupPins , value);
        }

        public bool CanRename
        {
            get => _canRename;
            set => RaiseAndSetIfChanged(ref _canRename, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (RaiseAndSetIfChanged(ref _name, value)) Script.SetName(Node, _name);
            }
        }

        public NodeCategory Category => Node.Category;


        public IList<PinViewModel> InputPins { get; set; } = new ObservableCollection<PinViewModel>();
        public IList<PinViewModel> OutputPins { get; set; } = new ObservableCollection<PinViewModel>();
        public IList<PinViewModel> EnterPins { get; set; } = new ObservableCollection<PinViewModel>();
        public IList<PinViewModel> ExitPins { get; set; } = new ObservableCollection<PinViewModel>();

        public ScriptViewModel Script { get; }

        public int Id => Node.Id;

        public GroupViewModel Group
        {
            get => _group;
            set
            {
                if (_group != value)
                {
                    if (_group != null) _group.Remove(this);

                    _group = value;

                    if (_group != null)
                    {
                        _group.Add(this);
                    }

                    RaisePropertyChanged();
                }
            }
        }

        internal bool SetGroup(GroupViewModel group)
        {
            if (_group != group)
            {
                if (group != null)
                {
                    _group = group;
                    Node.GroupId = group.Uid;
                }
                else
                {
                    _group = null;
                    Node.GroupId = 0;
                }
                RaisePropertyChanged(nameof(Group));
                return true;
            }

            return false;
        }

        public bool HasError
        {
            get { return _error != null; }
        }
        public string Error
        {
            get { return _error; }
            set
            {
                if (RaiseAndSetIfChanged(ref _error, value))
                {
                    RaisePropertyChanged(nameof(HasError));
                    if (value != null)
                    {
                        if (Group != null)
                            Group.ErrorNode = this;
                        if (Script != null)
                            Script.ErrorNode = this;
                    }
                }
            }
        }
        public IEnumerable<PinViewModel> AllPins
        {
            get
            {
                return new IList<PinViewModel>[]{ EnterPins,ExitPins,InputPins,OutputPins }.SelectMany(_=>_);
            }
        }
        public IEnumerable<LinkViewModel> AllLinks
        {
            get
            {
                foreach (var pinViewModel in AllPins)
                {
                    foreach (var linkViewModel in pinViewModel.Connections)
                    {
                        yield return linkViewModel;
                    }
                }
            }
        }
        public IEnumerable<NodeViewModel> AllConnectedInputNodes
        {
            get
            {
                return AllConnectedInputPins.Select(_ => _.Node);
            }
        }

        public IEnumerable<PinViewModel> AllConnectedInputPins
        {
            get
            {
                return AllInputLinks.Select(_ => _.From as PinViewModel).Where(_ => _ != null);
            }
        }
        public IEnumerable<LinkViewModel> AllInputLinks
        {
            get
            {
                foreach (var pinViewModel in new[]{ EnterPins,InputPins }.SelectMany(_ => _))
                {
                    foreach (var linkViewModel in pinViewModel.Connections)
                    {
                        yield return linkViewModel;
                    }
                }
            }
        }

        public IEnumerable<NodeViewModel> AllConnectedOutputNodes
        {
            get
            {
                return AllConnectedOutputPins.Select(_ => _.Node);
            }
        }

        public IEnumerable<PinViewModel> AllConnectedOutputPins
        {
            get
            {
                return AllOutputLinks.Select(_ => _.To as PinViewModel).Where(_ => _ != null);
            }
        }
        public IEnumerable<LinkViewModel> AllOutputLinks
        {
            get
            {
                foreach (var pinViewModel in new[] { ExitPins, OutputPins }.SelectMany(_ => _))
                {
                    foreach (var linkViewModel in pinViewModel.Connections)
                    {
                        yield return linkViewModel;
                    }
                }
            }
        }
        public string Type
        {
            get { return Node?.Type; }
        }

        protected override void HandlePositionUpdate()
        {
            UpdateGroupSize();
        }

        protected override void HandleSizeUpdate()
        {
            UpdateGroupSize();
        }

        private void UpdateGroupSize()
        {
            Group?.UpdateSize();
        }

        private void DeleteThis()
        {
            if (!IsSelected)
                Script.Select(this);
            Script.DeleteSelected();
        }

        private void CopyThis()
        {
            if (!IsSelected)
                Script.Select(this);
            Script.CopySelected();
        }

        private void CutThis() {
            if (!IsSelected)
                Script.Select(this);
            Script.CutSelected();
        }

        private void DuplicateThis()
        {
            if (!IsSelected)
                Script.Select(this);
            Script.DuplicateSelected();
        }
        private void GroupThis()
        {
            if (!IsSelected)
                Script.Select(this);
            Script.GroupSelected();
        }

        private void UngroupThis()
        {
            if (!IsSelected)
                Script.Select(this);
            Script.UngroupSelected();
        }
    }
}