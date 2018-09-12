using System;
using System.Windows;
using System.Windows.Controls;
using Toe.Scripting.WPF.ViewModels;
using Toe.Scripting.WPF.Views;

namespace Toe.Scripting.WPF
{
    public class ScriptControl : UserControl
    {
        public static readonly DependencyProperty ScriptProperty =
            DependencyProperty.Register(
                "Script", typeof(Script),
                typeof(ScriptControl),
                new PropertyMetadata(new Script(), ScriptPropertyChanged)
            );

        public static readonly DependencyProperty NodeRegistryProperty =
            DependencyProperty.Register(
                "NodeRegistry", typeof(INodeRegistry),
                typeof(ScriptControl),
                new PropertyMetadata(null, NodeRegistryPropertyChanged)
            );

        public static readonly DependencyProperty NodeViewModelFactoryProperty =
            DependencyProperty.Register(
                "NodeViewModelFactory", typeof(INodeViewModelFactory),
                typeof(ScriptControl),
                new PropertyMetadata(new NodeViewModelFactory(EmptyNodeRegistry.Instance), NodeViewModelFactoryPropertyChanged)
            );

        private readonly ScriptView _scriptView;
        private ScriptViewModel _viewModel;

        public ScriptControl()
        {
            Content = _scriptView = new ScriptView();
            RefreshViewModel();
        }

        public Script Script
        {
            get => (Script) GetValue(ScriptProperty);
            set => SetValue(ScriptProperty, value);
        }

        public INodeRegistry NodeRegistry
        {
            get => (INodeRegistry) GetValue(NodeRegistryProperty);
            set => SetValue(NodeRegistryProperty, value);
        }

        public INodeViewModelFactory NodeViewModelFactory
        {
            get => (INodeViewModelFactory) GetValue(NodeViewModelFactoryProperty);
            set => SetValue(NodeViewModelFactoryProperty, value);
        }

        private static void ScriptPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScriptControl) d).RefreshViewModel();
        }

        private static void NodeRegistryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScriptControl) d).RefreshViewModel();
        }

        private static void NodeViewModelFactoryPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((ScriptControl) d).RefreshViewModel();
        }

        private void RefreshViewModel()
        {
            if (_viewModel != null) _viewModel.ScriptChanged -= FireScriptChanged;

            if (NodeRegistry == null)
                _scriptView.DataContext = _viewModel = null;
            else
                _scriptView.DataContext = _viewModel = new ScriptViewModel(NodeRegistry, NodeViewModelFactory, Script);
            if (_viewModel != null) _viewModel.ScriptChanged += FireScriptChanged;
        }

        private void FireScriptChanged(object sender, ScriptChangedEventArgs e)
        {
            ScriptChanged?.Invoke(this, e);
        }

        public event EventHandler<ScriptChangedEventArgs> ScriptChanged;
    }
}