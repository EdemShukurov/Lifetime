using System;

namespace LifetimeExample.Interfaces
{
    public interface INotifyPropertyChanged<out T>
    {
        event Action<T> OnPropertyChanged;
    }
}