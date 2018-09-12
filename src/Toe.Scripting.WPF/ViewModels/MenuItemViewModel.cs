using System;
using System.Collections.Generic;

namespace Toe.Scripting.WPF.ViewModels
{
    public class MenuItemViewModel : ViewModelBase
    {
        private string _header;
        private IList<MenuItemViewModel> _submenuItems;

        public string Header
        {
            get => _header;
            set => RaiseAndSetIfChanged(ref _header, value);
        }

        public Action Command { get; set; }

        public IList<MenuItemViewModel> SubmenuItems
        {
            get => _submenuItems;
            set => RaiseAndSetIfChanged(ref _submenuItems, value);
        }
    }
}