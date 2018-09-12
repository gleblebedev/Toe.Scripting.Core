using System;
using System.IO;

namespace Toe.Scripting
{
    public class Connection : IEquatable<Connection>, ICloneable
    {
        private string _pinId = "";

        public Connection()
        {
        }

        public Connection(NodeAndPin nodeAndPin)
        {
            NodeId = nodeAndPin.Node.Id;
            PinId = nodeAndPin.Pin.Id;
        }

        public Connection(ScriptNode node, Pin pin)
        {
            NodeId = node.Id;
            PinId = pin.Id;
        }

        public Connection(int nodeId, string pinId)
        {
            NodeId = nodeId;
            PinId = pinId;
        }

        public int NodeId { get; set; }

        public string PinId
        {
            get => _pinId;
            set => _pinId = value ?? "";
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public bool Equals(Connection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NodeId == other.NodeId && string.Equals(PinId, other.PinId);
        }

        public static Connection Clone(Connection connection)
        {
            if (connection == null)
                return null;
            return new Connection(connection.NodeId, connection.PinId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Connection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NodeId * 397) ^ (PinId != null ? PinId.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Connection left, Connection right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Connection left, Connection right)
        {
            return !Equals(left, right);
        }

        public Connection Clone()
        {
            var clone = new Connection
            {
                NodeId = NodeId,
                PinId = PinId
            };
            return clone;
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(NodeId);
            writer.Write(PinId);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            NodeId = reader.ReadInt32();
            PinId = reader.ReadString();
        }
    }
}