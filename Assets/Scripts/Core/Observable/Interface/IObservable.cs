using System;

namespace Core.Observable.Interface
{
    public interface IObservable<T>
    {
        T Value { get; }
        event Action<T> OnValueChanged;
        void SetValue(T value);
    }
}