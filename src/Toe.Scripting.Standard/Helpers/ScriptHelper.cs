using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Toe.Scripting.Helpers
{
    public class ScriptHelper : ScriptHelper<object>
    {
        public ScriptHelper(Script script) : base(script)
        {
        }

        public static ScriptHelper<T> Create<T>(Script script)
        {
            return new ScriptHelper<T>(script);
        }
    }

    public abstract class ScriptHelperBase
    {
        public Version Version { get; set; }
    }

    public class ScriptHelper<T> : ScriptHelperBase, ICloneable
    {
        public ScriptHelper()
        {
            Nodes = new NodeList<T>(this);
        }

        public ScriptHelper(Script script):this()
        {
            Version = script.Version;

            var nodeLookup = new Dictionary<int, NodeHelper<T>>();
            foreach (var node in script.Nodes)
            {
                var nodeHelper = new NodeHelper<T>(node);
                nodeLookup.Add(node.Id, nodeHelper);
                Nodes.Add(nodeHelper);
            }

            foreach (var executionLink in script.GetExecutionLinks())
            {
                var from = nodeLookup[executionLink.FromNodeId].ExitPins[executionLink.FromPinId];
                var to = nodeLookup[executionLink.ToNodeId].ExitPins[executionLink.ToPinId];
                Link(from, to);
            }
            foreach (var executionLink in script.GetDataLinks())
            {
                var from = nodeLookup[executionLink.FromNodeId].OutputPins[executionLink.FromPinId];
                var to = nodeLookup[executionLink.ToNodeId].InputPins[executionLink.ToPinId];
                Link(from, to);
            }
        }

        public NodeList<T> Nodes { get; }

        public Script BuildScript()
        {
            var script = new Script();
            EnsureUniqueIds();
            script.Version = Version;
            foreach (var nodeHelper in Nodes) script.Nodes.Add(nodeHelper.BuildNode());
            return script;
        }

        public void EnsureUniqueIds()
        {
            var visitedIds = new HashSet<int> {0};
            var nextId = 1;
            foreach (var nodeHelper in Nodes)
            {
                if (visitedIds.Add(nodeHelper.Id))
                {
                    if (nextId <= nodeHelper.Id)
                        nextId = nodeHelper.Id + 1;
                }
                else
                {
                    visitedIds.Add(nextId);
                    nodeHelper.Id = nextId;
                    ++nextId;
                }
            }
        }

        public void Add(NodeHelper<T> clone)
        {
            Nodes.Add(clone);
        }
        public void CopyConnections(PinHelper<T> from, PinHelper<T> to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), string.Format("Can't copy connections to {0}: source pin is null", to));
            }
            foreach (var link in from.Links)
            {
                if (RuntimeHelpers.Equals(link.To, from))
                    Link(link.From, to);
                else
                    Link(to, link.To);
            }
        }
        public LinkHelper<T> LinkData(PinHelper<T> from, NodeHelper<T> to)
        {
            if (to.InputPins.Count != 1)
                throw new ArgumentException("Target node should have exactly one input pin but it has " + to.OutputPins.Count, nameof(to));
            return Link(from, to.InputPins.First());
        }
        public LinkHelper<T> LinkData(NodeHelper<T> from, PinHelper<T> to)
        {
            if (from.OutputPins.Count != 1)
                throw new ArgumentException("Source node should have exactly one output pin but it has "+ from.OutputPins.Count, nameof(from));
            return Link(from.OutputPins.First(), to);
        }
        public LinkHelper<T> LinkData(NodeHelper<T> from, NodeHelper<T> to)
        {
            if (to.InputPins.Count != 1)
                throw new ArgumentException("Target node should have exactly one input pin but it has " + to.OutputPins.Count, nameof(to));
            if (from.OutputPins.Count != 1)
                throw new ArgumentException("Source node should have exactly one output pin but it has " + from.OutputPins.Count, nameof(from));
            return Link(from.OutputPins.First(), to.InputPins.First());
        }
        public LinkHelper<T> Link(PinHelper<T> from, PinHelper<T> to)
        {
            var link = new LinkHelper<T>(from,to);
            from.Links.Add(link);
            to.Links.Add(link);
            return link;
        }
        public void RemoveLink(LinkHelper<T> linkHelper)
        {
            linkHelper.From.Links.Remove(linkHelper);
            linkHelper.To.Links.Remove(linkHelper);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScriptHelper<T> Clone()
        {
            var scriptHelper = new ScriptHelper<T>();
            var pinLookup = new Dictionary<PinHelper<T>, PinHelper<T>>();
            foreach (var nodeHelper in Nodes)
            {
                var clone = nodeHelper.Clone();
                foreach (var pins in nodeHelper.EnumeratePins().Zip(clone.EnumeratePins(), (a,b)=>new {Pin = a, Clone = b}))
                {
                    pinLookup[pins.Pin] = pins.Clone;
                }
                scriptHelper.Add(clone);
            }

            foreach (var link in EnumerateLinks())
            {
                scriptHelper.Link(pinLookup[link.From], pinLookup[link.To]);
            }
            return scriptHelper;
        }

        private IEnumerable<LinkHelper<T>> EnumerateLinks()
        {
            return Nodes.SelectMany(_ => _.EnumerateOwnedLinks());
        }

        public IEnumerable<NodeHelper<T>> Merge(Script script)
        {
            var map = new Dictionary<int, NodeHelper<T>>();
            foreach (var node in script.Nodes)
            {
                var helper = new NodeHelper<T>(node);
                map.Add(node.Id,helper);
                Add(helper);
            }

            foreach (var node in script.Nodes)
            {
                foreach (var pinWithConnection in node.InputPins)
                {
                    if (pinWithConnection.Connection != null)
                    {
                        var from = map[pinWithConnection.Connection.NodeId].OutputPins[pinWithConnection.Connection.PinId];
                        var to = map[node.Id].InputPins[pinWithConnection.Id];
                        Link(from, to);
                    }
                }
                foreach (var pinWithConnection in node.ExitPins)
                {
                    if (pinWithConnection.Connection != null)
                    {
                        var to = map[pinWithConnection.Connection.NodeId].EnterPins[pinWithConnection.Connection.PinId];
                        var from = map[node.Id].ExitPins[pinWithConnection.Id];
                        Link(from, to);
                    }
                }
            }

            return map.Values;
        }
    }
}