using System;
using System.Collections.Generic;
using System.IO;

namespace Toe.Scripting
{
    public sealed class PinWithConnection : Pin, ICloneable, IEquatable<PinWithConnection>
    {
        public Connection Connection { get; set; }

        public PinWithConnection()
        {
        }

        public PinWithConnection(string id, string type):base(id,type)
        {
        }

        public PinWithConnection(string id, string type, Connection connection) : base(id, type)
        {
            Connection = connection;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public new PinWithConnection Clone()
        {
            var clone = new PinWithConnection
            {
                Id = Id,
                Type = Type
            };
            if (Connection != null)
                clone.Connection = Connection.Clone();
            return clone;
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Connection != null);
            if (Connection != null) Connection.Serialize(writer);
        }

        public override void Serialize(TextWriter writer)
        {
            writer.WriteLine("        {");
            writer.Write("          \"Name\": \"");
            writer.Write(Id);
            writer.WriteLine("\",");
            writer.Write("          \"Type\": \"");
            writer.Write(Type);
            writer.Write("\"");
            if (Connection == null)
            {
                writer.WriteLine();
            }
            else
            {
                writer.WriteLine(",");
                writer.WriteLine("          \"Connection\": {");
                writer.Write("            \"NodeId\": \"");
                writer.Write(Connection.NodeId);
                writer.WriteLine("\",");
                writer.Write("            \"PinName\": \"");
                writer.Write(Connection.PinId);
                writer.WriteLine("\"");
                writer.WriteLine("          }");
            }

            writer.Write("        }");
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            if (reader.ReadBoolean())
            {
                Connection = new Connection();
                Connection.Deserialize(reader);
            }
            else
            {
                Connection = null;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PinWithConnection);
        }

        public bool Equals(PinWithConnection other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<Connection>.Default.Equals(Connection, other.Connection);
        }

        public override int GetHashCode()
        {
            var hashCode = 790373007;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Connection>.Default.GetHashCode(Connection);
            return hashCode;
        }

        public static bool operator ==(PinWithConnection connection1, PinWithConnection connection2)
        {
            return EqualityComparer<PinWithConnection>.Default.Equals(connection1, connection2);
        }

        public static bool operator !=(PinWithConnection connection1, PinWithConnection connection2)
        {
            return !(connection1 == connection2);
        }
    }
}