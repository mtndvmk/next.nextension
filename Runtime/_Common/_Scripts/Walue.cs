using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    /// <summary>
    /// Invoke event when value changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Walue<T> : AbsNNotify
    {
        public Walue(T value = default)
        {
            setValueWithoutNotify(value);
        }
        private T value;

        public readonly NCallback<Walue<T>> onValueChangedEvent = new NCallback<Walue<T>>();
        public readonly NCallback<Walue<T>> onValueChangedOnceTimeEvent = new NCallback<Walue<T>>();

        protected override void onNotified()
        {
            onValueChangedEvent.tryInvoke(this, Debug.LogException);
            if (onValueChangedOnceTimeEvent.ListenerCount > 0)
            {
                onValueChangedOnceTimeEvent.tryInvoke(this, Debug.LogException);
                onValueChangedOnceTimeEvent.clear();
            }
        }

        public T Value
        {
            get => value;
            set
            {
                if (object.Equals(this.value, value)) return;
                setValueWithoutNotify(value);
                notify();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void setValueWithoutNotify(T value)
        {
            this.value = value;
        }
        public void removeAllListeners()
        {
            onValueChangedEvent.clear();
            onValueChangedOnceTimeEvent.clear();
        }
        public static explicit operator T(Walue<T> walue) => walue.value;
    }
}