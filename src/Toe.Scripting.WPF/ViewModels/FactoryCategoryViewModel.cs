using System.Collections.Generic;

namespace Toe.Scripting.WPF.ViewModels
{
    public class FactoryCategoryViewModel
    {
        public IList<FactoryViewModel> Factories { get; set; } = new List<FactoryViewModel>();
        public IList<FactoryCategoryViewModel> Categories { get; set; } = new List<FactoryCategoryViewModel>();
        public string Name { get; set; }
    }
}