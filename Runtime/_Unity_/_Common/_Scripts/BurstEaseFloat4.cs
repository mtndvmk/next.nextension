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
            result = math.lerp(a, b, math.saturate(t));
        }
        [BurstCompile]
        public static void sineIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, 1 - math.cos(ts * math.PI / 2));
        }
        [BurstCompile]
        public static void sineOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, math.sin(ts * math.PI / 2));
        }
        [BurstCompile]
        public static void sineInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, (math.cos(ts * math.PI) - 1) / -2);
        }
        [BurstCompile]
        public static void quadIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, ts * ts);
        }
        [BurstCompile]
        public static void quadOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = 1 - ts;
            result = math.lerp(a, b, 1 - t1 * t1);
        }
        [BurstCompile]
        public static void quadInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = -2 * ts + 2;
            float f = ts < 0.5f ? (2 * ts * ts) : (1 - t1 * t1 / 2);
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void cubicIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, ts * ts * ts);
        }
        [BurstCompile]
        public static void cubicOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = 1 - ts;
            result = math.lerp(a, b, 1 - t1 * t1 * t1);
        }
        [BurstCompile]
        public static void cubicInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = -2 * ts + 2;
            float f = (ts < 0.5f) ? (4 * ts * ts * ts) : (1 - t1 * t1 * t1 / 2);
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void quartIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, ts * ts * ts * ts);
        }
        [BurstCompile]
        public static void quartOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = 1 - ts;
            result = math.lerp(a, b, 1 - t1 * t1 * t1 * t1);
        }
        [BurstCompile]
        public static void quartInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = -2 * ts + 2;
            float f = ts < 0.5f ? (8 * ts * ts * ts * ts) : (1 - t1 * t1 * t1 * t1 / 2);
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void quintIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, ts * ts * ts * ts * ts);
        }
        [BurstCompile]
        public static void quintOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = 1 - ts;
            result = math.lerp(a, b, 1 - t1 * t1 * t1 * t1 * t1);
        }
        [BurstCompile]
        public static void quintInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = -2 * ts + 2;
            float f = ts < 0.5f ? 16 * ts * ts * ts * ts * ts : (1 - t1 * t1 * t1 * t1 * t1 / 2);
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void expoIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, math.pow(2, 10 * ts - 10));
        }
        [BurstCompile]
        public static void expoOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, 1 - math.pow(2, -10 * ts));
        }
        [BurstCompile]
        public static void expoInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float f = ts < 0.5f ? math.pow(2, 20 * ts - 10) / 2 : (2 - math.pow(2, -20 * ts + 10)) / 2;
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void circIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, 1 - math.sqrt(1 - ts * ts));
        }
        [BurstCompile]
        public static void circOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = ts - 1;
            result = math.lerp(a, b, math.sqrt(1 - t1 * t1));
        }
        [BurstCompile]
        public static void circInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = 2 * ts;
            float t2 = -2 * ts + 2;
            float f = ts < 0.5f ? (1 - math.sqrt(1 - t1 * t1)) / 2 : (math.sqrt(1 - t2 * t2) + 1) / 2;
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void backIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, 2.70158f * ts * ts * ts - 1.70158f * ts * ts);
        }
        [BurstCompile]
        public static void backOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = ts - 1;
            float t2 = t1 * t1;
            result = math.lerp(a, b, 1 + 2.70158f * t2 * t1 + 1.70158f * t2);
        }
        [BurstCompile]
        public static void backInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float t1 = ts * 2;
            float t2 = t1 - 2;
            float f = ts < 0.5f ?
                t1 * t1 * (3.5949095f * t1 - 2.5949095f) / 2 :
                (t2 * t2 * (3.5949095f * t2 + 2.5949095f) + 2) / 2;
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void elasticIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, -(math.pow(2, 10 * ts - 10) * math.sin((ts * 10 - 10.75f) * (2 * math.PI) / 3)));
        }
        [BurstCompile]
        public static void elasticOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            result = math.lerp(a, b, math.pow(2, -10 * ts) * math.sin((ts * 10 - 0.75f) * (math.PI * 2) / 3) + 1);
        }
        [BurstCompile]
        public static void elasticInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            float ts = math.saturate(t);
            float f = ts < 0.5f ?
               (math.pow(2, 20 * ts - 10) * math.sin((20 * ts - 11.125f) * (math.PI * 2 / 4.5f)) / -2) :
               (math.pow(2, -20 * ts + 10) * math.sin((20 * ts - 11.125f) * (math.PI * 2 / 4.5f)) / 2 + 1);
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void bounceOut(in float4 a, in float4 b, in float inT, out float4 result)
        {
            float t = math.saturate(inT);
            float n1 = 7.5625f;
            float d1 = 2.75f;
            float f;
            if (t < 1 / d1)
            {
                f = n1 * t * t;
            }
            else if (t < 2 / d1)
            {
                t -= 1.5f / d1;
                f = n1 * t * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                f = n1 * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / d1;
                f = n1 * t * t + 0.984375f;
            }
            result = math.lerp(a, b, f);
        }
        [BurstCompile]
        public static void bounceIn(in float4 a, in float4 b, in float t, out float4 result)
        {
            bounceOut(0, 1, 1 - t, out var b1);
            result = math.lerp(a, b, 1 - b1);
        }
        [BurstCompile]
        public static void bounceInOut(in float4 a, in float4 b, in float t, out float4 result)
        {
            bounceOut(0, 1, 1 - 2 * t, out var b1);
            bounceOut(0, 1, 2 * t - 1, out var b2);
            float4 f = t < 0.5f ? ((1 - b1) / 2) : ((1 + b2) / 2);
            result = math.lerp(a, b, f);
        }
    }
}
