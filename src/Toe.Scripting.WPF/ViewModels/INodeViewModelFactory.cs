namespace Toe.Scripting.WPF.ViewModels
{
    public interface INodeViewModelFactory
    {
        NodeViewModel Create(ScriptViewModel script, ScriptNode node);
    }
}