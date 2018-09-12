using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Toe.Scripting.Helpers
{
    public class LinksList<T>:IEnumerable<LinkHelper<T>>
    {
        private readonly PinHelper<T> _pin;

        private List<LinkHelper<T>> _list = new List<LinkHelper<T>>();

        public LinksList(PinHelper<T> pin)
        {
            _pin = pin;
        }
        public int Count
        {
            get { return _list.Count; }
        }

        public LinkHelper<T> Last
        {
            get { return _list[_list.Count]; }
        }
        public LinkHelper<T> First
        {
            get { return _list[0]; }
        }

        public IEnumerable<PinHelper<T>> ConnectedPins
        {
            get
            {
                foreach (var link in _list)
                {
                    if (link.From == _pin)
                        yield return link.To;
                    else
                        yield return link.From;
                }
            }
        }

        public LinkHelper<T> this[int index]
        {
            get { return _list[index]; }
        }

        public IEnumerator<LinkHelper<T>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Add(LinkHelper<T> link)
        {
            _list.Add(link);
        }

        internal void Remove(LinkHelper<T> link)
        {
            _list.Remove(link);
        }

        public void Clear()
        {
            for (int i = _list.Count - 1; i >= 0; --i)
            {
                _pin.Node.Script.RemoveLink(_list[i]);
            }
        }
    }
}