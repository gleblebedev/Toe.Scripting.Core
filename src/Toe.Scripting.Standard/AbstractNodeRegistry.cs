using System.Collections;
using System.Collections.Generic;

namespace Toe.Scripting
{
    public abstract class AbstractNodeRegistry : INodeRegistry
    {
        protected readonly List<INodeFactory> _nodeFactories = new List<INodeFactory>();

        public IEnumerator<INodeFactory> GetEnumerator()
        {
            return _nodeFactories.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(INodeFactory factory)
        {
            _nodeFactories.Add(factory);
        }
    }
}