using UnityEngine;
using UnityEngine.UI;

namespace Nextension.Tween
{
    public static class NTweenExtension
    {
        public static void cancelAllTweeners(this Object target)
        {
            NTween.cancelAllTweeners(target);
        }
        public static void cancelAllTweenersInGameObject(this GameObject target, bool includeChildren, bool includeInActive = false)
        {
            Component[] components;
            if (includeChildren)
            {
                components = target.GetComponentsInChildren<Component>(includeInActive);
            }
            else
            {
                components = target.GetComponents<Component>();
            }
            foreach (var com in components)
            {
                NTween.cancelAllTweeners(com);
            }
        }

        public static NRunnableTweener moveTo(this Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener rotateTo(this Transform target, Vector3 destinationDegrees, float duration, bool isLocalSpace = true)
        {
            return rotateTo(target, Quaternion.Euler(destinationDegrees), duration, isLocalSpace);
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
        public static NRunnableTweener scaleTo(this Transform target, float destination, float duration)
        {
            var tweener = NTween.scaleTo(target, destination, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }

        public static NRunnableTweener punchPosition(this Transform target, Vector3 punchDestination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.punchPosition(target, punchDestination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener punchRotation(this Transform target, Quaternion punchDestination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.punchRotation(target, punchDestination.toFloat4(), duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener punchScale(this Transform target, Vector3 punchDestination, float duration)
        {
            var tweener = NTween.punchScale(target, punchDestination, duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener punchScale(this Transform target, float punchDestination, float duration)
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
        public static NRunnableTweener fadeTo(this CanvasRenderer target, float endAlpha, float duration)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromTo(fromColor.a, endAlpha, a => target.SetColor(fromColor.setA(a)), duration);
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
        public static NRunnableTweener colorTo(this CanvasRenderer target, Color destination, float duration)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromTo(fromColor.toFloat4(), destination.toFloat4(), c => target.SetColor(c.toColor()), duration);
            tweener.setCancelControlKey(target);
            return tweener;
        }
    }
}