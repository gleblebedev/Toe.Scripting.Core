using System.Windows;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Views
{
    /// <summary>
    /// Interaction logic for ScriptDialog.xaml
    /// </summary>
    public partial class ScriptDialog : Window
    {
        public ScriptDialog(Script script, INodeRegistry registry = null,  INodeViewModelFactory viewModelFactory = null)
        {
            InitializeComponent();
            this.DataContext = new ScriptViewModel(registry, viewModelFactory, script);
        }
    }
}
