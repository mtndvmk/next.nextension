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
            PList<Component> components;
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

        public static NTweener moveTo(this Transform target, Vector3 destination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener moveTo(this Transform target, Transform destination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.moveTo(target, destination, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener rotateTo(this Transform target, Vector3 destinationDegrees, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            return rotateTo(target, Quaternion.Euler(destinationDegrees), duration, isLocalSpace, isSetCtrlKey);
        }
        public static NTweener rotateTo(this Transform target, Quaternion destination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.rotateTo(target, destination.toFloat4(), duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener rotateTo(this Transform target, Transform destination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.rotateTo(target, destination, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener scaleTo(this Transform target, Vector3 destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.scaleTo(target, destination, duration);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener scaleTo(this Transform target, float destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.scaleTo(target, destination, duration);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener scaleTo(this Transform target, Transform destination, float duration, bool uniformScale = false, bool isSetCtrlKey = true)
        {
            var tweener = NTween.scaleTo(target, destination, duration, uniformScale);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener scaleFromTo(this Transform target, Vector3 from, Vector3 destination, float duration, bool isSetCtrlKey = true)
        {
            target.localScale = from;
            return scaleTo(target, destination, duration, isSetCtrlKey);
        }
        public static NTweener scaleFromTo(this Transform target, float from, float destination, float duration, bool isSetCtrlKey = true)
        {
            target.setScale(from);
            return scaleTo(target, destination, duration, isSetCtrlKey);
        }

        public static NTweener punchPosition(this Transform target, Vector3 punchDestination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.punchPosition(target, punchDestination, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener punchPosition(this Transform target, Transform punchDestination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.punchPosition(target, punchDestination, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener punchRotation(this Transform target, Quaternion punchDestination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.punchRotation(target, punchDestination.toFloat4(), duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener punchRotation(this Transform target, Vector3 punchDestinationInDegrees, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = punchRotation(target, Quaternion.Euler(punchDestinationInDegrees), duration, isLocalSpace, isSetCtrlKey);
            return tweener;
        }
        public static NTweener punchRotation(this Transform target, Transform punchDestination, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.punchRotation(target, punchDestination, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener punchScale(this Transform target, Vector3 punchDestination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.punchScale(target, punchDestination, duration);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener punchScale(this Transform target, float punchDestination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.punchScale(target, punchDestination, duration);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener punchScale(this Transform target, Transform punchDestination, float duration, bool uniformScale = false, bool isSetCtrlKey = true)
        {
            var tweener = NTween.punchScale(target, punchDestination, duration, uniformScale);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakePosition(this Transform target, float4 magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakePosition(target, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakePosition(this Transform target, Vector3 magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakePosition(target, (float3)magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakePosition(this Transform target, float magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakePosition(target, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakePosition(this Transform target, Transform destination, float4 magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakePosition(target, destination, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakePosition(this Transform target, Transform destination, Vector3 magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakePosition(target, destination, (float3)magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakePosition(this Transform target, Transform destination, float magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakePosition(target, destination, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeRotation(this Transform target, float4 magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeRotation(target, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeRotation(this Transform target, Vector3 magnitudeInDegrees, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeRotation(target, (float3)magnitudeInDegrees, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeRotation(this Transform target, float magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeRotation(target, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeRotation(this Transform target, Transform destination, float4 magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeRotation(target, destination, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeRotation(this Transform target, Transform destination, Vector3 magnitudeInDegrees, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeRotation(target, destination, (float3)magnitudeInDegrees, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeRotation(this Transform target, Transform destination, float magnitude, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeRotation(target, destination, magnitude, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeScale(this Transform target, float4 magnitude, float duration, bool uniformScale = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeScale(target, magnitude, duration, uniformScale);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeScale(this Transform target, Vector3 magnitude, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeScale(target, (float3)magnitude, duration);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeScale(this Transform target, float magnitude, float duration, bool uniformScale = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeScale(target, magnitude, duration, uniformScale);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeScale(this Transform target, Transform destination, float4 magnitude, float duration, bool uniformScale = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeScale(target, destination, magnitude, duration, uniformScale);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeScale(this Transform target, Transform destination, Vector3 magnitude, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeScale(target, destination, (float3)magnitude, duration);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener shakeScale(this Transform target, Transform destination, float magnitude, float duration, bool uniformScale = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.shakeScale(target, destination, magnitude, duration, uniformScale);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener jumpTo(this Transform target, Vector3 destination, float jumpHeight, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.jumpTo(target, (float3)destination, jumpHeight, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener jumpTo(this Transform target, Vector3 destination, Vector3 jumpHeight, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.jumpTo(target, (float3)destination, (float3)jumpHeight, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener jumpTo(this Transform target, Transform destination, float jumpHeight, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.jumpTo(target, destination, jumpHeight, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public static NTweener jumpTo(this Transform target, Transform destination, Vector3 jumpHeight, float duration, bool isLocalSpace = true, bool isSetCtrlKey = true)
        {
            var tweener = NTween.jumpTo(target, destination, (float3)jumpHeight, duration, isLocalSpace);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }

        public unsafe static NTweener fadeTo(this CanvasGroup target, float endAlpha, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.alpha, endAlpha, duration, target, &ColorSetter.setAlpha_CanvasGroup);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener fadeTo(this Graphic target, float endAlpha, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.a, endAlpha, duration, target, &ColorSetter.setAlpha_Graphic);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener fadeTo(this Material target, float endAlpha, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.a, endAlpha, duration, target, &ColorSetter.setAlpha_Material);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener fadeTo(this SpriteRenderer target, float endAlpha, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.a, endAlpha, duration, target, &ColorSetter.setAlpha_SpriteRenderer);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener fadeTo(this CanvasRenderer target, float endAlpha, float duration, bool isSetCtrlKey = true)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromToUnsafe(fromColor.a, endAlpha, duration, target, &ColorSetter.setAlpha_CanvasRenderer);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }

        public unsafe static NTweener colorTo(this Graphic target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.toFloat4(), destination.toFloat4(), duration, target, &ColorSetter.setColor_Graphic);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener colorTo(this Material target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.toFloat4(), destination.toFloat4(), duration, target, &ColorSetter.setColor_Material);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener colorTo(this SpriteRenderer target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.toFloat4(), destination.toFloat4(), duration, target, &ColorSetter.setColor_SpriteRenderer);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener colorTo(this CanvasRenderer target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromToUnsafe(fromColor.toFloat4(), destination.toFloat4(), duration, target, &ColorSetter.setColor_CanvasRenderer);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }

        public unsafe static NTweener hsvTo(this Graphic target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.toHsvFloat4(), destination.toHsvFloat4(), duration, target, &ColorSetter.setHsvColor_Graphic);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener hsvTo(this Material target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.toHsvFloat4(), destination.toHsvFloat4(), duration, target, &ColorSetter.setHsvColor_Material);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener hsvTo(this SpriteRenderer target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var tweener = NTween.fromToUnsafe(target.color.toHsvFloat4(), destination.toHsvFloat4(), duration, target, &ColorSetter.setHsvColor_SpriteRenderer);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
        public unsafe static NTweener hsvTo(this CanvasRenderer target, Color destination, float duration, bool isSetCtrlKey = true)
        {
            var fromColor = target.GetColor();
            var tweener = NTween.fromToUnsafe(fromColor.toHsvFloat4(), destination.toHsvFloat4(), duration, target, &ColorSetter.setHsvColor_CanvasRenderer);
            if (isSetCtrlKey)
            {
                tweener.setCancelControlKey(target);
            }
            return tweener;
        }
    }
}
