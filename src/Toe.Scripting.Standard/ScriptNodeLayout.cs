using System;
using System.IO;

namespace Toe.Scripting
{
    public sealed class ScriptNodeLayout : ICloneable
    {
        public int NodeId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScriptNodeLayout Clone()
        {
            return new ScriptNodeLayout
            {
                NodeId = NodeId,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height
            };
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(NodeId);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Width);
            writer.Write(Height);
        }

        public void Deserialize(BinaryReader reader)
        {
            NodeId = reader.ReadInt32();
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Width = reader.ReadSingle();
            Height = reader.ReadSingle();
        }
    }
}