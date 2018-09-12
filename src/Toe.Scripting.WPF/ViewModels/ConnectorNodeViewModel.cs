namespace Toe.Scripting.WPF.ViewModels
{
    public class ConnectorNodeViewModel : NodeViewModel
    {
        public ConnectorNodeViewModel() : base()
        {
            this.HasGroupPins = true;
        }

        public ConnectorNodeViewModel(ScriptViewModel script, ScriptNode node):base(script,node)
        {
            this.HasGroupPins = true;
        }
    }
}