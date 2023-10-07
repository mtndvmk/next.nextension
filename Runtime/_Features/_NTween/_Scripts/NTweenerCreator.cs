using System;
using UnityEngine;

namespace Nextension.Tween
{
    internal static class NTweenerCreator
    {
        public static NRunnableTweener createTransformFromToTweener<T>(Transform target, T destination, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new TransformFromToTween<T>.Tweener(target, destination, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NRunnableTweener createTransformPunchTweener<T>(Transform target, T punchValue, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new TransformPunchTween<T>.Tweener(target, punchValue, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NRunnableTweener createTransformShakeTweener<T>(Transform target, float range, float duration, TransformTweenType transformTweenType) where T : unmanaged
        {
            var tweener = new TransformShakeTween<T>.Tweener(target, range, transformTweenType)
            {
                duration = duration,
            };
            return tweener;
        }

        public static NRunnableTweener createFromToValueTweener<T>(T from, T destination, Action<T> onChanged, float duration) where T : unmanaged
        {
            NRunnableTweener tweener;
            tweener = new FromToValueTween<T>.Tweener(from, destination, onChanged)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NRunnableTweener createPunchValueTweener<T>(T origin, T punchValue, Action<T> onChanged, float duration) where T : unmanaged
        {
            NRunnableTweener tweener;
            tweener = new PunchValueTween<T>.Tweener(origin, punchValue, onChanged)
            {
                duration = duration,
            };
            return tweener;
        }
        public static NRunnableTweener createShakeValueTweener<T>(T origin, float range, Action<T> onChanged, float duration) where T : unmanaged
        {
            NRunnableTweener tweener;
            tweener = new ShakeValueTween<T>.Tweener(origin, range, onChanged)
            {
                duration = duration,
            };
            return tweener;
        }
    }
}