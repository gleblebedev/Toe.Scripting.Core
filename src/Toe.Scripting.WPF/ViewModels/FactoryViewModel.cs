using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Toe.Scripting.WPF.ViewModels
{
    public class FactoryViewModel
    {
        private readonly INodeFactory _factory;
        private readonly ScriptViewModel _script;

        public FactoryViewModel(ScriptViewModel script, INodeFactory factory)
        {
            _script = script;
            _factory = factory;
            CreateCommand = new ScriptingCommand(() => Create(script.CreateLocation));
        }

        public string Name => _factory.Name;

        public IEnumerable<string> InputTypes => _factory.InputTypes;
        public IEnumerable<string> OutputTypes => _factory.OutputTypes;

        public ICommand CreateCommand { get; set; }
        public bool HasEnterPins => _factory.HasEnterPins;
        public bool HasExitPins => _factory.HasExitPins;

        public void Create(Point _)
        {
            _script.Add(_factory.Build(), _);
        }
    }
}