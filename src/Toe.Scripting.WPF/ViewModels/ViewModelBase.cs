using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Toe.Scripting.WPF.Annotations;

namespace Toe.Scripting.WPF.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            //var isNull = ((object) field) == null;
            //var isValueNull = ((object)value) == null;
            //if (isNull && isValueNull)
            //    return false;

            //isNull || isValueNull || 
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }
    }
}