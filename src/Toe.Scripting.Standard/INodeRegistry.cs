using System.Collections.Generic;

namespace Toe.Scripting
{
    public interface INodeRegistry : IEnumerable<INodeFactory>
    {
        void Add(INodeFactory factory);
    }
}