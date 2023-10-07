using System;
using Unity.Burst;
using Unity.Mathematics;

namespace Nextension
{
    [BurstCompile]
    public static class BurstEaseFloat4
    {
        [BurstCompile]
        public static void ease(in float4 a, in float4 b, in float t, in EaseType type, out float4 result)
        {
            switch (type)
            {
                default:
                case EaseType.Linear: lerp(a, b, t, out result); break;
                case EaseType.SineIn: sineIn(a, b, t, out result); break;
                case EaseType.SineOut: sineOut(a, b, t, out result); break;
                case EaseType.SineInOut: sineInOut(a, b, t, out result); break;
                case EaseType.QuadIn: quadIn(a, b, t, out result); break;
                case EaseType.QuadOut: quadOut(a, b, t, out result); break;
                case EaseType.QuadInOut: quadInOut(a, b, t, out result); break;
                case EaseType.CubicIn: cubicIn(a, b, t, out result); break;
                case EaseType.CubicOut: cubicOut(a, b, t, out result); break;
                case EaseType.CubicInOut: cubicInOut(a, b, t, out result); break;
                case EaseType.QuartIn: quartIn(a, b, t, out result); break;
                case EaseType.QuartOut: quartOut(a, b, t, out result); break;
                case EaseType.QuartInOut: quartInOut(a, b, t, out result); break;
                case EaseType.QuintIn: quintIn(a, b, t, out result); break;
                case EaseType.QuintOut: quintOut(a, b, t, out result); break;
                case EaseType.QuintInOut: quintInOut(a, b, t, out result); break;
                case EaseType.ExpoIn: expoIn(a, b, t, out result); break;
                case EaseType.ExpoOut: expoOut(a, b, t, out result); break;
                case EaseType.ExpoInOut: expoInOut(a, b, t, out result); break;
                case EaseType.CircIn: circIn(a, b, t, out result); break;
                case EaseType.CircOut: circOut(a, b, t, out result); break;
                case EaseType.CircInOut: circInOut(a, b, t, out result); break;
                case EaseType.BackIn: backIn(a, b, t, out result); break;
                case EaseType.BackOut: backOut(a, b, t, out result); break;
                case EaseType.BackInOut: backInOut(a, b, t, out result); break;
                case EaseType.ElasticIn: elasticIn(a, b, t, out result); break;
                case EaseType.ElasticOut: elasticOut(a, b, t, out result); break;
                case EaseType.ElasticInOut: elasticInOut(a, b, t, out result); break;
                case EaseType.BounceIn: bounceIn(a, b, t, out result); break;
                case EaseType.BounceOut: bounceOut(a, b, t, out result); break;
                case EaseType.BounceInOut: bounceInOut(a, b, t, out result); break;
            }
        }
        [BurstCompile]
        public static void lerp(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * t;
            }
        }
        [BurstCompile]
        public static void sineIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * (1 - (float)Math.Cos(t * math.PI / 2));
            }
        }
        [BurstCompile]
        public static void sineOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * (float)Math.Sin(t * math.PI / 2);
            }
        }
        [BurstCompile]
        public static void sineInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * ((float)Math.Cos(t * math.PI) - 1) / -2;
            }
        }
        [BurstCompile]
        public static void quadIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * t * t;
            }
        }
        [BurstCompile]
        public static void quadOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = 1 - t;
                result = a + (b - a) * (1 - t1 * t1);
            }
        }
        [BurstCompile]
        public static void quadInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = -2 * t + 2;
                result = t < 0.5f ?
                    a + (b - a) * (2 * t * t) :
                    a + (b - a) * (1 - t1 * t1 / 2);
            }
        }
        [BurstCompile]
        public static void cubicIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * t * t * t;
            }
        }
        [BurstCompile]
        public static void cubicOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = 1 - t;
                result = a + (b - a) * (1 - t1 * t1 * t1);
            }
        }
        [BurstCompile]
        public static void cubicInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = -2 * t + 2;
                result = (t < 0.5f) ?
                    a + (b - a) * (4 * t * t * t) :
                    a + (b - a) * (1 - t1 * t1 * t1 / 2);
            }
        }
        [BurstCompile]
        public static void quartIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * t * t * t * t;
            }
        }
        [BurstCompile]
        public static void quartOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = 1 - t;
                result = a + (b - a) * (1 - t1 * t1 * t1 * t1);
            }
        }
        [BurstCompile]
        public static void quartInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = -2 * t + 2;
                result = t < 0.5f ?
                    a + (b - a) * (8 * t * t * t * t) :
                    a + (b - a) * (1 - t1 * t1 * t1 * t1 / 2);
            }

        }
        [BurstCompile]
        public static void quintIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * t * t * t * t * t;

            }
        }
        [BurstCompile]
        public static void quintOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = 1 - t;
                result = a + (b - a) * (1 - t1 * t1 * t1 * t1 * t1);
            }
        }
        [BurstCompile]
        public static void quintInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = -2 * t + 2;
                result = t < 0.5f ?
                    a + (b - a) * 16 * t * t * t * t * t :
                    a + (b - a) * (1 - t1 * t1 * t1 * t1 * t1 / 2);
            }
        }
        [BurstCompile]
        public static void expoIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * (float)Math.Pow(2, 10 * t - 10);
            }
        }
        [BurstCompile]
        public static void expoOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * (1 - (float)Math.Pow(2, -10 * t));
            }
        }
        [BurstCompile]
        public static void expoInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = t < 0.5f ?
                    a + (b - a) * (float)Math.Pow(2, 20 * t - 10) / 2 :
                    a + (b - a) * (2 - (float)Math.Pow(2, -20 * t + 10)) / 2;
            }
        }
        [BurstCompile]
        public static void circIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * (1 - (float)Math.Sqrt(1 - t * t));
            }
        }
        [BurstCompile]
        public static void circOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = t - 1;
                result = a + (b - a) * (float)Math.Sqrt(1 - t1 * t1);
            }
        }
        [BurstCompile]
        public static void circInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = 2 * t;
                var t2 = -2 * t + 2;
                result = t < 0.5f ?
                       a + (b - a) * (1 - (float)Math.Sqrt(1 - t1 * t1)) / 2 :
                       a + (b - a) * ((float)Math.Sqrt(1 - t2 * t2) + 1) / 2;
            }
        }
        [BurstCompile]
        public static void backIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            //var c1 = 1.70158f;
            //var c3 = c1 + 1;// = 2.70158f
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * (2.70158f * t * t * t - 1.70158f * t * t);
            }
        }
        [BurstCompile]
        public static void backOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            //var c1 = 1.70158f;
            //var c3 = c1 + 1;// = 2.70158f
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = t - 1;
                var t2 = t1 * t1;
                result = a + (b - a) * (1 + 2.70158f * t2 * t1 + 1.70158f * t2);
            }

        }
        [BurstCompile]
        public static void backInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            //var c1 = 1.70158f;
            //var c2 = c1 * 1.525f;// = 2.5949095f
            //var c3 = c2 + 1;// = 3.5949095f
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                var t1 = t * 2;
                var t2 = t1 - 2;
                result = t < 0.5f ?
                    a + (b - a) * t1 * t1 * (3.5949095f * t1 - 2.5949095f) / 2 :
                    a + (b - a) * (t2 * t2 * (3.5949095f * t2 + 2.5949095f) + 2) / 2;
            }
        }
        [BurstCompile]
        public static void elasticIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * (-(float)Math.Pow(2, 10 * t - 10) * (float)Math.Sin((t * 10 - 10.75f) * (2 * math.PI) / 3));
            }
        }
        [BurstCompile]
        public static void elasticOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = a + (b - a) * ((float)Math.Pow(2, -10 * t) * (float)Math.Sin((t * 10 - 0.75f) * (math.PI * 2) / 3) + 1);
            }
        }
        [BurstCompile]
        public static void elasticInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                result = t < 0.5f ?
                   a + (b - a) * ((float)Math.Pow(2, 20 * t - 10) * (float)Math.Sin((20 * t - 11.125f) * (math.PI * 2 / 4.5f)) / -2) :
                   a + (b - a) * ((float)Math.Pow(2, -20 * t + 10) * (float)Math.Sin((20 * t - 11.125f) * (math.PI * 2 / 4.5f)) / 2 + 1);
            }
        }
        [BurstCompile]
        public static void bounceOut(in float4 a, in float4 b, in float inT, out float4 result)
        {
            var t = inT;
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                float n1 = 7.5625f;
                float d1 = 2.75f;

                if (t < 1 / d1)
                {
                    result = a + (b - a) * (n1 * t * t);
                }
                else if (t < 2 / d1)
                {
                    result = a + (b - a) * (n1 * (t -= 1.5f / d1) * t + 0.75f);
                }
                else if (t < 2.5f / d1)
                {
                    result = a + (b - a) * (n1 * (t -= 2.25f / d1) * t + 0.9375f);
                }
                else
                {
                    result = a + (b - a) * (n1 * (t -= 2.625f / d1) * t + 0.984375f);
                }
            }

        }
        [BurstCompile]
        public static void bounceIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                bounceOut(0, 1, 1 - t, out var b1);
                result = a + (b - a) * (1 - b1);
            }
        }
        [BurstCompile]
        public static void bounceInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            if (t >= 1)
            {
                result = b;
            }
            else if (t <= 0)
            {
                result = a;
            }
            else
            {
                bounceOut(0, 1, 1 - 2 * t, out var b1);
                bounceOut(0, 1, 2 * t - 1, out var b2);
                result = t < 0.5f ?
                    a + (b - a) * ((1 - b1) / 2) :
                    a + (b - a) * ((1 + b2) / 2);
            }
        }
    }
}
