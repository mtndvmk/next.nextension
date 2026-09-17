using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    public static class ColorSetter
    {
        public static void setColor_Graphic(object target, float4 color)
        {
            ((Graphic)target).color = color.toColor();
        }
        public static void setColor_Material(object target, float4 color)
        {
            ((Material)target).color = color.toColor();
        }
        public static void setColor_SpriteRenderer(object target, float4 color)
        {
            ((SpriteRenderer)target).color = color.toColor();
        }
        public static void setColor_CanvasRenderer(object target, float4 color)
        {
            ((CanvasRenderer)target).SetColor(color.toColor());
        }

        public static void setAlpha_CanvasGroup(object target, float a)
        {
            ((CanvasGroup)target).alpha = a;
        }
        public static void setAlpha_Graphic(object target, float a)
        {
            var t = (Graphic)target;
            t.color = t.color.setA(a);
        }
        public static void setAlpha_Material(object target, float a)
        {
            var t = (Material)target;
            t.color = t.color.setA(a);
        }
        public static void setAlpha_SpriteRenderer(object target, float a)
        {
            var t = (SpriteRenderer)target;
            t.color = t.color.setA(a);
        }
        public static void setAlpha_CanvasRenderer(object target, float a)
        {
            var t = (CanvasRenderer)target;
            t.SetColor(t.GetColor().setA(a));
        }

        public static void setHsvColor_Graphic(object target, float4 color)
        {
            ((Graphic)target).color = color.toHsvColor();
        }
        public static void setHsvColor_Material(object target, float4 color)
        {
            ((Material)target).color = color.toHsvColor();
        }
        public static void setHsvColor_SpriteRenderer(object target, float4 color)
        {
            ((SpriteRenderer)target).color = color.toHsvColor();
        }
        public static void setHsvColor_CanvasRenderer(object target, float4 color)
        {
            ((CanvasRenderer)target).SetColor(color.toHsvColor());
        }
    }
}