using Nextension.Tween;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nextension
{
    internal abstract class AbsValueTweener<TValue, TJobData> : GenericNRunnableTweener<TJobData>
        where TValue : unmanaged
        where TJobData : struct
    {
        private Action<TValue> onValueChanged;
        internal AbsValueTweener(Action<TValue> onValueChanged)
        {
#if UNITY_EDITOR
            if (TweenSupportedDataType<TValue>.type == SupportedDataType.NotSupported)
            {
                throw new NotSupportedException();
            }
#endif
            this.onValueChanged = onValueChanged;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void invokeValueChanged(TValue value)
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

        public void updateDuration(float duration)
        {
            this.duration = duration;
        }
    }
}
