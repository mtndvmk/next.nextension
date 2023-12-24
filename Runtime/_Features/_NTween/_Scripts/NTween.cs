using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public static class NTween
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void cancelAllTweeners(uint key)
        {
            NTweenManager.cancelFromUintControlKey(key);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void cancelAllTweeners(UnityEngine.Object key)
        {
            NTweenManager.cancelFromObjectControlKey(key);
        }
        #region Transform Tween
        public static NRunnableTweener moveTo(Transform target, float3 destination, float duration, bool isLocalSpace = true)
        {
            NRunnableTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener rotateTo(Transform target, float4 destination, float duration, bool isLocalSpace = true)
        {
            NRunnableTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.Local_Rotation);
            }
            else
            {
                tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.World_Rotation);
            }
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener scaleTo(Transform target, float3 destination, float duration)
        {
            NRunnableTweener tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.Local_Scale);
            tweener.schedule();
            return tweener;
        }

        public static NRunnableTweener punchPosition(Transform target, float3 punchDestination, float duration, bool isLocalSpace = true)
        {
            NRunnableTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener punchRotation(Transform target, float4 punchDestination, float duration, bool isLocalSpace = true)
        {
            NRunnableTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.Local_Rotation);
            }
            else
            {
                tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.World_Rotation);
            }
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener punchScale(Transform target, float3 punchDestination, float duration)
        {
            NRunnableTweener tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.Local_Scale);
            tweener.schedule();
            return tweener;
        }

        public static NRunnableTweener shakePosition(Transform target, float distance, float duration, bool isLocalSpace = true)
        {
            NRunnableTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransformShakeTweener<float3>(target, distance, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransformShakeTweener<float3>(target, distance, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener shakeRotation(Transform target, float distance, float duration, bool isLocalSpace = true)
        {
            NRunnableTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransformShakeTweener<float4>(target, distance, duration, TransformTweenType.Local_Rotation);
            }
            else
            {
                tweener = NTweenerCreator.createTransformShakeTweener<float4>(target, distance, duration, TransformTweenType.World_Rotation);
            }
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener shakeScale(Transform target, float distance, float duration)
        {
            NRunnableTweener tweener = NTweenerCreator.createTransformShakeTweener<float3>(target, distance, duration, TransformTweenType.Local_Scale);
            tweener.schedule();
            return tweener;
        }
        #endregion

        #region ValueTween
        public static NRunnableTweener fromTo<T>(T from, T destination, Action<T> onChanged, float duration) where T : unmanaged
        {
            NRunnableTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }

        public static NRunnableTweener punchValue<T>(T from, T destination, Action<T> onChanged, float duration) where T : unmanaged
        {
            NRunnableTweener tweener = NTweenerCreator.createPunchValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }

        public static NRunnableTweener shakeValue<T>(T from, float range, Action<T> onChanged, float duration) where T : unmanaged
        {
            NRunnableTweener tweener = NTweenerCreator.createShakeValueTweener(from, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        #endregion
    }
}