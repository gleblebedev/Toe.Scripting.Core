using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Toe.Scripting.Helpers
{
    public class NodeHelper
    {
    }

    public class NodeHelper<T> : NodeHelper
    {
        public NodeHelper()
        {
            InputPins = new PinList<T>(this);
            OutputPins = new PinList<T>(this);
            EnterPins = new PinList<T>(this);
            ExitPins = new PinList<T>(this);
        }

        public NodeHelper(ScriptNode node)
        {
            Id = node.Id;
            Type = node.Type;
            Value = node.Value;
            Name = node.Name;
            Category= node.Category;

            InputPins = new PinList<T>(this,node.InputPins);
            OutputPins = new PinList<T>(this,node.OutputPins);
            EnterPins = new PinList<T>(this,node.EnterPins);
            ExitPins = new PinList<T>(this,node.ExitPins);
        }

        public PinList<T> ExitPins { get; }

        public PinList<T> EnterPins { get; }

        public PinList<T> OutputPins { get; }

        public PinList<T> InputPins { get; }

        public ScriptHelper<T> Script { get; internal set; }

        public int Id { get; set; }
        public NodeCategory Category { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public T Extra { get; set; }

        public IEnumerable<PinList<T>> EnumeratePinLists()
        {
            yield return EnterPins;
            yield return InputPins;
            yield return ExitPins;
            yield return OutputPins;
        }

        public IEnumerable<PinHelper<T>> EnumeratePins()
        {
            return EnumeratePinLists().SelectMany(_ => _);
        }

        public ScriptNode BuildNode()
        {
            var node = new ScriptNode
            {
                Id = Id,
                Type = Type,
                Name = Name,
                Value = Value,
                Category = Category
            };
            BuildPins(EnterPins, node.EnterPins);
            BuildPins(ExitPins, node.ExitPins);
            BuildPins(InputPins, node.InputPins);
            BuildPins(OutputPins, node.OutputPins);
            return node;
        }

        private void BuildPins(PinList<T> pins, List<Pin> nodePins)
        {
            foreach (var pinHelper in pins) nodePins.Add(pinHelper.BuildPin());
        }

        private void BuildPins(PinList<T> pins, List<PinWithConnection> nodePins)
        {
            foreach (var pinHelper in pins) nodePins.Add(pinHelper.BuildPinWithConnection());
        }

        public NodeHelper<T> Clone()
        {
            NodeHelper<T> node = new NodeHelper<T>();
            node.Id = Id;
            node.Name = Name;
            node.Value = Value;
            node.Type = Type;
            node.InputPins.AddRange(InputPins.Select(_=>_.Clone(node)));
            node.OutputPins.AddRange(OutputPins.Select(_ => _.Clone(node)));
            node.EnterPins.AddRange(EnterPins.Select(_ => _.Clone(node)));
            node.ExitPins.AddRange(ExitPins.Select(_ => _.Clone(node)));
            return node;
        }
        public NodeHelper<T> CloneWithConnections()
        {
            var node = Clone();
            Script.Add(node);
            CloneConnections(InputPins, node.InputPins);
            CloneConnections(OutputPins, node.OutputPins);
            CloneConnections(EnterPins, node.EnterPins);
            CloneConnections(ExitPins, node.ExitPins);
            return node;
        }

        private void CloneConnections(PinList<T> pins, PinList<T> clonePins)
        {
            for (int i = 0; i < pins.Count; ++i)
            {
                var pin = pins[i];
                foreach (var linkHelper in pin.Links)
                {
                    if (RuntimeHelpers.Equals(linkHelper.From, pin))
                        Script.Link(clonePins[i], linkHelper.To);
                    else
                        Script.Link(linkHelper.From, clonePins[i]);
                }
            }
        }
   
        public void RemoveAllLinks()
        {
            foreach (var pinHelper in InputPins)
            {
                pinHelper.RemoveLinks();
            }
            foreach (var pinHelper in OutputPins)
            {
                pinHelper.RemoveLinks();
            }
            foreach (var pinHelper in ExitPins)
            {
                pinHelper.RemoveLinks();
            }
            foreach (var pinHelper in EnterPins)
            {
                pinHelper.RemoveLinks();
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Type);
        }

        public IEnumerable<LinkHelper<T>> EnumerateLinks()
        {
            return new[]{EnterPins,InputPins,OutputPins,ExitPins}.SelectMany(_=>_).SelectMany(_ => _.Links);
        }
        public IEnumerable<LinkHelper<T>> EnumerateOwnedLinks()
        {
            return new[] { OutputPins, EnterPins }.SelectMany(_ => _).SelectMany(_ => _.Links);
        }
    }
}