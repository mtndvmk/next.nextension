using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension.Tween
{
    public static class NTweenExtension
    {
        public static void cancelAllTweeners(this GameObject target, bool includeChildren, bool includeInActive)
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
            NTween.cancelAllTweeners(new ObjectCancelControlKey(target));
            foreach (var com in components)
            {
                NTween.cancelAllTweeners(new ObjectCancelControlKey(com));
            }
        }
        public static void cancelAllTweeners(this UnityEngine.Object target)
        {
            if (target == null)
            {
                throw new NullReferenceException("cancelAllTweener.target");
            }
            var controlKey = new ObjectCancelControlKey(target);
            NTween.cancelAllTweeners(controlKey);
        }

        public static NRunnableTweener moveTo(this Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener rotateTo(this Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            return rotateTo(target, Quaternion.Euler(destination), duration, isLocalSpace);
        }
        public static NRunnableTweener rotateTo(this Transform target, Quaternion destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.rotateTo(target, destination.toFloat4(), duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener scaleTo(this Transform target, Vector3 destination, float duration)
        {
            var tweener = NTween.scaleTo(target, destination, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener punchScale(this Transform target, Vector3 punchDestination, float duration)
        {
            var tweener = NTween.punchScale(target, punchDestination, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static CombinedNTweener jumpTo(this Transform target, Vector3 destination, float jumpHeight, float duration, bool isLocalSpace = true)
        {
            var moveTweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            var currentHeight = 0f;
            var heightTweener = NTween.punchValue(0, jumpHeight, (f) => currentHeight = f, duration).setEase(EaseType.QuadOut);

            var combinedTweener = new CombinedNTweener(moveTweener, heightTweener);
            combinedTweener.onUpdated(() =>
            {
                target.plusPositionY(currentHeight, isLocalSpace);
            });
            combinedTweener.schedule();
            return combinedTweener;
        }

        public static NRunnableTweener fadeTo(this CanvasGroup target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.alpha, endAlpha, a => target.alpha = a, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener fadeTo(this Graphic target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.color.a, endAlpha, a => target.color = target.color.setA(a), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener fadeTo(this Material target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.color.a, endAlpha, a => target.color = target.color.setA(a), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener fadeTo(this SpriteRenderer target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.color.a, endAlpha, a => target.color = target.color.setA(a), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener colorTo(this Graphic target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toFloat4(), destination.toFloat4(), c => target.color = c.toColor(), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener colorTo(this Material target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toFloat4(), destination.toFloat4(), c => target.color = c.toColor(), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener colorTo(this SpriteRenderer target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toFloat4(), destination.toFloat4(), c => target.color = c.toColor(), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
    }
}