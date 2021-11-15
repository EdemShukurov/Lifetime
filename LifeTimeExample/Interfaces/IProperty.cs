namespace LifetimeExample.Interfaces
{
    public interface IProperty<T> : IPropertyValue<T>, INotifyPropertyChanged<T>
    {        
    }
}