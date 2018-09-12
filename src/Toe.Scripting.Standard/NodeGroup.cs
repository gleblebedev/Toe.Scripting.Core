using System;
using System.IO;

namespace Toe.Scripting
{
    public sealed class NodeGroup : ScriptItem, ICloneable
    {
        private string _name = "";

        public string Name
        {
            get => _name;
            set => _name = value ?? "";
        }

        public bool IsCollapsed { get; set; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public NodeGroup Clone()
        {
            var clone = CloneNew();
            clone.Id = Id;
            return clone;
        }

        public NodeGroup CloneNew()
        {
            var clone = new NodeGroup
            {
                Name = Name,
                IsCollapsed = IsCollapsed
            };
            return clone;
        }

        public override string ToString()
        {
            if (Id != Collection<NodeGroup>.InvalidId) return string.Format("UID:{0} {1} ({2})", Id, Name);

            return string.Format("{0} ({1})", Name);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Name);
            writer.Write(IsCollapsed);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Name = reader.ReadString();
            IsCollapsed = reader.ReadBoolean();
        }
    }
}