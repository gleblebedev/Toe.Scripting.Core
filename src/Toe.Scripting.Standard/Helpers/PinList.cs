using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toe.Scripting.Helpers
{
    public class PinList<T>:IEnumerable<PinHelper<T>>
    {
        private List<PinHelper<T>> _list = new List<PinHelper<T>>();

        private readonly NodeHelper<T> _node;

        public int Count
        {
            get { return _list.Count; }
        }
        public void RemoveAt(int index)
        {
            var pin = _list[index];
            pin.RemoveLinks();
            pin.Node = null;
            _list.RemoveAt(index);
        }
        public PinList(NodeHelper<T> node)
        {
            _node = node;
        }
        public PinList(NodeHelper<T> node, IEnumerable<PinHelper<T>> pins)
        {
            _node = node;
            foreach (var pinHelper in pins)
            {
                pinHelper.Node = _node;
                _list.Add(pinHelper);
            }
        }

        public IEnumerable<LinkHelper<T>> Links
        {
            get
            {
                foreach (var pinHelper in _list)
                {
                    foreach (var link in pinHelper.Links)
                    {
                        yield return link;
                    }
                }
            }
        }

        public IEnumerable<PinHelper<T>> ConnectedPins
        {
            get
            {
                foreach (var pinHelper in _list)
                {
                    foreach (var pin in pinHelper.ConnectedPins)
                    {
                        yield return pin;
                    }
                }
            }
        }

        public PinList(NodeHelper<T> node, IEnumerable<Pin> pins)
        {
            _node = node;
            foreach (var pinHelper in pins.Select(_ => new PinHelper<T>(_)))
            {
                pinHelper.Node = _node;
                _list.Add(pinHelper);
            }
        }

        public PinHelper<T> this[string name]
        {
            get { return _list.FirstOrDefault(_ => _.Id == name); }
        }
        public PinHelper<T> this[int index]
        {
            get { return _list[index]; }
        }
        public PinHelper<T> First()
        {
            return _list[0];
        }
        public PinHelper<T> Last()
        {
            return _list[_list.Count-1];
        }
        public IEnumerator<PinHelper<T>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < 3; ++i)
            {
                if (i != 0)
                    sb.Append(", ");
                sb.Append(this[i]);
            }
            if (Count > 3)
                sb.Append(", ...");
            sb.Append("]");
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddRange(IEnumerable<PinHelper<T>> pins)
        {
            foreach (var pin in pins)
            {
                Add(pin);
            }
        }

        public void Add(PinHelper<T> pin)
        {
            pin.Node = _node;
            _list.Add(pin);
        }

        public void Clear()
        {
            for (int i = _list.Count - 1; i >= 0; --i)
            {
                RemoveAt(i);
            }
        }

        public void RemoveWhere(Func<PinHelper<T>, bool> func)
        {
            for (int i = _list.Count - 1; i >= 0; --i)
            {
                if (func(_list[i]))
                {
                    RemoveAt(i);
                }
            }
        }
    }
}