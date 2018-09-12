using System.Collections;
using System.Collections.Generic;

namespace Toe.Scripting
{
    public class EmptyNodeRegistry : INodeRegistry
    {
        public static readonly INodeRegistry Instance = new EmptyNodeRegistry();
        public IEnumerator<INodeFactory> GetEnumerator()
        {
            yield break;
        }

        public void Add(INodeFactory factory)
        {
            throw new System.InvalidOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}