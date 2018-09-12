using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Toe.Scripting.WPF.ViewModels;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(IList<MenuItemViewModel>), typeof(IList<MenuItem>))]
    public class MenuItemViewModelToMenuItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var viewModel = value as IEnumerable<MenuItemViewModel>;
            if (viewModel == null)
                return null;
            return viewModel.Select(_ => CreateMenuItem(_)).ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private MenuItem CreateMenuItem(MenuItemViewModel viewModel)
        {
            //if (viewModel.Header == "-" && viewModel.Command == null)
            //{
            //    return new Separator();
            //}
            var item = new MenuItem {DataContext = viewModel};
            if (viewModel.Command != null)
                item.Command = new ScriptingCommand(viewModel.Command);
            var header = new Binding(nameof(viewModel.Header));
            BindingOperations.SetBinding(item, HeaderedItemsControl.HeaderProperty, header);
            if (viewModel.SubmenuItems != null)
                foreach (var subitem in viewModel.SubmenuItems)
                    item.Items.Add(CreateMenuItem(subitem));
            return item;
        }
    }
}