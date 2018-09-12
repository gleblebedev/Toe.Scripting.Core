using System.Collections;
using System.Collections.Generic;

namespace Toe.Scripting
{
    public abstract class AbstractNodeByTypeRegistry : INodeRegistry
    {
        protected readonly Dictionary<string, INodeFactory> _nodeFactories = new Dictionary<string, INodeFactory>();

        public IEnumerator<INodeFactory> GetEnumerator()
        {
            return _nodeFactories.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(INodeFactory factory)
        {
            _nodeFactories.Add(factory.Type, factory);
        }

        public virtual bool TryResolve(string typeKey, out INodeFactory factory)
        {
            return _nodeFactories.TryGetValue(typeKey, out factory);
        }

        public virtual INodeFactory Resolve(string typeKey)
        {
            INodeFactory factory;
            if (!TryResolve(typeKey, out factory))
                throw new KeyNotFoundException("Factory \"" + typeKey + "\" isn't found.");
            return factory;
        }
    }
}