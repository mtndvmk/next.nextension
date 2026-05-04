using Unity.Mathematics;
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
            NPList<Component> components;
            if (includeChildren)
            {
                components = target.getComponentsInChildren_CachedList<Component>(includeInActive);
            }
            else
            {
                components = target.getComponents_CachedList<Component>();
            }
            foreach (var com in components)
            {
                NTween.cancelAllTweeners(com);
            }
            components.Dispose();
        }

        public static NRunnableTweener moveTo(this Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener moveTo(this Transform target, Transform destination, float duration, bool isLocalSpace = true)
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
        public static NRunnableTweener rotateTo(this Transform target, Transform destination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.rotateTo(target, destination, duration, isLocalSpace);
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
        public static NRunnableTweener scaleTo(this Transform target, Transform destination, float duration, bool uniformScale = false)
        {
            var tweener = NTween.scaleTo(target, destination, duration, uniformScale);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener scaleFromTo(this Transform target, Vector3 from, Vector3 destination, float duration)
        {
            target.localScale = from;
            return scaleTo(target, destination, duration);
        }
        public static NRunnableTweener scaleFromTo(this Transform target, float from, float destination, float duration)
        {
            target.setScale(from);
            return scaleTo(target, destination, duration);
        }

        public static NRunnableTweener punchPosition(this Transform target, Vector3 punchDestination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.punchPosition(target, punchDestination, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener punchPosition(this Transform target, Transform punchDestination, float duration, bool isLocalSpace = true)
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
        public static NRunnableTweener punchRotation(this Transform target, Vector3 punchDestinationInDegrees, float duration, bool isLocalSpace = true)
        {
            var tweener = punchRotation(target, Quaternion.Euler(punchDestinationInDegrees), duration, isLocalSpace);
            return tweener;
        }
        public static NRunnableTweener punchRotation(this Transform target, Transform punchDestination, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.punchRotation(target, punchDestination, duration, isLocalSpace);
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
        public static NRunnableTweener punchScale(this Transform target, Transform punchDestination, float duration, bool uniformScale = false)
        {
            var tweener = NTween.punchScale(target, punchDestination, duration, uniformScale);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener shakePosition(this Transform target, float magnitude, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.shakePosition(target, magnitude, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener shakePosition(this Transform target, Transform destination, float magnitude, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.shakePosition(target, destination, magnitude, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener shakeRotation(this Transform target, float magnitude, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.shakeRotation(target, magnitude, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener shakeRotation(this Transform target, Transform destination, float magnitude, float duration, bool isLocalSpace = true)
        {
            var tweener = NTween.shakeRotation(target, destination, magnitude, duration, isLocalSpace);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener shakeScale(this Transform target, float magnitude, float duration, bool uniformScale = true)
        {
            var tweener = NTween.shakeScale(target, magnitude, duration, uniformScale);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener shakeScale(this Transform target, Transform destination, float magnitude, float duration, bool uniformScale = true)
        {
            var tweener = NTween.shakeScale(target, destination, magnitude, duration, uniformScale);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        private class JumpDataFloat
        {
            public Transform target;
            public bool isLocalSpace;

            public void setHeight(float height)
            {
                target.plusPositionY(height, isLocalSpace);
            }
        }
        private class JumpDataFloat3
        {
            public Transform target;
            public bool isLocalSpace;

            public void setHeight(float3 height)
            {
                target.plusPositionXYZ(height.x, height.y, height.z, isLocalSpace);
            }
        }
        public static CombinedNTweener jumpTo(this Transform target, Vector3 destination, float jumpHeight, float duration, bool isLocalSpace = true)
        {
            var moveTweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            var jumpData = new JumpDataFloat
            {
                target = target,
                isLocalSpace = isLocalSpace
            };

            var heightTweener = NTween.punchValue(0, jumpHeight, duration, jumpData.setHeight).setEase(EaseType.QuadOut);
            var combinedTweener = new CombinedNTweener(moveTweener, heightTweener);
            combinedTweener.setCancelControlKey(target);
            combinedTweener.schedule();
            return combinedTweener;
        }
        public static CombinedNTweener jumpTo(this Transform target, Vector3 destination, Vector3 jumpHeight, float duration, bool isLocalSpace = true)
        {
            var moveTweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            var jumpData = new JumpDataFloat3
            {
                target = target,
                isLocalSpace = isLocalSpace
            };
            var heightTweener = NTween.punchValue(float3.zero, jumpHeight, duration, jumpData.setHeight).setEase(EaseType.QuadOut);
            var combinedTweener = new CombinedNTweener(moveTweener, heightTweener);
            combinedTweener.setCancelControlKey(target);
            combinedTweener.schedule();
            return combinedTweener;
        }

        public static NRunnableTweener fadeTo(this CanvasGroup target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.alpha, endAlpha, duration, a => target.alpha = a);
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener fadeTo(this Graphic target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.color.a, endAlpha, duration, a => target.color = target.color.setA(a));
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener fadeTo(this Material target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.color.a, endAlpha, duration, a => target.color = target.color.setA(a));
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener fadeTo(this SpriteRenderer target, float endAlpha, float duration)
        {
            var tweener = NTween.fromTo(target.color.a, endAlpha, duration, a => target.color = target.color.setA(a));
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener fadeTo(this CanvasRenderer target, float endAlpha, float duration)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromTo(fromColor.a, endAlpha, duration, a => target.SetColor(fromColor.setA(a)));
            tweener.setCancelControlKey(target);
            return tweener;
        }

        public static NRunnableTweener colorTo(this Graphic target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toFloat4(), destination.toFloat4(), duration, c => target.color = c.toColor());
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener colorTo(this Material target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toFloat4(), destination.toFloat4(), duration, c => target.color = c.toColor());
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener colorTo(this SpriteRenderer target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toFloat4(), destination.toFloat4(), duration, c => target.color = c.toColor());
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener colorTo(this CanvasRenderer target, Color destination, float duration)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromTo(fromColor.toFloat4(), destination.toFloat4(), duration, c => target.SetColor(c.toColor()));
            tweener.setCancelControlKey(target);
            return tweener;
        }

        public static NRunnableTweener hsvTo(this Graphic target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toHsvFloat4(), destination.toHsvFloat4(), duration, c => target.color = c.toHsvColor());
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener hsvTo(this Material target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toHsvFloat4(), destination.toHsvFloat4(), duration, c => target.color = c.toHsvColor());
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener hsvTo(this SpriteRenderer target, Color destination, float duration)
        {
            var tweener = NTween.fromTo(target.color.toHsvFloat4(), destination.toHsvFloat4(), duration, c => target.color = c.toHsvColor());
            tweener.setCancelControlKey(target);
            return tweener;
        }
        public static NRunnableTweener hsvTo(this CanvasRenderer target, Color destination, float duration)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromTo(fromColor.toHsvFloat4(), destination.toHsvFloat4(), duration, c => target.SetColor(c.toHsvColor()));
            tweener.setCancelControlKey(target);
            return tweener;
        }
    }
}