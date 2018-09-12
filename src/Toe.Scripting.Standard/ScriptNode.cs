using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Toe.Scripting
{
    public sealed class ScriptNode : ScriptItem, ICloneable
    {
        public string Type { get; set; }
        public string Name { get; set; } = "";
        public string Value { get; set; }
        public int GroupId { get; set; }

        public NodeCategory Category { get; set; }

        public List<PinWithConnection> InputPins { get; } = new List<PinWithConnection>();

        public List<Pin> OutputPins { get; } = new List<Pin>();

        public List<Pin> EnterPins { get; } = new List<Pin>();

        public List<PinWithConnection> ExitPins { get; } = new List<PinWithConnection>();

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Pin ResolveOutputPin(Connection connection)
        {
            if (connection.NodeId != Id)
                throw new InvalidOperationException("Wrong node Uid");
            return OutputPins.FirstOrDefault(_ => _.Id == connection.PinId);
        }

        public PinWithConnection ResolveInputPin(Connection connection)
        {
            if (connection.NodeId != Id)
                throw new InvalidOperationException("Wrong node Uid");
            return InputPins.FirstOrDefault(_ => _.Id == connection.PinId);
        }

        public Pin ResolveEnterPin(Connection connection)
        {
            if (connection.NodeId != Id)
                throw new InvalidOperationException("Wrong node Uid");
            return EnterPins.FirstOrDefault(_ => _.Id == connection.PinId);
        }

        public PinWithConnection ResolveExitPin(Connection connection)
        {
            if (connection.NodeId != Id)
                throw new InvalidOperationException("Wrong node Uid");
            return ExitPins.FirstOrDefault(_ => _.Id == connection.PinId);
        }

        public ScriptNode Clone()
        {
            var clone = CloneNew();
            clone.Id = Id;
            return clone;
        }

        public ScriptNode CloneNew()
        {
            var clone = new ScriptNode
            {
                Category = Category,
                Name = Name,
                Type = Type,
                Value = Value,
                GroupId = GroupId
            };
            foreach (var pin in InputPins) clone.InputPins.Add(pin.Clone());
            foreach (var pin in OutputPins) clone.OutputPins.Add(pin.Clone());
            foreach (var pin in EnterPins) clone.EnterPins.Add(pin.Clone());
            foreach (var pin in ExitPins) clone.ExitPins.Add(pin.Clone());
            return clone;
        }

        public override string ToString()
        {
            if (Id != Collection<ScriptNode>.InvalidId) return string.Format("UID:{0} {1} ({2})", Id, Name, Type);

            return string.Format("{0} ({1})", Name, Type);
        }

        public void Serialize(TextWriter writer)
        {
            writer.WriteLine("    {");
            if (Id != Collection<ScriptNode>.InvalidId)
                writer.Write("      \"Uid\": \"");
            writer.Write(Id);
            writer.WriteLine("\",");
            writer.Write("      \"Type\": \"");
            writer.Write(Type);
            writer.WriteLine("\",");
            writer.Write("      \"Name\": \"");
            writer.Write(Name);
            writer.WriteLine("\",");
            if (Value != null)
            {
                writer.Write("      \"Value\": \"");
                writer.Write(Value);
                writer.WriteLine("\",");
            }

            if (GroupId != 0)
            {
                writer.Write("      \"GroupId\": \"");
                writer.Write(GroupId);
                writer.WriteLine("\",");
            }

            writer.Write("      \"Category\": \"");
            writer.Write(Category);
            writer.WriteLine("\",");
            writer.WriteLine("      \"EnterPins\": [");
            SerializePins(EnterPins, writer);
            writer.WriteLine("      ],");
            writer.WriteLine("      \"ExitPins\": [");
            SerializePins(ExitPins, writer);
            writer.WriteLine("      ],");
            writer.WriteLine("      \"InputPins\": [");
            SerializePins(InputPins, writer);
            writer.WriteLine("      ],");
            writer.WriteLine("      \"OutputPins\": [");
            SerializePins(OutputPins, writer);
            writer.WriteLine("      ]");
            writer.Write("    }");
        }

        private void SerializePins(IEnumerable<Pin> enterPins, TextWriter writer)
        {
            var prefix = "";
            var altPrefix = "," + Environment.NewLine;

            foreach (var enterPin in enterPins)
            {
                writer.Write(prefix);
                prefix = altPrefix;
                enterPin.Serialize(writer);
            }

            writer.WriteLine();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(GroupId);
            writer.Write((int) Category);
            writer.Write(Type ?? "");
            writer.Write(Name ?? "");
            writer.Write(Value != null);
            if (Value != null)
                writer.Write(Value);

            writer.Write(EnterPins.Count);
            foreach (var pin in EnterPins) pin.Serialize(writer);
            writer.Write(InputPins.Count);
            foreach (var pin in InputPins) pin.Serialize(writer);
            writer.Write(ExitPins.Count);
            foreach (var pin in ExitPins) pin.Serialize(writer);
            writer.Write(OutputPins.Count);
            foreach (var pin in OutputPins) pin.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            GroupId = reader.ReadInt32();
            Category = (NodeCategory) reader.ReadInt32();
            Type = reader.ReadString();
            Name = reader.ReadString();
            var hasValue = reader.ReadBoolean();
            if (hasValue)
                Value = reader.ReadString();

            var pinsCount = reader.ReadInt32();
            for (var i = 0; i < pinsCount; i++)
            {
                var pin = new Pin();
                pin.Deserialize(reader);
                EnterPins.Add(pin);
            }

            pinsCount = reader.ReadInt32();
            for (var i = 0; i < pinsCount; i++)
            {
                var pin = new PinWithConnection();
                pin.Deserialize(reader);
                InputPins.Add(pin);
            }

            pinsCount = reader.ReadInt32();
            for (var i = 0; i < pinsCount; i++)
            {
                var pin = new PinWithConnection();
                pin.Deserialize(reader);
                ExitPins.Add(pin);
            }

            pinsCount = reader.ReadInt32();
            for (var i = 0; i < pinsCount; i++)
            {
                var pin = new Pin();
                pin.Deserialize(reader);
                OutputPins.Add(pin);
            }
        }
    }
}