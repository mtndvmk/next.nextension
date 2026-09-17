using System;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    internal static class NTweenerCreator
    {
        public static NTweener createTransformFromToTweener<T>(Transform target, T destination, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new TransformFromToTween<T>.Tweener(target, destination, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createTransformPunchTweener<T>(Transform target, T punchDestination, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new TransformPunchTween<T>.Tweener(target, punchDestination, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createTransformShakeTweener<T>(Transform target, float4 range, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new TransformShakeTween<T>.Tweener(target, range, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createTransformJumpTweener<T>(Transform target, T destination, T jumpHeight, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new TransformJumpTween<T>.Tweener(target, destination, jumpHeight, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }

        public static NTweener createTransform2FromToTweener<T>(Transform target, Transform destination, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new Transform2FromToTween<T>.Tweener(target, destination, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createTransform2PunchTweener<T>(Transform target, Transform destination, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new Transform2PunchTween<T>.Tweener(target, destination, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createTransform2ShakeTweener<T>(Transform target, Transform destination, float4 range, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new Transform2ShakeTween<T>.Tweener(target, destination, range, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createTransform2JumpTweener<T>(Transform target, Transform destination, T jumpHeight, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new Transform2JumpTween<T>.Tweener(target, destination, jumpHeight, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }

        public static NTweener createFromToValueTweener<T>(T from, T destination, Action<T> onChanged, float duration) where T : unmanaged
        {
            NTweener tweener;
            tweener = new FromToValueTween<T>.Tweener(from, destination, onChanged)
            {
                duration = duration,
            };
            return tweener;
        }
        public unsafe static NTweener createFromToValueUnsafeTweener<T>(T from, T destination, object target, delegate*<object, T, void> setPtr, float duration) where T : unmanaged
        {
            NTweener tweener;
            tweener = new FromToValueTween<T>.UnsafeTweener(from, destination, target, setPtr)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createPunchValueTweener<T>(T origin, T punchDestination, Action<T> onChanged, float duration) where T : unmanaged
        {
            NTweener tweener;
            tweener = new PunchValueTween<T>.Tweener(origin, punchDestination, onChanged)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createShakeValueTweener<T>(T origin, float4 range, Action<T> onChanged, float duration) where T : unmanaged
        {
            NTweener tweener;
            tweener = new ShakeValueTween<T>.Tweener(origin, range, onChanged)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NTweener createJumpValueTweener<T>(T origin, T destination, T jumpHeight, Action<T> onChanged, float duration) where T : unmanaged
        {
            NTweener tweener;
            tweener = new JumpValueTween<T>.Tweener(origin, destination, jumpHeight, onChanged)
            {
                duration = duration,
            };
            return tweener;
        }
    }
}