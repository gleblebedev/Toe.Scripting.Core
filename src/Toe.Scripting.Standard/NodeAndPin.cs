using System;

namespace Toe.Scripting
{
    public class NodeAndPin : IEquatable<NodeAndPin>
    {
        public ScriptNode Node;
        public Pin Pin;

        public NodeAndPin(ScriptNode node, Pin pin)
        {
            Node = node;
            Pin = pin;
        }

        public NodeAndPin()
        {
        }

        public bool Equals(NodeAndPin other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Node, other.Node) && Equals(Pin, other.Pin);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NodeAndPin) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Node != null ? Node.GetHashCode() : 0) * 397) ^ (Pin != null ? Pin.GetHashCode() : 0);
            }
        }

        public static bool operator ==(NodeAndPin left, NodeAndPin right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NodeAndPin left, NodeAndPin right)
        {
            return !Equals(left, right);
        }
    }
}