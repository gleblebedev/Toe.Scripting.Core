using System;
using System.Collections;
using System.Collections.Generic;

namespace Toe.Scripting
{
    public class Collection<T> : IList<T> where T : ScriptItem
    {
        public const int InvalidId = 0;

        private readonly Dictionary<int, T> _values = new Dictionary<int, T>();

        private int _nextIndex = 1;

        public IEnumerator<T> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(T item)
        {
            if (item.Id == InvalidId)
            {
                item.Id = _nextIndex;
                ++_nextIndex;
            }
            else
            {
                if (item.Id >= _nextIndex) _nextIndex = item.Id + 1;
            }

            _values.Add(item.Id, item);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(T item)
        {
            if (item.Id == InvalidId)
                return false;
            return _values.ContainsKey(item.Id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _values.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _values.Remove(item.Id);
        }

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            if (item.Id == InvalidId)
                return InvalidId;
            return item.Id;
        }

        public void Insert(int index, T item)
        {
            if (index == InvalidId)
                throw new ArgumentException("Invalid index");
            item.Id = index;
            Add(item);
        }

        public void RemoveAt(int index)
        {
            if (index == InvalidId)
                throw new ArgumentException("Invalid index");
            _values.Remove(index);
        }

        public T this[int index]
        {
            get
            {
                if (index == InvalidId)
                    throw new ArgumentException("Invalid index");
                T v;
                if (!_values.TryGetValue(index, out v))
                    return default(T);
                return v;
            }
            set
            {
                if (index == InvalidId)
                    throw new ArgumentException("Invalid index");
                value.Id = index;
                _values[index] = value;
            }
        }
    }
}