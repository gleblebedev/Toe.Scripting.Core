using System.Collections.Generic;

namespace Toe.Scripting
{
    public interface INodeFactory
    {
        string Type { get; }
        string Name { get; }
        string[] Category { get; }
        NodeFactoryVisibility Visibility { get; }

        IEnumerable<string> InputTypes { get; }
        IEnumerable<string> OutputTypes { get; }
        bool HasEnterPins { get; }
        bool HasExitPins { get; }

        ScriptNode Build();
    }
}