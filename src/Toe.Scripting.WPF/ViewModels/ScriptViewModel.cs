using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Toe.Scripting.WPF.ViewModels
{
    public class ScriptViewModel : PositionedViewModelBase
    {
        private readonly ObservableCollection<PositionedViewModelBase> _items =
            new ObservableCollection<PositionedViewModelBase>();

        protected readonly INodeRegistry _registry;
        private readonly INodeViewModelFactory _viewModelFactory;
        private FactoryCategoryViewModel _factories;
        private FactoryCategoryViewModel _filteredFactories;
        private bool _isContextMenuOpen;
        private Matrix _scaleMatrix = Matrix.Identity;

        protected Script _script = new Script();
        private SearchPinFilter _searchPinFilter;
        private string _searchQuery;
        private bool _hasUnsavedChanged;
        private NodeViewModel _errorNode;
        private bool _isPanning;

        public ScriptViewModel(INodeRegistry registry = null, INodeViewModelFactory viewModelFactory = null,
            Script script = null)
        {
            _registry = registry ?? EmptyNodeRegistry.Instance;
            _viewModelFactory = viewModelFactory ?? new NodeViewModelFactory(_registry);
            GroupSelectedCommand = new ScriptingCommand(() => GroupSelected()) { CanExecute = false };
            UngroupSelectedCommand = new ScriptingCommand(UngroupSelected) { CanExecute = false };
            CopySelectedCommand = new ScriptingCommand(CopySelected) { CanExecute = false };
            CutSelectedCommand = new ScriptingCommand(CutSelected) { CanExecute = false };
            DuplicateSelectedCommand = new ScriptingCommand(DuplicateSelected) { CanExecute = false };
            PasteCommand = new ScriptingCommand(() => Paste()) { CanExecute = true };
            UndoCommand = new ScriptingCommand(Undo) {CanExecute = false};
            RedoCommand = new ScriptingCommand(Redo) {CanExecute = false};
            //BindingOperations.SetBinding(UndoCommand, ScriptingCommand.CanExecuteProperty, new Binding
            //{
            //    Path = new PropertyPath(nameof(UndoStackViewModel.CanUndo)),
            //    Source = UndoStack
            //});
            //BindingOperations.SetBinding(RedoCommand, ScriptingCommand.CanExecuteProperty, new Binding
            //{
            //    Path = new PropertyPath(nameof(UndoStackViewModel.CanRedo)),
            //    Source = UndoStack
            //});
            SelectAllCommand = new ScriptingCommand(SelectAll);
            DeleteSelectedCommand = new ScriptingCommand(DeleteSelected);
            ResetZoomCommand = new ScriptingCommand(ResetZoom);
            if (script != null)
                SetScript(script);
        }

        public bool HasUnsavedChanged
        {
            get => _hasUnsavedChanged;
            set => RaiseAndSetIfChanged(ref _hasUnsavedChanged, value);
        }

        public ScriptingCommand GroupSelectedCommand { get; }
        public ScriptingCommand UngroupSelectedCommand { get; }
        public ScriptingCommand DuplicateSelectedCommand { get; }

        public ScriptingCommand CutSelectedCommand { get; }

        public ScriptingCommand PasteCommand { get; }

        public ScriptingCommand CopySelectedCommand { get; }

        public ICommand SelectAllCommand { get; set; }
        public ScriptingCommand UndoCommand { get; set; }

        public ScriptingCommand RedoCommand { get; set; }


        public ICommand DeleteSelectedCommand { get; set; }

        public ICommand ResetZoomCommand { get; set; }

        public INodeRegistry Registry => _registry;

        public FactoryCategoryViewModel Factories
        {
            get
            {
                if (_factories != null)
                    return _factories;
                var all = Registry.Where(_ => _.Visibility == NodeFactoryVisibility.Visible && _.Name != null);
                _factories = BuildCategory("", all, 0);
                return _factories;
            }
        }

        public ICollection<PositionedViewModelBase> Items => _items;

        public bool NeedsArragement { get; set; }
        public MouseViewModel Mouse { get; set; } = new MouseViewModel();

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (RaiseAndSetIfChanged(ref _searchQuery, value))
                {
                    Debug.WriteLine("new query "+value);
                    UpdateSearchResult();
                }
                else
                {
                    Debug.WriteLine("same query " + value);
                }
            }
        }

        public IReadOnlyList<PositionedViewModelBase> GetSelectedItems()
        {
            return SelectedItems.ToArray();
        }
        public int NumSelectedItems
        {
            get { return SelectedItems.Count; }
        }

        protected HashSet<PositionedViewModelBase> SelectedItems { get; private set; } = new HashSet<PositionedViewModelBase>();

        protected IEnumerable<NodeViewModel> SelectedNodes
        {
            get { return SelectedItems.Select(_ => _ as NodeViewModel).Where(_ => _ != null); }
        }

        protected IEnumerable<NodeViewModel> SelectedNodesIncludingGroups
        {
            get
            {
                var visitedNodes = new HashSet<int>();
                foreach (var item in SelectedItems)
                {
                    var group = item as GroupViewModel;
                    if (group != null)
                        foreach (var nodeInGroup in group.Nodes)
                            if (visitedNodes.Add(nodeInGroup.Id))
                                yield return nodeInGroup;
                    var node = item as NodeViewModel;
                    if (node != null)
                        if (visitedNodes.Add(node.Id))
                            yield return node;
                }
            }
        }

        protected IEnumerable<LinkViewModel> SelectedLinks
        {
            get { return SelectedItems.Select(_ => _ as LinkViewModel).Where(_ => _ != null); }
        }

        protected IEnumerable<GroupViewModel> SelectedGroups
        {
            get { return SelectedItems.Select(_ => _ as GroupViewModel).Where(_ => _ != null); }
        }


        public Script Script
        {
            get => _script;
            set
            {
                if (_script != value)
                {
                    CreateUndoSnapshot();
                    SetScript(value);
                }
            }
        }

        public UndoStackViewModel UndoStack { get; } = new UndoStackViewModel();

        public Matrix ScaleMatrix
        {
            get => _scaleMatrix;
            set => RaiseAndSetIfChanged(ref _scaleMatrix, value);
        }

        public bool IsContextMenuOpen
        {
            get => _isContextMenuOpen;
            set
            {
                if (RaiseAndSetIfChanged(ref _isContextMenuOpen, value))
                    if (_isContextMenuOpen == false)
                        SearchPinFilter = null;
                    else
                        CreateLocation = Mouse.Location;
            }
        }

        public Point CreateLocation { get; set; }

        public FactoryCategoryViewModel FilteredFactories
        {
            get
            {
                if (_filteredFactories == null) _filteredFactories = ApplyFilter(Factories);
                return _filteredFactories;
            }
            set => RaiseAndSetIfChanged(ref _filteredFactories, value);
        }

        public SearchPinFilter SearchPinFilter
        {
            get => _searchPinFilter;
            set
            {
                //if (RaiseAndSetIfChanged(ref _searchPinFilter, value))
                if (_searchPinFilter != value)
                {
                    Debug.WriteLine("New pin filter: " + value);
                    _searchPinFilter = value;
                    RaisePropertyChanged();
                    UpdateSearchResult();
                    UpdateSelectedPins();
                }

                else
                {
                    Debug.WriteLine("The same pin filter: "+_searchPinFilter);
                }
            }
        }

        public IList<PinViewModel> SelectedPins { get; set; } = new ObservableCollection<PinViewModel>();

        public IEnumerable<NodeViewModel> Nodes
        {
            get { return Items.Select(_ => _ as NodeViewModel).Where(_ => _ != null); }
        }
        public IEnumerable<GroupViewModel> Groups
        {
            get { return Items.Select(_ => _ as GroupViewModel).Where(_ => _ != null); }
        }
        public bool HasSelection => SelectedItems.Count > 0;

        public NodeViewModel SelectedNode
        {
            get
            {
                using (var e = SelectedNodes.GetEnumerator())
                {
                    if (!e.MoveNext())
                        return null;
                    var res = e.Current;
                    if (e.MoveNext())
                        return null;
                    return res;
                }
            }
        }

        public NodeViewModel ErrorNode
        {
            get { return _errorNode; }
            set { RaiseAndSetIfChanged(ref _errorNode, value); }
        }

        public bool IsPanning
        {
            get { return _isPanning; }
            set { RaiseAndSetIfChanged(ref _isPanning, value); }
        }


        public bool CanPaste()
        {
            return Clipboard.ContainsData("Toe.Scripting.Script");
        }

        public bool Paste()
        {
            var data = Clipboard.GetData("Toe.Scripting.Script") as MemoryStream;
            if (data == null)
                return false;

            var oldIds = new HashSet<int>(this.Nodes.Select(_ => _.Id));
            using (var reader = new BinaryReader(data))
            {
                var script = Script.Deserialize(reader);
                MergeWith(script);

            }
            ClearSelection();
            foreach (var item in Nodes.Where(x=>!oldIds.Contains(x.Id))) {
                AddToSelectionImpl(item);
            }
            UpdateSelectionRelatedCommands();
            return true;
        }

        public void CutSelected()
        {
            CopySelected();
            DeleteSelected();
        }

        private void UpdateSelectedPins()
        {
            DeselectPins();
            if (SearchPinFilter != null)
            {
                var connectedNodes = CollectConnectedNodes(SearchPinFilter?.Pin);
                foreach (var pin in Nodes.SelectMany(_ => SearchPinFilter.FindMatchingPins(_)))
                {
                    if (!connectedNodes.Contains(pin.Node.Id))
                    {
                        SelectedPins.Add(pin);
                        pin.IsSelected = true;
                    }
                }
            }
        }

        private HashSet<int> CollectConnectedNodes(PinViewModel pin)
        {
            var nodesToExclude = new HashSet<int>();
            if (pin == null)
                return nodesToExclude;

            bool searchOutputPins = SearchPinFilter.Pin.IsInputPin;
            var nodeQueue = new Queue<NodeViewModel>();
            nodeQueue.Enqueue(SearchPinFilter.Pin.Node);

            while (nodeQueue.Count > 0)
            {
                var item = nodeQueue.Dequeue();
                if (!nodesToExclude.Contains(item.Id))
                {
                    nodesToExclude.Add(item.Id);
                    if(searchOutputPins)
                        EnqueueRange(nodeQueue,item.AllConnectedOutputNodes);
                    else
                        EnqueueRange(nodeQueue, item.AllConnectedInputNodes);
                }
            }
            return nodesToExclude;
        }

        private void EnqueueRange<T>(Queue<T> queue, IEnumerable<T> data)
        {
            foreach (var i in data)
            {
                queue.Enqueue(i);
            }
        }

        public event EventHandler<ScriptChangedEventArgs> ScriptChanged;
        public event EventHandler<EventArgs> SelectionChanged;

        private FactoryCategoryViewModel BuildCategory(string name, IEnumerable<INodeFactory> all, int depth)
        {
            var groups = all.ToLookup(_ => _.Category.Length <= depth ? "" : _.Category[depth]);
            var vm = new FactoryCategoryViewModel {Name = name};
            foreach (var g in groups.Where(_ => _.Key != "").OrderBy(_ => _.Key))
                vm.Categories.Add(BuildCategory(g.Key, g, depth + 1));

            foreach (var item in groups[""]) vm.Factories.Add(new FactoryViewModel(this, item));

            return vm;
        }

        private void UpdateSearchResult()
        {
            FilteredFactories = ApplyFilter(Factories);
        }

        private void DeselectPins()
        {
            foreach (var pin in SelectedPins) pin.IsSelected = false;
            SelectedPins.Clear();
        }

        private FactoryViewModel ApplyFilter(FactoryViewModel factory)
        {
            if (factory == null)
                return null;
            if (!string.IsNullOrWhiteSpace(_searchQuery))
                if (factory.Name.IndexOf(_searchQuery, StringComparison.InvariantCultureIgnoreCase) < 0)
                    return null;
            if (SearchPinFilter != null)
                if (!SearchPinFilter.IsMatch(factory))
                    return null;

            return factory;
        }

        private FactoryCategoryViewModel ApplyFilter(FactoryCategoryViewModel category)
        {
            if (category == null)
                return null;
            var categories = category.Categories.Select(_ => ApplyFilter(_)).Where(_ => _ != null)
                .Where(_ => _.Factories.Count > 0 || _.Categories.Count > 0).ToList();
            var factories = category.Factories.Select(_ => ApplyFilter(_)).Where(_ => _ != null).ToList();
            if (categories.Count == 0 && factories.Count == 0)
                return null;
            if (categories.Count == 1 && factories.Count == 0)
                return new FactoryCategoryViewModel
                {
                    Name = category.Name + "\\" + categories[0].Name,
                    Categories = categories[0].Categories,
                    Factories = categories[0].Factories
                };

            return new FactoryCategoryViewModel {Name = category.Name, Categories = categories, Factories = factories};
        }

        private void SelectAll()
        {
            foreach (var item in Items)
            {
                SelectedItems.Add(item);
                item.IsSelected = true;
            }

            UpdateSelectionRelatedCommands();
        }

        private void UpdateSelectionRelatedCommands()
        {
            var hasSelection = SelectedItems.Count > 0;
            var hasSelectedNodes = SelectedNodes.Any();
            CopySelectedCommand.CanExecute = hasSelection;
            CutSelectedCommand.CanExecute = hasSelection;
            DuplicateSelectedCommand.CanExecute = hasSelection;
            GroupSelectedCommand.CanExecute = hasSelectedNodes;
            UngroupSelectedCommand.CanExecute = hasSelectedNodes;
            RaiseSelectionChanged();
        }

        private void Undo()
        {
            SaveLayout();
            UndoStack.Undo(this);
            UpdateUndoCommands();
            RaiseScriptChanged();
        }

        private void Redo()
        {
            UndoStack.Redo(this);
            UpdateUndoCommands();
            RaiseScriptChanged();
        }

        private void ResetZoom()
        {
            ScaleMatrix = Matrix.Identity;
        }

        internal void SetScript(Script script)
        {
            if (_script == script)
                return;
            _script = script;
            ClearSelection();
            ClearItems();
            if (_script == null)
            {
                RaiseScriptChanged();
                return;
            }

            var groups = new Dictionary<int, GroupViewModel>();
            foreach (var group in script.Groups)
            {
                var a = AddGroupImpl(group);
                groups.Add(group.Id, a);
            }

            if (script.Layout?.Nodes == null) NeedsArragement = true;
            var nodes = new Dictionary<int, NodeViewModel>();
            foreach (var node in script.Nodes)
            {
                var a = AddImpl(node);
                nodes.Add(node.Id, a);
                if (node.GroupId != 0)
                {
                    GroupViewModel group;
                    if (!groups.TryGetValue(node.GroupId, out group))
                    {
                        var gr = new NodeGroup {Id = node.GroupId};
                        _script.Groups.Add(gr);
                        group = AddGroupImpl(gr);
                        groups.Add(node.GroupId, group);
                    }
                }
            }

            foreach (var node in script.Nodes)
            {
                foreach (var pin in node.InputPins)
                    if (pin.Connection != null)
                    {
                        var a = nodes[node.Id];
                        var b = nodes[pin.Connection.NodeId];
                        Items.Add(new LinkViewModel(
                            this,
                            b.OutputPins.FirstOrDefault(_ => _.Id == pin.Connection.PinId),
                            a.InputPins.FirstOrDefault(_ => _.Id == pin.Id),
                            pin,
                            false
                        ));
                    }

                foreach (var pin in node.ExitPins)
                    if (pin.Connection != null)
                    {
                        var a = nodes[node.Id];
                        var b = nodes[pin.Connection.NodeId];
                        Items.Add(new LinkViewModel(
                            this,
                            a.ExitPins.FirstOrDefault(_ => _.Id == pin.Id),
                            b.EnterPins.FirstOrDefault(_ => _.Id == pin.Connection.PinId),
                            pin,
                            true
                        ));
                    }
            }

            var nodeGroups = nodes.Values.ToLookup(_ => _.Node.GroupId);

            foreach (var groupViewModel in groups)
            {
                groupViewModel.Value.AddRange(nodeGroups[groupViewModel.Key]);
            }
            RestoreLayout();
            RaiseScriptChanged();
            HasUnsavedChanged = false;
        }

        private void ClearItems()
        {
            var disposables = Items.Select(_ => _ as IDisposable).Where(_ => _ != null).ToList();
            Items.Clear();
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }

        public void Rearrange(ILayoutAlgorithm algorithm = null)
        {
            CreateUndoSnapshot();
            NeedsArragement = false;
            SaveLayout();
            (algorithm ?? new SimpleLayoutAlgorithm()).ArrangeNodes(_script);
            RestoreLayout();
        }

        public void SaveLayout()
        {
            _script.Layout = new ScriptLayout
            {
                Nodes =
                    Nodes.Select(_ => new ScriptNodeLayout
                    {
                        NodeId = _.Id,
                        X = (float) _.Position.X,
                        Y = (float) _.Position.Y,
                        Width = (float) _.Size.Width,
                        Height = (float) _.Size.Height
                    }).ToList()
            };
        }

        public void RestoreLayout()
        {
            var nodeLayouts = _script?.Layout?.Nodes;
            if (nodeLayouts == null) return;
            var nodes = Nodes.ToDictionary(_ => _.Id);
            foreach (var scriptNodeLayout in _script.Layout.Nodes)
            {
                NodeViewModel node;
                if (nodes.TryGetValue(scriptNodeLayout.NodeId, out node))
                    node.Position = new Point(scriptNodeLayout.X, scriptNodeLayout.Y);
            }
        }

        private GroupViewModel AddGroupImpl(NodeGroup gr)
        {
            GroupViewModel a;
            if (gr.Id == Collection<ScriptNode>.InvalidId)
                _script.Add(gr);
            a = new GroupViewModel(this, gr);
            _items.Insert(0, a);
            return a;
        }

        public NodeViewModel Add(ScriptNode node, Point? point = null)
        {
            CreateUndoSnapshot();
            var res = AddImpl(node, point);
            RaiseScriptChanged();
            return res;
        }

        private NodeViewModel AddImpl(ScriptNode node, Point? point = null)
        {
            NodeViewModel a;
            if (node.Id == Collection<ScriptNode>.InvalidId)
                _script.Add(node);
            a = _viewModelFactory.Create(this, node);
            if (point.HasValue)
            {
                a.Position = point.Value;
            }
            Items.Add(a);

            if (_searchPinFilter != null)
            {
                if (_searchPinFilter.Pin != null)
                {
                    var p = _searchPinFilter.FindMatchingPins(a).FirstOrDefault();
                    if (_searchPinFilter.Pin.IsInputPin)
                        Connect(p, _searchPinFilter.Pin, _searchPinFilter.Pin.IsExecutionPin);
                    else
                        Connect(_searchPinFilter.Pin, p, _searchPinFilter.Pin.IsExecutionPin);
                }

                SearchPinFilter = null;
            }

            return a;
        }

        public void Connect(PinViewModel fromPin, PinViewModel toPin, bool isExecution)
        {
            DeselectPins();
            if (fromPin == null || toPin == null)
                return;
            if (fromPin.Type != toPin.Type)
                return;
            CreateUndoSnapshot();
            NodeAndPin from;
            NodeAndPin to;
            if (isExecution)
            {
                from = _script.ResolveExitPin(new Connection {NodeId = fromPin.Node.Id, PinId = fromPin.Pin.Id});
                to = _script.ResolveEnterPin(new Connection {NodeId = toPin.Node.Id, PinId = toPin.Pin.Id});
            }
            else
            {
                from = _script.ResolveOutputPin(new Connection {NodeId = fromPin.Node.Id, PinId = fromPin.Pin.Id});
                to = _script.ResolveInputPin(new Connection {NodeId = toPin.Node.Id, PinId = toPin.Pin.Id});
            }

            if (from == null || to == null)
                return;
            if (isExecution)
                foreach (var connection in fromPin.Connections.ToList())
                    DeleteConnection(connection);
            else
                foreach (var connection in toPin.Connections.ToList())
                    DeleteConnection(connection);
            PinWithConnection hostPin;
            if (isExecution)
            {
                hostPin = (PinWithConnection) from.Pin;
                hostPin.Connection = new Connection(to);
            }
            else
            {
                hostPin = (PinWithConnection) to.Pin;
                hostPin.Connection = new Connection(from);
            }

            Items.Add(new LinkViewModel(this, fromPin, toPin, hostPin, isExecution));
            RaiseScriptChanged();
        }

        private void RaiseScriptChanged()
        {
            ScriptChanged?.Invoke(this, new ScriptChangedEventArgs(Script));
        }
        private void RaiseSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteConnection(LinkViewModel link)
        {
            link.HostPin.Connection = null;
            link.From = null;
            link.To = null;
            _items.Remove(link);
        }

        private void DeleteGroup(GroupViewModel group)
        {
            foreach (var node in group.Nodes.ToList()) DeleteNode(node);
            _script.Groups.Remove(group.NodeGroup);
            _items.Remove(group);
        }

        private void DeleteNode(NodeViewModel node)
        {
            var allConnections = new[] {node.InputPins, node.OutputPins, node.EnterPins, node.ExitPins}
                .SelectMany(_ => _).SelectMany(_ => _.Connections).ToList();
            foreach (var pinViewModel in allConnections) DeleteConnection(pinViewModel);
            node.Group?.Remove(node);
            _script.Nodes.Remove(node.Node);
            _items.Remove(node);
            var disposable = node as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        public void Select(PositionedViewModelBase viewModel)
        {
            ClearSelection();
            AddToSelectionImpl(viewModel);
            UpdateSelectionRelatedCommands();
            
        }

        public void AddToSelection(PositionedViewModelBase viewModel)
        {
            viewModel.IsSelected = true;
            SelectedItems.Add(viewModel);
            UpdateSelectionRelatedCommands();
        }
        private void AddToSelectionImpl(PositionedViewModelBase viewModel)
        {
            viewModel.IsSelected = true;
            SelectedItems.Add(viewModel);
        }
        public void Select(Rect area)
        {
            ClearSelection();
            AddToSelection(area);
        }

        public void AddToSelection(Rect area)
        {
            foreach (var item in Items.Where(_ => _ is NodeViewModel))
                if (area.IntersectsWith(item.GetRect()))
                    AddToSelectionImpl(item);
            UpdateSelectionRelatedCommands();
        }

        public void RemoveSelection(PositionedViewModelBase viewModel)
        {
            SelectedItems.Remove(viewModel);
            viewModel.IsSelected = false;
            UpdateSelectionRelatedCommands();
        }

        public void MoveSelectedNodes(Vector vector)
        {
            foreach (var modelBase in SelectedNodesIncludingGroups)
                modelBase.Position += vector;
        }

        public void ClearSelection()
        {
            foreach (var vm in SelectedItems) vm.IsSelected = false;
            SelectedItems.Clear();
            UpdateSelectionRelatedCommands();
        }

        public void DeleteSelected()
        {
            CreateUndoSnapshot();
            var selected = SelectedItems.ToList();
            ClearSelection();
            foreach (var groupViewModel in selected.Select(_ => _ as GroupViewModel).Where(_ => _ != null))
                DeleteGroup(groupViewModel);
            foreach (var linkViewModel in selected.Select(_ => _ as LinkViewModel).Where(_ => _ != null))
                DeleteConnection(linkViewModel);
            foreach (var linkViewModel in selected.Select(_ => _ as NodeViewModel).Where(_ => _ != null))
                DeleteNode(linkViewModel);
            RaiseScriptChanged();
        }

        private void CreateUndoSnapshot()
        {
            HasUnsavedChanged = true;
            SaveLayout();
            var clone = _script.Clone();
            UndoStack.Enqueue(new UndoActionViewModel(clone));
            UpdateUndoCommands();
        }

        private void UpdateUndoCommands()
        {
            var invalidateRequerySuggested = false;
            if (UndoCommand.CanExecute != UndoStack.CanUndo)
            {
                UndoCommand.CanExecute = UndoStack.CanUndo;
                invalidateRequerySuggested = true;
            }

            if (RedoCommand.CanExecute != UndoStack.CanRedo)
            {
                RedoCommand.CanExecute = UndoStack.CanRedo;
                invalidateRequerySuggested = true;
            }

            if (invalidateRequerySuggested)
                CommandManager.InvalidateRequerySuggested();
        }

        public void StartDragging()
        {
            CreateUndoSnapshot();
        }

        public void Scale(float factor, Point center)
        {
            //ScaleMatrix = ScaleMatrix *  new Matrix(factor, 0, 0, factor, 0, 0);
            Trace.WriteLine("Scale by " + factor + " at " + center);
            var m = ScaleMatrix;
            m.ScaleAt(factor, factor, center.X, center.Y);
            ScaleMatrix = m;
            //= ScaleMatrix * new Matrix(factor, 0, 0, factor, 0, 0);
        }

        public void CreateNodeMenu(PinViewModel viewModel)
        {
            IsContextMenuOpen = true;
        }

        public void SetValue(ScriptNode node, string value)
        {
            CreateUndoSnapshot();
            node.Value = value;
            RaiseScriptChanged();
        }

        public void SetName(ScriptNode node, string value)
        {
            CreateUndoSnapshot();
            node.Name = value;
            RaiseScriptChanged();
        }

        public void StartConnection(PinViewModel viewModel)
        {
            SearchPinFilter = new SearchPinFilter(viewModel, viewModel.Type);
            SearchQuery = "";
        }

        public void ResetUndoStack()
        {
            UndoStack.Clear();
            UpdateUndoCommands();
        }

        public void MergeWith(Script script)
        {
            CreateUndoSnapshot();
            var s = _script ?? new Script();
            _script = null;
            var oldIds = new HashSet<int>(this.Nodes.Select(_ => _.Id));

            var newNodes = s.MergeWith(script, (float)Mouse.Location.X, (float)Mouse.Location.Y);
            SetScript(s);

            ClearSelection();
            foreach (var item in Nodes.Where(x => !oldIds.Contains(x.Id))) {
                AddToSelectionImpl(item);
            }

            UpdateSelectionRelatedCommands();
        }

        public Script ExtractSelected()
        {
            var script = new Script();

            Dictionary<int, ScriptNodeLayout> layoutLookup = null;
            HashSet<int> visitedGroups = new HashSet<int>();
            if (Script.Layout != null)
            {
                layoutLookup = Script.Layout?.Nodes?.ToDictionary(_ => _.NodeId);
                script.Layout = new ScriptLayout();
            }


            foreach (var node in SelectedNodesIncludingGroups)
            {
                var clone = node.Node.Clone();
                script.Add(clone);
                if (layoutLookup != null)
                {
                    ScriptNodeLayout layout;
                    if (layoutLookup.TryGetValue(node.Id, out layout))
                    {
                        script.Layout.Nodes.Add(layout.Clone());
                    }
                }

                if (node.Group != null)
                {
                    if (visitedGroups.Add(node.Group.Uid))
                    {
                        script.Groups.Add(node.Group.NodeGroup.Clone());
                    }
                }
            }

            var validIds = new HashSet<int>(script.Nodes.Select(_ => _.Id));
            foreach (var node in script.Nodes)
            foreach (var pin in node.InputPins.Concat(node.ExitPins))
                if (pin.Connection != null)
                    if (!validIds.Contains(pin.Connection.NodeId))
                        pin.Connection = null;
            return script;
        }

        public void CopySelected()
        {
            var subScript = ExtractSelected();
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                subScript.Serialize(writer);
            }

            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                subScript.Serialize(writer);
            }
            var data = new DataObject();
            data.SetData("Toe.Scripting.Script", new MemoryStream(stream.ToArray()));
            data.SetData(DataFormats.UnicodeText, stringBuilder.ToString());
            Clipboard.SetDataObject(data);
        }

        public void DuplicateSelected()
        {
            var subScript = ExtractSelected();
            MergeWith(subScript);
        }

        public GroupViewModel GroupSelected()
        {
            var nodes = SelectedNodesIncludingGroups.ToList();
            if (nodes.Count == 0)
                return null;
            var hasUngouped = nodes.Any(_ => _.Group == null);
            var groups = nodes.Select(_ => _.Group).Concat(SelectedGroups).Where(_=>_ != null).Distinct().ToList();
            if (!hasUngouped && groups.Count == 1)
                return groups[0];
            CreateUndoSnapshot();

            GroupViewModel groupViewModel;
            if (groups.Count == 1)
            {
                groupViewModel = groups[0];
            }
            else
            {
                var group = new NodeGroup();
                groupViewModel = AddGroupImpl(group);
                groupViewModel.Name = "Group " + groupViewModel.NodeGroup.Id;
            }

            foreach (var nodeRange in nodes.ToLookup(_ => _.Group).Where(_ => _.Key != groupViewModel && _.Key != null))
            {
                nodeRange.Key.RemoveRange(nodeRange);
            }

            ;
            groupViewModel.AddRange(nodes);
            return groupViewModel;
        }

        public void UngroupSelected()
        {
            var nodes = SelectedNodesIncludingGroups.ToList();
            if (!nodes.Any(_ => _.Group != null))
                return;
            CreateUndoSnapshot();
            foreach (var node in nodes) node.Group = null;
        }

        public void SetGroupName(NodeGroup nodeGroup, string value)
        {
            CreateUndoSnapshot();
            _script.Groups[nodeGroup.Id].Name = value;
        }

        public void StopDragging()
        {
            SaveLayout();
        }
    }
}