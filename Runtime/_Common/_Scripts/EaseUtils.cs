using Unity.Mathematics;
using UnityEngine;
using System;
using Unity.Burst;

namespace Nextension
{
    public enum EaseType : byte
    {
        Linear,
        SineIn,
        SineOut,
        SineInOut,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        CircIn,
        CircOut,
        CircInOut,
        BackIn,
        BackOut,
        BackInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
    }
    /// <summary>
    /// from https://easings.net/
    /// </summary>
    public static class EaseUtils
    {
        public static float ease(float a, float b, float t, EaseType type)
        {
            switch (type)
            {
                default:
                case EaseType.Linear: return lerp(a, b, t);
                case EaseType.SineIn: return sineIn(a, b, t);
                case EaseType.SineOut: return sineOut(a, b, t);
                case EaseType.SineInOut: return sineInOut(a, b, t);
                case EaseType.QuadIn: return quadIn(a, b, t);
                case EaseType.QuadOut: return quadOut(a, b, t);
                case EaseType.QuadInOut: return quadInOut(a, b, t);
                case EaseType.CubicIn: return cubicIn(a, b, t);
                case EaseType.CubicOut: return cubicOut(a, b, t);
                case EaseType.CubicInOut: return cubicInOut(a, b, t);
                case EaseType.QuartIn: return quartIn(a, b, t);
                case EaseType.QuartOut: return quartOut(a, b, t);
                case EaseType.QuartInOut: return quartInOut(a, b, t);
                case EaseType.QuintIn: return quintIn(a, b, t);
                case EaseType.QuintOut: return quintOut(a, b, t);
                case EaseType.QuintInOut: return quintInOut(a, b, t);
                case EaseType.ExpoIn: return expoIn(a, b, t);
                case EaseType.ExpoOut: return expoOut(a, b, t);
                case EaseType.ExpoInOut: return expoInOut(a, b, t);
                case EaseType.CircIn: return circIn(a, b, t);
                case EaseType.CircOut: return circOut(a, b, t);
                case EaseType.CircInOut: return circInOut(a, b, t);
                case EaseType.BackIn: return backIn(a, b, t);
                case EaseType.BackOut: return backOut(a, b, t);
                case EaseType.BackInOut: return backInOut(a, b, t);
                case EaseType.ElasticIn: return elasticIn(a, b, t);
                case EaseType.ElasticOut: return elasticOut(a, b, t);
                case EaseType.ElasticInOut: return elasticInOut(a, b, t);
                case EaseType.BounceIn: return bounceIn(a, b, t);
                case EaseType.BounceOut: return bounceOut(a, b, t);
                case EaseType.BounceInOut: return bounceInOut(a, b, t);
            }
        }
        public static float2 ease(float2 a, float2 b, float t, EaseType type)
        {
            var x = ease(a.x, b.x, t, type);
            var y = ease(a.y, b.y, t, type);
            return new float2(x, y);
        }
        public static float3 ease(float3 a, float3 b, float t, EaseType type)
        {
            var x = ease(a.x, b.x, t, type);
            var y = ease(a.y, b.y, t, type);
            var z = ease(a.z, b.z, t, type);
            return new float3(x, y, z);
        }
        public static float4 ease(float4 a, float4 b, float t, EaseType type)
        {
            var x = ease(a.x, b.x, t, type);
            var y = ease(a.y, b.y, t, type);
            var z = ease(a.z, b.z, t, type);
            var w = ease(a.w, b.w, t, type);
            return new float4(x, y, z, w);
        }
        public static Vector3 ease(Vector3 a, Vector3 b, float t, EaseType type)
        {
            var x = ease(a.x, b.x, t, type);
            var y = ease(a.y, b.y, t, type);
            var z = ease(a.z, b.z, t, type);
            return new Vector3(x, y, z);
        }

