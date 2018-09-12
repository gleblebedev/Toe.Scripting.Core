using System;
using System.Collections.Generic;
using System.IO;

namespace Toe.Scripting
{
    public class Pin : ICloneable, IEquatable<Pin>
    {
        private string _id = "";

        private string _type = "";

        public Pin()
        {
        }

        public Pin(string id, string type)
        {
            Id = id;
            Type = type;
        }

        public string Id
        {
            get => _id;
            set => _id = value ?? "";
        }

        public string Type
        {
            get => _type;
            set => _type = value ?? "";
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Pin Clone()
        {
            var clone = new Pin
            {
                Id = Id,
                Type = Type
            };
            return clone;
        }

        public PinWithConnection AsPinWithConnection()
        {
            var clone = new PinWithConnection
            {
                Id = Id,
                Type = Type
            };
            return clone;
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(_type);
            writer.Write(_id);
        }

        public virtual void Serialize(TextWriter writer)
        {
            writer.WriteLine("        {");
            writer.Write("          \"Id\": \"");
            writer.Write(Id);
            writer.WriteLine("\",");
            writer.Write("          \"Type\": \"");
            writer.Write(Type);
            writer.WriteLine("\"");
            writer.Write("        }");
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            _type = reader.ReadString();
            _id = reader.ReadString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Pin);
        }

        public bool Equals(Pin other)
        {
            return other != null &&
                   _id == other._id &&
                   _type == other._type;
        }

        public override int GetHashCode()
        {
            var hashCode = -1248223815;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_type);
            return hashCode;
        }

        public static bool operator ==(Pin pin1, Pin pin2)
        {
            return EqualityComparer<Pin>.Default.Equals(pin1, pin2);
        }

        public static bool operator !=(Pin pin1, Pin pin2)
        {
            return !(pin1 == pin2);
        }
    }
}