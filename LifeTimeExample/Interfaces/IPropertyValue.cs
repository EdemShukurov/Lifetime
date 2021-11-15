namespace LifetimeExample.Interfaces
{
    public interface IPropertyValue<T>
    {
        T Value { get; set; }
    }
}