        public static float lerp(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * t;
        }
        public static float sineIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (1 - (float)Math.Cos(t * math.PI / 2));
        }
        public static float sineOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (float)Math.Sin(t * math.PI / 2);
        }
        public static float sineInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * ((float)Math.Cos(t * math.PI) - 1) / -2;
        }
        public static float quadIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * t * t;
        }
        public static float quadOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = 1 - t;
            return a + (b - a) * (1 - t1 * t1);
        }
        public static float quadInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = -2 * t + 2;
            return t < 0.5f ? 
                a + (b - a) * (2 * t * t) : 
                a + (b - a) * (1 - t1 * t1 / 2);
        }
        public static float cubicIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * t * t * t;
        }
        public static float cubicOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = 1 - t;
            return a + (b - a) * (1 - t1 * t1 * t1);
        }
        public static float cubicInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = -2 * t + 2;
            return (t < 0.5f) ? 
                a + (b - a) * (4 * t * t * t) : 
                a + (b - a) * (1 - t1 * t1 * t1 / 2);
        }
        public static float quartIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * t * t * t * t;
        }
        public static float quartOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = 1 - t;
            return a + (b - a) * (1 - t1 * t1 * t1 * t1);
        }
        public static float quartInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = -2 * t + 2;
            return t < 0.5f ? 
                a + (b - a) * (8 * t * t * t * t) : 
                a + (b - a) * (1 - t1 * t1 * t1 * t1 / 2);
        }
        public static float quintIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * t * t * t * t * t;
        }
        public static float quintOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = 1 - t;
            return a + (b - a) * (1 - t1 * t1 * t1 * t1 * t1);
        }
        public static float quintInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = -2 * t + 2;
            return t < 0.5f ? 
                a + (b - a) * 16 * t * t * t * t * t : 
                a + (b - a) * (1 - t1 * t1 * t1 * t1 * t1 / 2);
        }
        public static float expoIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (float)Math.Pow(2, 10 * t - 10);
        }
        public static float expoOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (1 - (float)Math.Pow(2, -10 * t));
        }
        public static float expoInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return t < 0.5f ? 
                a + (b - a) * (float)Math.Pow(2, 20 * t - 10) / 2 : 
                a + (b - a) * (2 - (float)Math.Pow(2, -20 * t + 10)) / 2;
        }
        public static float circIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (1 - (float)Math.Sqrt(1 - t * t));
        }
        public static float circOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = t - 1;
            return a + (b - a) * (float)Math.Sqrt(1 - t1 * t1);
        }
        public static float circInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = 2 * t;
            var t2 = -2 * t + 2;
            return t < 0.5f ?
                   a + (b - a) * (1 - (float)Math.Sqrt(1 - t1 * t1)) / 2 :
                   a + (b - a) * ((float)Math.Sqrt(1 - t2 * t2) + 1) / 2;
        }
        public static float backIn(float a, float b, float t)
        {
            //var c1 = 1.70158f;
            //var c3 = c1 + 1;// = 2.70158f
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (2.70158f * t * t * t - 1.70158f * t * t);
        }
        public static float backOut(float a, float b, float t)
        {
            //var c1 = 1.70158f;
            //var c3 = c1 + 1;// = 2.70158f
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            var t1 = t - 1;
            var t2 = t1 * t1;
            return a + (b - a) * (1 + 2.70158f * t2 * t1 + 1.70158f * t2);
        }
        public static float backInOut(float a, float b, float t)
        {
            //var c1 = 1.70158f;
            //var c2 = c1 * 1.525f;// = 2.5949095f
            //var c3 = c2 + 1;// = 3.5949095f
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }

            var t1 = t * 2;
            var t2 = t1 - 2;
            return t < 0.5f ?
                   a + (b - a) * t1 * t1 * (3.5949095f * t1 - 2.5949095f) / 2 :
                   a + (b - a) * (t2 * t2 * (3.5949095f * t2 + 2.5949095f) + 2) / 2;
        }
        public static float elasticIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (-(float)Math.Pow(2, 10 * t - 10) * (float)Math.Sin((t * 10 - 10.75f) * (2 * math.PI) / 3));
        }
        public static float elasticOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * ((float)Math.Pow(2, -10 * t) * (float)Math.Sin((t * 10 - 0.75f) * (math.PI * 2) / 3) + 1);
        }
        public static float elasticInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return t < 0.5f ?
                   a + (b - a) * ((float)Math.Pow(2, 20 * t - 10) * (float)Math.Sin((20 * t - 11.125f) * (math.PI * 2 / 4.5f)) / -2) :
                   a + (b - a) * ((float)Math.Pow(2, -20 * t + 10) * (float)Math.Sin((20 * t - 11.125f) * (math.PI * 2 / 4.5f)) / 2 + 1);
        }
        public static float bounceOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            float n1 = 7.5625f;
            float d1 = 2.75f;

            if (t < 1 / d1)
            {
                return a + (b - a) * (n1 * t * t);
            }
            else if (t < 2 / d1)
            {
                return a + (b - a) * (n1 * (t -= 1.5f / d1) * t + 0.75f);
            }
            else if (t < 2.5f / d1)
            {
                return a + (b - a) * (n1 * (t -= 2.25f / d1) * t + 0.9375f);
            }
            else
            {
                return a + (b - a) * (n1 * (t -= 2.625f / d1) * t + 0.984375f);
            }
        }
        public static float bounceIn(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return a + (b - a) * (1 - bounceOut(0, 1, 1 - t));
        }
        public static float bounceInOut(float a, float b, float t)
        {
            if (t >= 1)
            {
                return b;
            }
            if (t <= 0)
            {
                return a;
            }
            return t < 0.5f ?
                   a + (b - a) * ((1 - bounceOut(0, 1, 1 - 2 * t)) / 2) :
                   a + (b - a) * ((1 + bounceOut(0, 1, 2 * t - 1)) / 2);
        }
    }
}
