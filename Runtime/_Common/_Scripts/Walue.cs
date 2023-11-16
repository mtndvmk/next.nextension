using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    /// <summary>
    /// Invoke event when value changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Walue<T> : AbsNNotify
        where T : IEquatable<T>
    {
        public Walue()
        {

        }
        public Walue(T value)
        {
            setValueWithoutNotify(value);
        }
        public Walue(T value, Action<T> onValueChangedEvent)
        {
            setValueWithoutNotify(value);
            this.onValueChangedEvent.add(onValueChangedEvent);
        }
        private T _value;

        public readonly NCallback<T> onValueChangedEvent = new();
        public readonly NCallback<T> onValueChangedOnceTimeEvent = new();

        protected override void onNotified()
        {
            onValueChangedEvent.tryInvoke(_value);
            if (onValueChangedOnceTimeEvent.Count > 0)
            {
                onValueChangedOnceTimeEvent.tryInvoke(_value);
                onValueChangedOnceTimeEvent.clear();
            }
        }

        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value)) return;
                setValueWithoutNotify(value);
                notify();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void setValueWithoutNotify(T value)
        {
            this._value = value;
        }
        public void removeAllListeners()
        {
            onValueChangedEvent.clear();
            onValueChangedOnceTimeEvent.clear();
        }
        public static implicit operator T(Walue<T> walue) => walue._value;
    }
}