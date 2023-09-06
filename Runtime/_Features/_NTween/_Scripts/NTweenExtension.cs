using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension.Tween
{
    public static class NTweenExtension
    {
        public static void cancelAllTweener(this GameObject target, bool includeChildren, bool includeInActive)
        {
            if (target == null)
            {
                throw new NullReferenceException("cancelAllTweener.target");
            }
            Component[] components;
            if (includeChildren)
            {
                components = target.GetComponents<Component>();
            }
            else
            {
                components = target.GetComponentsInChildren<Component>(includeInActive);
            }
            cancelAllTweener(new ObjectCancelControlKey(target));
            foreach (var com in components)
            {
                cancelAllTweener(new ObjectCancelControlKey(com));
            }
        }
        public static void cancelAllTweener(this UnityEngine.Object target)
        {
            if (target == null)
            {
                throw new NullReferenceException("cancelAllTweener.target");
            }
            var controlKey = new ObjectCancelControlKey(target);
            NTweenManager.cancelFromControlledTweener(controlKey);
        }
        public static void cancelAllTweener(AbsCancelControlKey controlKey)
        {
            NTweenManager.cancelFromControlledTweener(controlKey);
        }
        public static NTweener moveTo(this Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTweenManager.moveTo(target, destination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener rotateTo(this Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTweenManager.rotateTo(target, destination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener rotateTo(this Transform target, Quaternion destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTweenManager.rotateTo(target, destination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener scaleTo(this Transform target, Vector3 destination, float duration)
        {
            var tweener = NTweenManager.scaleTo(target, destination, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener punchScale(this Transform target, Vector3 punchDestination, float duration)
        {
            var tweener = NTweenManager.punchScale(target, punchDestination, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener fadeTo(this CanvasGroup target, float endAlpha, float duration)
        {
            var tweener = NTweenManager.fromTo(target.alpha, endAlpha, a => target.alpha = a, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener fadeTo(this Graphic target, float endAlpha, float duration)
        {
            var tweener = NTweenManager.fromTo(target.color.a, endAlpha, a => target.color = target.color.setA(a), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener fadeTo(this Material target, float endAlpha, float duration)
        {
            var tweener = NTweenManager.fromTo(target.color.a, endAlpha, a => target.color = target.color.setA(a), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener fadeTo(this SpriteRenderer target, float endAlpha, float duration)
        {
            var tweener = NTweenManager.fromTo(target.color.a, endAlpha, a => target.color = target.color.setA(a), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener colorTo(this Graphic target, Color destination, float duration)
        {
            var tweener = NTweenManager.fromTo(target.color.toFloat4(), destination.toFloat4(), c => target.color = c.toColor(), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener colorTo(this Material target, Color destination, float duration)
        {
            var tweener = NTweenManager.fromTo(target.color.toFloat4(), destination.toFloat4(), c => target.color = c.toColor(), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NTweener colorTo(this SpriteRenderer target, Color destination, float duration)
        {
            var tweener = NTweenManager.fromTo(target.color.toFloat4(), destination.toFloat4(), c => target.color = c.toColor(), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
    }
}