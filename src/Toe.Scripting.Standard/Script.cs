using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Toe.Scripting
{
    public class Script : ICloneable
    {
        public static readonly Version ZeroVersion = new Version(0, 0, 0, 0);

        private readonly Collection<NodeGroup> _groups = new Collection<NodeGroup>();

        private readonly Collection<ScriptNode> _nodes = new Collection<ScriptNode>();


        public Version Version { get; set; } = ZeroVersion;

        public IList<ScriptNode> Nodes => _nodes;

        public IList<NodeGroup> Groups => _groups;

        public ScriptLayout Layout { get; set; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public IEnumerable<Link> GetDataLinks()
        {
            foreach (var node in Nodes)
            foreach (var pin in node.InputPins)
                if (pin.Connection != null)
                    yield return new DataLink(this, node, pin);
        }

        public IEnumerable<Link> GetExecutionLinks()
        {
            foreach (var node in Nodes)
            foreach (var pin in node.ExitPins)
                if (pin.Connection != null)
                    yield return new ExecutionLink(this, node, pin);
        }

        public IEnumerable<Link> GetLinks()
        {
            foreach (var node in Nodes)
            {
                foreach (var pin in node.InputPins)
                    if (pin.Connection != null)
                        yield return new DataLink(this, node, pin);
                foreach (var pin in node.ExitPins)
                    if (pin.Connection != null)
                        yield return new ExecutionLink(this, node, pin);
            }
        }

        public NodeAndPin ResolveEnterPin(Connection connection)
        {
            if (connection == null)
                return null;
            var node = _nodes[connection.NodeId];
            if (node == null)
                return null;
            var pin = node.ResolveEnterPin(connection);
            if (pin == null)
                return null;
            return new NodeAndPin {Node = node, Pin = pin};
        }

        public NodeAndPin ResolveExitPin(Connection connection)
        {
            if (connection == null)
                return null;
            var node = _nodes[connection.NodeId];
            if (node == null)
                return null;
            var pin = node.ResolveExitPin(connection);
            if (pin == null)
                return null;
            return new NodeAndPin {Node = node, Pin = pin};
        }

        public NodeAndPin ResolveOutputPin(Connection connection)
        {
            if (connection == null)
                return null;
            var node = _nodes[connection.NodeId];
            if (node == null)
                return null;
            var pin = node.ResolveOutputPin(connection);
            if (pin == null)
                return null;
            return new NodeAndPin {Node = node, Pin = pin};
        }

        public NodeAndPin ResolveInputPin(Connection connection)
        {
            if (connection == null)
                return null;
            var node = _nodes[connection.NodeId];
            if (node == null)
                return null;
            var pin = node.ResolveInputPin(connection);
            if (pin == null)
                return null;
            return new NodeAndPin {Node = node, Pin = pin};
        }

        public ScriptNode Add(ScriptNode node)
        {
            Nodes.Add(node);
            return node;
        }

        public NodeGroup Add(NodeGroup group)
        {
            Groups.Add(group);
            return group;
        }

        public Script Clone()
        {
            var clone = new Script();
            foreach (var scriptNode in Nodes) clone.Nodes.Add(scriptNode.Clone());
            foreach (var group in Groups) clone.Groups.Add(group.Clone());
            if (Layout != null)
                clone.Layout = Layout.Clone();
            return clone;
        }

        public IList<ScriptNode> MergeWith(Script script, float x, float y)
        {
            var nodeMap = new Dictionary<int, int>();
            var groupMap = new Dictionary<int, int>();
            var newNodes = new List<ScriptNode>();
            foreach (var group in script.Groups)
            {
                var newGroup = group.CloneNew();
                Groups.Add(newGroup);
                groupMap[group.Id] = newGroup.Id;
            }

            foreach (var node in script.Nodes)
            {
                var newNode = node.CloneNew();
                if (newNode.GroupId != 0) newNode.GroupId = groupMap[newNode.GroupId];
                newNodes.Add(newNode);
                Nodes.Add(newNode);
                nodeMap[node.Id] = newNode.Id;
            }

            foreach (var newNode in newNodes)
            {
                foreach (var pin in newNode.InputPins)
                    if (pin.Connection != null)
                        pin.Connection = new Connection(nodeMap[pin.Connection.NodeId], pin.Connection.PinId);
                foreach (var pin in newNode.ExitPins)
                    if (pin.Connection != null)
                        pin.Connection = new Connection(nodeMap[pin.Connection.NodeId], pin.Connection.PinId);
            }

            // Transfer layout where it's known
            var nodesWithoutLayout = new HashSet<int>(newNodes.Select(_ => _.Id));
            if (script.Layout != null && script.Layout.Nodes.Count > 0)
            {
                Layout = Layout ?? new ScriptLayout();
                var left = script.Layout.Nodes.Select(_ => _.X).Min();
                var top = script.Layout.Nodes.Select(_ => _.Y).Min();
                foreach (var scriptNodeLayout in script.Layout.Nodes)
                {
                    var layout = scriptNodeLayout.Clone();
                    int targetNodeId;
                    if (nodeMap.TryGetValue(layout.NodeId, out targetNodeId))
                    {
                        layout.NodeId = targetNodeId;
                        layout.X += x - left;
                        layout.Y += y - top;
                        Layout.Nodes.Add(layout);
                        nodesWithoutLayout.Remove(layout.NodeId);
                    }
                }
            }
            // Now process the rest of the nodes
            {
                foreach (var i in nodesWithoutLayout) {
                    var layout = new ScriptNodeLayout();
                    layout.NodeId = i;
                    layout.X += x;
                    layout.Y += y;
                    x += 10;
                    Layout.Nodes.Add(layout);
                }
            }

            return newNodes;
        }

        public void Serialize(TextWriter writer)
        {
            writer.WriteLine("{");
            writer.WriteLine("  \"Version\": {");
            writer.Write("    \"Major\": ");
            writer.Write(Version.Major);
            writer.WriteLine(",");
            writer.Write("    \"Minor\": ");
            writer.Write(Version.Minor);
            writer.WriteLine(",");
            writer.Write("    \"Build\": ");
            writer.Write(Version.Build);
            writer.WriteLine(",");
            writer.Write("    \"Revision\": ");
            writer.Write(Version.Revision);
            writer.WriteLine(",");
            writer.Write("    \"MajorRevision\": ");
            writer.Write(Version.MajorRevision);
            writer.WriteLine(",");
            writer.Write("    \"MinorRevision\": ");
            writer.Write(Version.MinorRevision);
            writer.WriteLine();
            writer.WriteLine("  },");
            writer.WriteLine("  \"Nodes\": [");
            var nodes = Nodes.ToList();
            for (var index = 0; index < nodes.Count; index++)
            {
                nodes[index].Serialize(writer);
                if (index != nodes.Count - 1)
                    writer.WriteLine(",");
                else
                    writer.WriteLine();
            }

            writer.WriteLine("  ]");
            writer.WriteLine("}");
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write('T');
            writer.Write('o');
            writer.Write('e');
            writer.Write('S');
            writer.Write(Version.Major);
            writer.Write(Version.Minor);
            writer.Write(Version.Build);
            writer.Write(Version.Revision);

            writer.Write(Nodes.Count);
            foreach (var scriptNode in Nodes) scriptNode.Serialize(writer);

            writer.Write(Groups.Count);
            foreach (var group in Groups) group.Serialize(writer);

            writer.Write(Layout != null);
            if (Layout != null) Layout.Serialize(writer);
        }

        public static Script Deserialize(BinaryReader reader)
        {
            var c = new[] {reader.ReadChar(), reader.ReadChar(), reader.ReadChar(), reader.ReadChar()};
            if (c[0] != 'T' || c[1] != 'o' || c[2] != 'e' || c[3] != 'S') throw new FormatException();
            var script = new Script();
            var Major = reader.ReadInt32();
            var Minor = reader.ReadInt32();
            var Build = reader.ReadInt32();
            var Revision = reader.ReadInt32();
            script.Version = new Version(Major, Minor, Build, Revision);
            var numNodes = reader.ReadInt32();
            for (var i = 0; i < numNodes; i++)
            {
                var n = new ScriptNode();
                n.Deserialize(reader);
                script.Nodes.Add(n);
            }

            var numGroups = reader.ReadInt32();
            for (var i = 0; i < numGroups; i++)
            {
                var n = new NodeGroup();
                n.Deserialize(reader);
                script.Groups.Add(n);
            }

            var hasLayout = reader.ReadBoolean();
            if (hasLayout)
            {
                script.Layout = new ScriptLayout();
                script.Layout.Deserialize(reader);
            }

            return script;
        }

        internal class DataLink : Link
        {
            private readonly Connection _connection;
            private readonly Script _script;
            private NodeAndPin _from;

            public DataLink(Script script, ScriptNode node, PinWithConnection pin)
            {
                _script = script;
                To = new NodeAndPin(node, pin);
                _connection = pin.Connection;
            }

            public override NodeAndPin From => _from ?? (_from = ResolveConnection());

            public override NodeAndPin To { get; }

            public override int FromNodeId => _connection.NodeId;

            public override string FromPinId => _connection.PinId;

            private NodeAndPin ResolveConnection()
            {
                return _script.ResolveOutputPin(_connection);
            }
        }

        internal class ExecutionLink : Link
        {
            private readonly Connection _connection;
            private readonly Script _script;
            private NodeAndPin _to;

            public ExecutionLink(Script script, ScriptNode node, PinWithConnection pin)
            {
                _script = script;
                From = new NodeAndPin(node, pin);
                _connection = pin.Connection;
            }

            public override NodeAndPin From { get; }

            public override NodeAndPin To => _to ?? (_to = ResolveConnection());

            public override string ToPinId => _connection.PinId;


            public override int ToNodeId => _connection.NodeId;


            private NodeAndPin ResolveConnection()
            {
                return _script.ResolveEnterPin(_connection);
            }
        }

        public void ClearGroups()
        {
            if (Groups.Count != 0)
            {
                foreach (var node in _nodes)
                {
                    node.GroupId = NodeGroup.InvalidId;
                }

                Groups.Clear();
            }
        }
    }
}