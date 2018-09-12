using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Toe.Scripting.WPF.Views
{
    public class ViewUtils
    {
        public static IEnumerable<DependencyObject> GetHierarchy(DependencyObject element)
        {
            do
            {
                yield return element;
                element = VisualTreeHelper.GetParent(element);
            } while (element != null);
        }
        public static IInputElement FindFocusableParent(DependencyObject element)
        {
            return GetHierarchy(element).Select(_ => _ as IInputElement).Where(_ => _ != null)
                .FirstOrDefault(_ => _.Focusable);
        }
        public static Canvas FindCanvasParent(DependencyObject element)
        {
            return GetHierarchy(element).OfType<Canvas>().FirstOrDefault();
        }
    }
}
