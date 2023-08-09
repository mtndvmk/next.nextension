using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension.Tween
{
    internal class BasicTweener<T> : NTweener
        where T : struct
    {
        public T from;
        public T destination;
        public Action<T> onValueChanged;

        public BasicTweener(T from, T to, Action<T> onValueChanged, TweenType tweenType) : base(tweenType)
        {
            this.from = from;
            this.destination = to;
            this.onValueChanged = onValueChanged;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeValueChanged(T value)
        {
            try
            {
                onValueChanged(value);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void doCompleteOnStart()
        {
            try
            {
                this.onValueChanged(destination);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual JobData<T> toJobData()
        {
            return new JobData<T>()
            {
                tweenType = this.tweenType,
                from = this.from,
                to = this.destination,
                duration = this.duration,
                easeType = this.easeType,
                startTime = this.startTime,
                tweenLoopType = this.tweenLoopType,
            };
        }
    }
}