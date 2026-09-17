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
        public static void updateDefaultUpdateMode(NUpdateMode updateMode)
        {
            NTweener.defaultUpdateMode = updateMode;
        }

        #region Transform Tween
        public static NTweener moveTo(Transform target, float3 destination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
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
        public static NTweener rotateTo(Transform target, float4 destination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
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
        public static NTweener scaleTo(Transform target, float3 destination, float duration)
        {
            var tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.Local_Scale);
            tweener.schedule();
            return tweener;
        }
        public static NTweener scaleTo(Transform target, float destination, float duration)
        {
            var tweener = NTweenerCreator.createTransformFromToTweener(target, destination, duration, TransformTweenType.Uniform_Local_Scale);
            tweener.schedule();
            return tweener;
        }

        public static NTweener moveTo(Transform target, Transform destination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransform2FromToTweener<float3>(target, destination, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2FromToTweener<float3>(target, destination, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener rotateTo(Transform target, Transform destination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransform2FromToTweener<float4>(target, destination, duration, TransformTweenType.Local_Rotation);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2FromToTweener<float4>(target, destination, duration, TransformTweenType.World_Rotation);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener scaleTo(Transform target, Transform destination, float duration, bool uniformScale = false)
        {
            NTweener tweener;
            if (uniformScale)
            {
                tweener = NTweenerCreator.createTransform2FromToTweener<float>(target, destination, duration, TransformTweenType.Uniform_Local_Scale);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2FromToTweener<float3>(target, destination, duration, TransformTweenType.Local_Scale);
            }
            tweener.schedule();
            return tweener;
        }

        public static NTweener punchPosition(Transform target, float3 punchDestination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
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
        public static NTweener punchRotation(Transform target, float4 punchDestination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
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
        public static NTweener punchScale(Transform target, float3 punchDestination, float duration)
        {
            NTweener tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.Local_Scale);
            tweener.schedule();
            return tweener;
        }
        public static NTweener punchScale(Transform target, float punchDestination, float duration)
        {
            NTweener tweener = NTweenerCreator.createTransformPunchTweener(target, punchDestination, duration, TransformTweenType.Uniform_Local_Scale);
            tweener.schedule();
            return tweener;
        }

        public static NTweener punchPosition(Transform target, Transform punchDestination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransform2PunchTweener<float3>(target, punchDestination, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2PunchTweener<float3>(target, punchDestination, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener punchRotation(Transform target, Transform punchDestination, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransform2PunchTweener<float4>(target, punchDestination, duration, TransformTweenType.Local_Rotation);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2PunchTweener<float4>(target, punchDestination, duration, TransformTweenType.World_Rotation);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener punchScale(Transform target, Transform punchDestination, float duration, bool uniformScale = false)
        {
            NTweener tweener;
            if (uniformScale)
            {
                tweener = NTweenerCreator.createTransform2PunchTweener<float>(target, punchDestination, duration, TransformTweenType.Uniform_Local_Scale);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2PunchTweener<float3>(target, punchDestination, duration, TransformTweenType.Local_Scale);
            }
            tweener.schedule();
            return tweener;
        }

        public static NTweener shakePosition(Transform target, float4 distance, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
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
        public static NTweener shakePosition(Transform target, float3 distance, float duration, bool isLocalSpace = true)
        {
            return shakePosition(target, new float4(distance, 0f), duration, isLocalSpace);
        }
        public static NTweener shakePosition(Transform target, float distance, float duration, bool isLocalSpace = true)
        {
            return shakePosition(target, new float4(distance), duration, isLocalSpace);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="distance">Rad/s</param>
        /// <param name="duration"></param>
        /// <param name="isLocalSpace"></param>
        /// <returns></returns>
        public static NTweener shakeRotation(Transform target, float4 distance, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
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
        public static NTweener shakeRotation(Transform target, float3 distance, float duration, bool isLocalSpace = true)
        {
            return shakeRotation(target, new float4(distance, 0f), duration, isLocalSpace);
        }
        public static NTweener shakeRotation(Transform target, float distance, float duration, bool isLocalSpace = true)
        {
            return shakeRotation(target, new float4(distance), duration, isLocalSpace);
        }

        public static NTweener shakeScale(Transform target, float4 distance, float duration, bool uniformScale = true)
        {
            NTweener tweener;
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
        public static NTweener shakeScale(Transform target, float3 distance, float duration, bool uniformScale = false)
        {
            return shakeScale(target, new float4(distance, 0f), duration, uniformScale);
        }
        public static NTweener shakeScale(Transform target, float distance, float duration, bool uniformScale = true)
        {
            return shakeScale(target, new float4(distance), duration, uniformScale);
        }

        public static NTweener shakePosition(Transform target, Transform destination, float4 distance, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransform2ShakeTweener<float3>(target, destination, distance, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2ShakeTweener<float3>(target, destination, distance, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener shakePosition(Transform target, Transform destination, float3 distance, float duration, bool isLocalSpace = true)
        {
            return shakePosition(target, destination, new float4(distance, 0f), duration, isLocalSpace);
        }
        public static NTweener shakePosition(Transform target, Transform destination, float distance, float duration, bool isLocalSpace = true)
        {
            return shakePosition(target, destination, new float4(distance), duration, isLocalSpace);
        }

        public static NTweener shakeRotation(Transform target, Transform destination, float4 distance, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransform2ShakeTweener<float4>(target, destination, distance, duration, TransformTweenType.Local_Rotation);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2ShakeTweener<float4>(target, destination, distance, duration, TransformTweenType.World_Rotation);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener shakeRotation(Transform target, Transform destination, float3 distance, float duration, bool isLocalSpace = true)
        {
            return shakeRotation(target, destination, new float4(distance, 0f), duration, isLocalSpace);
        }
        public static NTweener shakeRotation(Transform target, Transform destination, float distance, float duration, bool isLocalSpace = true)
        {
            return shakeRotation(target, destination, new float4(distance), duration, isLocalSpace);
        }

        public static NTweener shakeScale(Transform target, Transform destination, float4 distance, float duration, bool uniformScale = true)
        {
            NTweener tweener;
            if (uniformScale)
            {
                tweener = NTweenerCreator.createTransform2ShakeTweener<float>(target, destination, distance, duration, TransformTweenType.Uniform_Local_Scale);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2ShakeTweener<float3>(target, destination, distance, duration, TransformTweenType.Local_Scale);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener shakeScale(Transform target, Transform destination, float3 distance, float duration, bool uniformScale = false)
        {
            return shakeScale(target, destination, new float4(distance, 0f), duration, uniformScale);
        }
        public static NTweener shakeScale(Transform target, Transform destination, float distance, float duration, bool uniformScale = true)
        {
            return shakeScale(target, destination, new float4(distance), duration, uniformScale);
        }

        public static NTweener jumpTo(Transform target, float3 destination, float3 jumpHeight, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransformJumpTweener(target, destination, jumpHeight, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransformJumpTweener(target, destination, jumpHeight, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener jumpTo(Transform target, float3 destination, float jumpHeight, float duration, bool isLocalSpace = true)
        {
            return jumpTo(target, destination, new float3(0f, jumpHeight, 0f), duration, isLocalSpace);
        }

        public static NTweener jumpTo(Transform target, Transform destination, float3 jumpHeight, float duration, bool isLocalSpace = true)
        {
            NTweener tweener;
            if (isLocalSpace)
            {
                tweener = NTweenerCreator.createTransform2JumpTweener<float3>(target, destination, jumpHeight, duration, TransformTweenType.Local_Position);
            }
            else
            {
                tweener = NTweenerCreator.createTransform2JumpTweener<float3>(target, destination, jumpHeight, duration, TransformTweenType.World_Position);
            }
            tweener.schedule();
            return tweener;
        }
        public static NTweener jumpTo(Transform target, Transform destination, float jumpHeight, float duration, bool isLocalSpace = true)
        {
            return jumpTo(target, destination, new float3(0f, jumpHeight, 0f), duration, isLocalSpace);
        }
        #endregion

        #region ValueTween
        public static NTweener fromTo(float from, float destination, float duration, Action<float> onChanged)
        {
            NTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener fromTo(float2 from, float2 destination, float duration, Action<float2> onChanged)
        {
            NTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener fromTo(float3 from, float3 destination, float duration, Action<float3> onChanged)
        {
            NTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener fromTo(float4 from, float4 destination, float duration, Action<float4> onChanged)
        {
            NTweener tweener = NTweenerCreator.createFromToValueTweener(from, destination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }

        public unsafe static NTweener fromToUnsafe(float from, float destination, float duration, object target, delegate*<object, float, void> setPtr)
        {
            NTweener tweener = NTweenerCreator.createFromToValueUnsafeTweener(from, destination, target, setPtr, duration);
            tweener.schedule();
            return tweener;
        }
        public unsafe static NTweener fromToUnsafe(float2 from, float2 destination, float duration, object target, delegate*<object, float2, void> setPtr)
        {
            NTweener tweener = NTweenerCreator.createFromToValueUnsafeTweener(from, destination, target, setPtr, duration);
            tweener.schedule();
            return tweener;
        }
        public unsafe static NTweener fromToUnsafe(float3 from, float3 destination, float duration, object target, delegate*<object, float3, void> setPtr)
        {
            NTweener tweener = NTweenerCreator.createFromToValueUnsafeTweener(from, destination, target, setPtr, duration);
            tweener.schedule();
            return tweener;
        }
        public unsafe static NTweener fromToUnsafe(float4 from, float4 destination, float duration, object target, delegate*<object, float4, void> setPtr)
        {
            NTweener tweener = NTweenerCreator.createFromToValueUnsafeTweener(from, destination, target, setPtr, duration);
            tweener.schedule();
            return tweener;
        }

        public static NTweener punchValue(float origin, float punchDestination, float duration, Action<float> onChanged)
        {
            NTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener punchValue(float2 origin, float2 punchDestination, float duration, Action<float2> onChanged)
        {
            NTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener punchValue(float3 origin, float3 punchDestination, float duration, Action<float3> onChanged)
        {
            NTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener punchValue(float4 origin, float4 punchDestination, float duration, Action<float4> onChanged)
        {
            NTweener tweener = NTweenerCreator.createPunchValueTweener(origin, punchDestination, onChanged, duration);
            tweener.schedule();
            return tweener;
        }

        public static NTweener shakeValue(float origin, float4 range, float duration, Action<float> onChanged)
        {
            NTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener shakeValue(float origin, float range, float duration, Action<float> onChanged)
        {
            return shakeValue(origin, new float4(range), duration, onChanged);
        }
        public static NTweener shakeValue(float2 origin, float4 range, float duration, Action<float2> onChanged)
        {
            NTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener shakeValue(float2 origin, float2 range, float duration, Action<float2> onChanged)
        {
            return shakeValue(origin, new float4(range, 0f, 0f), duration, onChanged);
        }
        public static NTweener shakeValue(float2 origin, float range, float duration, Action<float2> onChanged)
        {
            return shakeValue(origin, new float4(range), duration, onChanged);
        }
        public static NTweener shakeValue(float3 origin, float4 range, float duration, Action<float3> onChanged)
        {
            NTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener shakeValue(float3 origin, float3 range, float duration, Action<float3> onChanged)
        {
            return shakeValue(origin, new float4(range, 0f), duration, onChanged);
        }
        public static NTweener shakeValue(float3 origin, float range, float duration, Action<float3> onChanged)
        {
            return shakeValue(origin, new float4(range), duration, onChanged);
        }
        public static NTweener shakeValue(float4 origin, float4 range, float duration, Action<float4> onChanged)
        {
            NTweener tweener = NTweenerCreator.createShakeValueTweener(origin, range, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener shakeValue(float4 origin, float range, float duration, Action<float4> onChanged)
        {
            return shakeValue(origin, new float4(range), duration, onChanged);
        }

        public static NTweener jumpValue(float origin, float destination, float jumpHeight, float duration, Action<float> onChanged)
        {
            NTweener tweener = NTweenerCreator.createJumpValueTweener(origin, destination, jumpHeight, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener jumpValue(float2 origin, float2 destination, float2 jumpHeight, float duration, Action<float2> onChanged)
        {
            NTweener tweener = NTweenerCreator.createJumpValueTweener(origin, destination, jumpHeight, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener jumpValue(float3 origin, float3 destination, float3 jumpHeight, float duration, Action<float3> onChanged)
        {
            NTweener tweener = NTweenerCreator.createJumpValueTweener(origin, destination, jumpHeight, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        public static NTweener jumpValue(float4 origin, float4 destination, float4 jumpHeight, float duration, Action<float4> onChanged)
        {
            NTweener tweener = NTweenerCreator.createJumpValueTweener(origin, destination, jumpHeight, onChanged, duration);
            tweener.schedule();
            return tweener;
        }
        #endregion
    }
}