using System;
using LifetimeExample.Interfaces;
using LifetimeExample.Monads;
using LifetimeUtility;

namespace LifetimeExample
{
    public class Property<T>: IProperty<T>
    {
        public static Property<T> Create(Lifetime lifetime)
        {
            var property = new Property<T>();
            lifetime.Add(property.ClearSubscribers);

            return property;
        }
        
        private void ClearSubscribers ()
        {
            OnPropertyChanged = null;
        }
        
        private Maybe<T> _value;
        
        public event Action<T> OnPropertyChanged;

        public T Value
        {
            get => _value.Value;
            set
            {
                if (!_value.HasValue || !_value.Value.Equals(value))
                {
                    _value = new Maybe<T>(value);

                    OnPropertyChanged?.Invoke(_value.Value);
                }
            }
        }

        public Property(T value)
        {
            _value = new Maybe<T>(value);
        }

        private Property()
        {
            _value = new Maybe<T>();
        }
        
        public static implicit operator T(Property<T> v)
        {
            return v.Value;
        }
    }
}