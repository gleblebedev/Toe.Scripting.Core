using System.Collections.ObjectModel;
using System.Windows;

namespace Toe.Scripting.WPF.ViewModels
{
    public abstract class PinViewModel : PositionedViewModelBase
    {
        private bool _isConnected;
        private Pin _pin;
        private bool _useNodeName;

        public PinViewModel(NodeViewModel node, Pin pin)
        {
            Node = node;
            _pin = pin;
            Type = pin.Type;
            Connections.CollectionChanged += (s, a) => { IsConnected = Connections.Count != 0; };
        }

        public NodeViewModel Node { get; }

        public ObservableCollection<LinkViewModel> Connections { get; } = new ObservableCollection<LinkViewModel>();

        public bool IsConnected
        {
            get => _isConnected;
            set => RaiseAndSetIfChanged(ref _isConnected, value);
        }

 
        public bool UseNodeName
        {
            get => _useNodeName;
            set
            {
                if (RaiseAndSetIfChanged(ref _useNodeName, value))
                    RaisePropertyChanged(nameof(Caption));
            }
        
        }

        public string Caption
        {
            get { return _useNodeName ? Node?.Name : _pin?.Id; }
        }

        public string Id
        {
            get { return _pin.Id; }
        }

        public abstract bool IsInputPin { get; }
        public abstract bool IsExecutionPin { get; }
        public string Type { get; private set; }
        public Pin Pin { get { return _pin; } }

        public void CreateNodeMenu(Point getPosition)
        {
            Node.Script.CreateNodeMenu(this);
        }

        public void StartConnection()
        {
            Node.Script.StartConnection(this);
        }
    }
}