using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Toe.Scripting.Helpers
{
    public class NodeList<T>:IEnumerable<NodeHelper<T>>
    {
        readonly HashSet<NodeHelper<T>> _nodes = new HashSet<NodeHelper<T>>();
        private ScriptHelper<T> _scriptHelper;

        public NodeList(ScriptHelper<T> scriptHelper)
        {
            _scriptHelper = scriptHelper;
        }

        public int Count
        {
            get { return _nodes.Count; }
        }

        public IEnumerator<NodeHelper<T>> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Remove(NodeHelper<T> nodeHelper)
        {
            if (nodeHelper.Script != _scriptHelper)
                throw new InvalidOperationException();
            nodeHelper.RemoveAllLinks();
            nodeHelper.Script = null;
            _nodes.Remove(nodeHelper);
        }
        public void RemoveWhere(Func<NodeHelper<T>, bool> predicate)
        {
            foreach (var node in this.ToList())
            {
                if (predicate(node))
                    Remove(node);
            }
        }
        public void Add(NodeHelper<T> nodeHelper)
        {
            if (nodeHelper == null)
                throw new ArgumentNullException(nameof(nodeHelper), "Node is null");
            if (nodeHelper.Script == _scriptHelper)
                return;
            if (nodeHelper.Script != null)
                nodeHelper.Script.Nodes.Remove(nodeHelper);
            nodeHelper.Script = _scriptHelper;
            _nodes.Add(nodeHelper);
        }

        public void RemoveIf(Func<NodeHelper<T>, bool> predicate)
        {
            foreach (var nodeHelper in _nodes.ToList())
            {
                if (predicate(nodeHelper))
                    Remove(nodeHelper);
            }
        }

        public NodeHelper<T> FindByType(string nodeType)
        {
            //TODO: Optimize with lookup?
            foreach (var nodeHelper in this)
            {
                if (nodeHelper.Type == nodeType)
                    return nodeHelper;
            }

            return null;
        }
    }
}