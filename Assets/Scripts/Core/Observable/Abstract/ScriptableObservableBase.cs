using System;
using UnityEngine;

namespace Core.Observable.Abstract
{
    public abstract class ScriptableObservableBase<T> : ScriptableObject, Interface.IObservable<T>
    {
        public T Value { get; private set; }
        
        private event Action<T> onValueChanged;
        public event Action<T> OnValueChanged
        {
            add => onValueChanged += value;
            remove => onValueChanged -= value;
        }

        public void SetValue(T value)
        {
            Value = value;
            onValueChanged?.Invoke(value);
        }
    }
}