using System;
using System.Collections.Generic;
using System.IO;

namespace Toe.Scripting
{
    public sealed class ScriptLayout : ICloneable
    {
        private IList<ScriptNodeLayout> _nodes = new List<ScriptNodeLayout>();

        public IList<ScriptNodeLayout> Nodes
        {
            get => _nodes;
            set => _nodes = value ?? new List<ScriptNodeLayout>();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScriptLayout Clone()
        {
            var clone = new ScriptLayout();
            foreach (var scriptNode in Nodes) clone.Nodes.Add(scriptNode.Clone());
            return clone;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Nodes.Count);
            foreach (var scriptNode in Nodes) scriptNode.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            Nodes.Clear();
            var numNodes = reader.ReadInt32();
            for (var i = 0; i < numNodes; i++)
            {
                var n = new ScriptNodeLayout();
                n.Deserialize(reader);
                Nodes.Add(n);
            }
        }
    }
}