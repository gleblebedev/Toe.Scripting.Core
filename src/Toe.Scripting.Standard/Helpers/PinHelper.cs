using System;
using System.Collections.Generic;

namespace Toe.Scripting.Helpers
{
    public abstract class PinHelper
    {
        public string Id { get; protected set; }
        public string Type { get; protected set; }
        public Pin BuildPin()
        {
            return new Pin { Type = Type, Id = Id };
        }
    }
    public class PinHelper<T>: PinHelper
    {
        public PinHelper(Pin pin):this(pin.Id, pin.Type)
        {
        }
        public PinHelper(string type)
        {
            Id = "";
            Type = type;
            Links = new LinksList<T>(this);
        }
        public PinHelper(string id, string type)
        {
            Id = id;
            Type = type;
            Links = new LinksList<T>(this);
        }
        public NodeHelper<T> Node { get; internal set; }

        public LinksList<T> Links { get; }

        public IEnumerable<PinHelper<T>> ConnectedPins
        {
            get { return Links.ConnectedPins; }
        }

        public PinWithConnection BuildPinWithConnection()
        {
            var pin = new PinWithConnection {Type = Type, Id = Id};
            if (Links.Count > 0)
            {
                var link = Links[0];
                if (link.From == this)
                    pin.Connection = new Connection(link.To.Node.Id, link.To.Id);
                else
                    pin.Connection = new Connection(link.From.Node.Id, link.From.Id);
            }

            return pin;
        }

        public PinHelper<T> Clone(NodeHelper<T> cloneNode)
        {
            return new PinHelper<T>(Id, Type);
        }


        public void RemoveLinks()
        {
            Links.Clear();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}), {2} connections", Id, Type, Links.Count);
        }
    }
}