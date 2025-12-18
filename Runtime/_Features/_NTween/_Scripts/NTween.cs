using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public static class NTween
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void schedule(NTweener tweener)
        {
            tweener.schedule();
        }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void updateDefaultUpdateMode(NTweener.UpdateMode updateMode)
        {
            NTweener.defaultUpdateMode = updateMode;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="destination">Rad/s</param>
        /// <param name="duration"></param>
        /// <param name="isLocalSpace"></param>
        /// <returns></returns>
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
            var tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.Local_Scale);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener scaleTo(Transform target, float destination, float duration)
        {
            var tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.Uniform_Local_Scale);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="punchDestination">Rad/s</param>
        /// <param name="duration"></param>
        /// <param name="isLocalSpace"></param>
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
        public static NRunnableTweener punchScale(Transform target, float punchDestination, float duration)
        {
            NRunnableTweener tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.Uniform_Local_Scale);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="distance">Rad/s</param>
        /// <param name="duration"></param>
        /// <param name="isLocalSpace"></param>
        /// <returns></returns>
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
        public static NRunnableTweener shakeScale(Transform target, float distance, float duration, bool uniformScale = true)
        {
            NRunnableTweener tweener;
            if (uniformScale)
            {
                tweener = NTweenerCreator.createTransformShakeTweener<float>(target, distance, duration, TransformTweenType.Uniform_Local_Scale);
            }
            else
            {
                tweener = NTweenerCreator.createTransformShakeTweener<float3>(target, distance, duration, TransformTweenType.Local_Scale);
            }
            tweener.schedule();
            return tweener;
        }
        #endregion

        #region ValueTween
        public static NRunnableTweener fromTo(float from, float destination, float duration, Action<float> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener fromTo(float2 from, float2 destination, float duration, Action<float2> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener fromTo(float3 from, float3 destination, float duration, Action<float3> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener fromTo(float4 from, float4 destination, float duration, Action<float4> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }

        public static NRunnableTweener punchValue(float origin, float punchDestination, float duration, Action<float> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener punchValue(float2 origin, float2 punchDestination, float duration, Action<float2> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener punchValue(float3 origin, float3 punchDestination, float duration, Action<float3> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener punchValue(float4 origin, float4 punchDestination, float duration, Action<float4> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }

        public static NRunnableTweener shakeValue(float origin, float range, float duration, Action<float> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener shakeValue(float2 origin, float range, float duration, Action<float2> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener shakeValue(float3 origin, float range, float duration, Action<float3> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NRunnableTweener shakeValue(float4 origin, float range, float duration, Action<float4> onChanged)
        {
            NRunnableTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        #endregion
    }
}