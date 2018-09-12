namespace Toe.Scripting.WPF.ViewModels
{
    public class NodeViewModelFactory : INodeViewModelFactory
    {
        private readonly INodeRegistry _registry;

        public NodeViewModelFactory(INodeRegistry registry)
        {
            _registry = registry;
        }
        public NodeViewModel Create(ScriptViewModel script, ScriptNode node)
        {
            //if (node.Category == NodeCategory.Event)
            //    return new EventNodeViewModel(script, node);
            if (node.Category == NodeCategory.Value)
                return new ConstantNodeViewModel(script, node);
            return new NodeViewModel(script, node);
        }
    }
}