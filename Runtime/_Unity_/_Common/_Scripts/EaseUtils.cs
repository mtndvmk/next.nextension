using Unity.Mathematics;

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
    /// Utility class for easing functions. Delegates logic to Burst-optimized BurstEaseFloat classes.
    /// </summary>
    public static class EaseUtils
    {
        public static float ease(float a, float b, float t, EaseType type)
        {
            BurstEaseFloat.ease(a, b, t, type, out float result);
            return result;
        }

        public static float2 ease(float2 a, float2 b, float t, EaseType type)
        {
            BurstEaseFloat2.ease(a, b, t, type, out float2 result);
            return result;
        }

        public static float3 ease(float3 a, float3 b, float t, EaseType type)
        {
            BurstEaseFloat3.ease(a, b, t, type, out float3 result);
            return result;
        }

        public static float4 ease(float4 a, float4 b, float t, EaseType type)
        {
            BurstEaseFloat4.ease(a, b, t, type, out float4 result);
            return result;
        }

        public static float lerp(float a, float b, float t) { BurstEaseFloat.lerp(a, b, t, out var result); return result; }
        public static float2 lerp(float2 a, float2 b, float t) { BurstEaseFloat2.lerp(a, b, t, out var result); return result; }
        public static float3 lerp(float3 a, float3 b, float t) { BurstEaseFloat3.lerp(a, b, t, out var result); return result; }
        public static float4 lerp(float4 a, float4 b, float t) { BurstEaseFloat4.lerp(a, b, t, out var result); return result; }

        public static float sineIn(float a, float b, float t) { BurstEaseFloat.sineIn(a, b, t, out var result); return result; }
        public static float2 sineIn(float2 a, float2 b, float t) { BurstEaseFloat2.sineIn(a, b, t, out var result); return result; }
        public static float3 sineIn(float3 a, float3 b, float t) { BurstEaseFloat3.sineIn(a, b, t, out var result); return result; }
        public static float4 sineIn(float4 a, float4 b, float t) { BurstEaseFloat4.sineIn(a, b, t, out var result); return result; }

        public static float sineOut(float a, float b, float t) { BurstEaseFloat.sineOut(a, b, t, out var result); return result; }
        public static float2 sineOut(float2 a, float2 b, float t) { BurstEaseFloat2.sineOut(a, b, t, out var result); return result; }
        public static float3 sineOut(float3 a, float3 b, float t) { BurstEaseFloat3.sineOut(a, b, t, out var result); return result; }
        public static float4 sineOut(float4 a, float4 b, float t) { BurstEaseFloat4.sineOut(a, b, t, out var result); return result; }

        public static float sineInOut(float a, float b, float t) { BurstEaseFloat.sineInOut(a, b, t, out var result); return result; }
        public static float2 sineInOut(float2 a, float2 b, float t) { BurstEaseFloat2.sineInOut(a, b, t, out var result); return result; }
        public static float3 sineInOut(float3 a, float3 b, float t) { BurstEaseFloat3.sineInOut(a, b, t, out var result); return result; }
        public static float4 sineInOut(float4 a, float4 b, float t) { BurstEaseFloat4.sineInOut(a, b, t, out var result); return result; }

        public static float quadIn(float a, float b, float t) { BurstEaseFloat.quadIn(a, b, t, out var result); return result; }
        public static float2 quadIn(float2 a, float2 b, float t) { BurstEaseFloat2.quadIn(a, b, t, out var result); return result; }
        public static float3 quadIn(float3 a, float3 b, float t) { BurstEaseFloat3.quadIn(a, b, t, out var result); return result; }
        public static float4 quadIn(float4 a, float4 b, float t) { BurstEaseFloat4.quadIn(a, b, t, out var result); return result; }

        public static float quadOut(float a, float b, float t) { BurstEaseFloat.quadOut(a, b, t, out var result); return result; }
        public static float2 quadOut(float2 a, float2 b, float t) { BurstEaseFloat2.quadOut(a, b, t, out var result); return result; }
        public static float3 quadOut(float3 a, float3 b, float t) { BurstEaseFloat3.quadOut(a, b, t, out var result); return result; }
        public static float4 quadOut(float4 a, float4 b, float t) { BurstEaseFloat4.quadOut(a, b, t, out var result); return result; }

        public static float quadInOut(float a, float b, float t) { BurstEaseFloat.quadInOut(a, b, t, out var result); return result; }
        public static float2 quadInOut(float2 a, float2 b, float t) { BurstEaseFloat2.quadInOut(a, b, t, out var result); return result; }
        public static float3 quadInOut(float3 a, float3 b, float t) { BurstEaseFloat3.quadInOut(a, b, t, out var result); return result; }
        public static float4 quadInOut(float4 a, float4 b, float t) { BurstEaseFloat4.quadInOut(a, b, t, out var result); return result; }

        public static float cubicIn(float a, float b, float t) { BurstEaseFloat.cubicIn(a, b, t, out var result); return result; }
        public static float2 cubicIn(float2 a, float2 b, float t) { BurstEaseFloat2.cubicIn(a, b, t, out var result); return result; }
        public static float3 cubicIn(float3 a, float3 b, float t) { BurstEaseFloat3.cubicIn(a, b, t, out var result); return result; }
        public static float4 cubicIn(float4 a, float4 b, float t) { BurstEaseFloat4.cubicIn(a, b, t, out var result); return result; }

        public static float cubicOut(float a, float b, float t) { BurstEaseFloat.cubicOut(a, b, t, out var result); return result; }
        public static float2 cubicOut(float2 a, float2 b, float t) { BurstEaseFloat2.cubicOut(a, b, t, out var result); return result; }
        public static float3 cubicOut(float3 a, float3 b, float t) { BurstEaseFloat3.cubicOut(a, b, t, out var result); return result; }
        public static float4 cubicOut(float4 a, float4 b, float t) { BurstEaseFloat4.cubicOut(a, b, t, out var result); return result; }

        public static float cubicInOut(float a, float b, float t) { BurstEaseFloat.cubicInOut(a, b, t, out var result); return result; }
        public static float2 cubicInOut(float2 a, float2 b, float t) { BurstEaseFloat2.cubicInOut(a, b, t, out var result); return result; }
        public static float3 cubicInOut(float3 a, float3 b, float t) { BurstEaseFloat3.cubicInOut(a, b, t, out var result); return result; }
        public static float4 cubicInOut(float4 a, float4 b, float t) { BurstEaseFloat4.cubicInOut(a, b, t, out var result); return result; }

        public static float quartIn(float a, float b, float t) { BurstEaseFloat.quartIn(a, b, t, out var result); return result; }
        public static float2 quartIn(float2 a, float2 b, float t) { BurstEaseFloat2.quartIn(a, b, t, out var result); return result; }
        public static float3 quartIn(float3 a, float3 b, float t) { BurstEaseFloat3.quartIn(a, b, t, out var result); return result; }
        public static float4 quartIn(float4 a, float4 b, float t) { BurstEaseFloat4.quartIn(a, b, t, out var result); return result; }

        public static float quartOut(float a, float b, float t) { BurstEaseFloat.quartOut(a, b, t, out var result); return result; }
        public static float2 quartOut(float2 a, float2 b, float t) { BurstEaseFloat2.quartOut(a, b, t, out var result); return result; }
        public static float3 quartOut(float3 a, float3 b, float t) { BurstEaseFloat3.quartOut(a, b, t, out var result); return result; }
        public static float4 quartOut(float4 a, float4 b, float t) { BurstEaseFloat4.quartOut(a, b, t, out var result); return result; }

        public static float quartInOut(float a, float b, float t) { BurstEaseFloat.quartInOut(a, b, t, out var result); return result; }
        public static float2 quartInOut(float2 a, float2 b, float t) { BurstEaseFloat2.quartInOut(a, b, t, out var result); return result; }
        public static float3 quartInOut(float3 a, float3 b, float t) { BurstEaseFloat3.quartInOut(a, b, t, out var result); return result; }
        public static float4 quartInOut(float4 a, float4 b, float t) { BurstEaseFloat4.quartInOut(a, b, t, out var result); return result; }

        public static float quintIn(float a, float b, float t) { BurstEaseFloat.quintIn(a, b, t, out var result); return result; }
        public static float2 quintIn(float2 a, float2 b, float t) { BurstEaseFloat2.quintIn(a, b, t, out var result); return result; }
        public static float3 quintIn(float3 a, float3 b, float t) { BurstEaseFloat3.quintIn(a, b, t, out var result); return result; }
        public static float4 quintIn(float4 a, float4 b, float t) { BurstEaseFloat4.quintIn(a, b, t, out var result); return result; }

        public static float quintOut(float a, float b, float t) { BurstEaseFloat.quintOut(a, b, t, out var result); return result; }
        public static float2 quintOut(float2 a, float2 b, float t) { BurstEaseFloat2.quintOut(a, b, t, out var result); return result; }
        public static float3 quintOut(float3 a, float3 b, float t) { BurstEaseFloat3.quintOut(a, b, t, out var result); return result; }
        public static float4 quintOut(float4 a, float4 b, float t) { BurstEaseFloat4.quintOut(a, b, t, out var result); return result; }

        public static float quintInOut(float a, float b, float t) { BurstEaseFloat.quintInOut(a, b, t, out var result); return result; }
        public static float2 quintInOut(float2 a, float2 b, float t) { BurstEaseFloat2.quintInOut(a, b, t, out var result); return result; }
        public static float3 quintInOut(float3 a, float3 b, float t) { BurstEaseFloat3.quintInOut(a, b, t, out var result); return result; }
        public static float4 quintInOut(float4 a, float4 b, float t) { BurstEaseFloat4.quintInOut(a, b, t, out var result); return result; }

        public static float expoIn(float a, float b, float t) { BurstEaseFloat.expoIn(a, b, t, out var result); return result; }
        public static float2 expoIn(float2 a, float2 b, float t) { BurstEaseFloat2.expoIn(a, b, t, out var result); return result; }
        public static float3 expoIn(float3 a, float3 b, float t) { BurstEaseFloat3.expoIn(a, b, t, out var result); return result; }
        public static float4 expoIn(float4 a, float4 b, float t) { BurstEaseFloat4.expoIn(a, b, t, out var result); return result; }

        public static float expoOut(float a, float b, float t) { BurstEaseFloat.expoOut(a, b, t, out var result); return result; }
        public static float2 expoOut(float2 a, float2 b, float t) { BurstEaseFloat2.expoOut(a, b, t, out var result); return result; }
        public static float3 expoOut(float3 a, float3 b, float t) { BurstEaseFloat3.expoOut(a, b, t, out var result); return result; }
        public static float4 expoOut(float4 a, float4 b, float t) { BurstEaseFloat4.expoOut(a, b, t, out var result); return result; }

        public static float expoInOut(float a, float b, float t) { BurstEaseFloat.expoInOut(a, b, t, out var result); return result; }
        public static float2 expoInOut(float2 a, float2 b, float t) { BurstEaseFloat2.expoInOut(a, b, t, out var result); return result; }
        public static float3 expoInOut(float3 a, float3 b, float t) { BurstEaseFloat3.expoInOut(a, b, t, out var result); return result; }
        public static float4 expoInOut(float4 a, float4 b, float t) { BurstEaseFloat4.expoInOut(a, b, t, out var result); return result; }

        public static float circIn(float a, float b, float t) { BurstEaseFloat.circIn(a, b, t, out var result); return result; }
        public static float2 circIn(float2 a, float2 b, float t) { BurstEaseFloat2.circIn(a, b, t, out var result); return result; }
        public static float3 circIn(float3 a, float3 b, float t) { BurstEaseFloat3.circIn(a, b, t, out var result); return result; }
        public static float4 circIn(float4 a, float4 b, float t) { BurstEaseFloat4.circIn(a, b, t, out var result); return result; }

        public static float circOut(float a, float b, float t) { BurstEaseFloat.circOut(a, b, t, out var result); return result; }
        public static float2 circOut(float2 a, float2 b, float t) { BurstEaseFloat2.circOut(a, b, t, out var result); return result; }
        public static float3 circOut(float3 a, float3 b, float t) { BurstEaseFloat3.circOut(a, b, t, out var result); return result; }
        public static float4 circOut(float4 a, float4 b, float t) { BurstEaseFloat4.circOut(a, b, t, out var result); return result; }

        public static float circInOut(float a, float b, float t) { BurstEaseFloat.circInOut(a, b, t, out var result); return result; }
        public static float2 circInOut(float2 a, float2 b, float t) { BurstEaseFloat2.circInOut(a, b, t, out var result); return result; }
        public static float3 circInOut(float3 a, float3 b, float t) { BurstEaseFloat3.circInOut(a, b, t, out var result); return result; }
        public static float4 circInOut(float4 a, float4 b, float t) { BurstEaseFloat4.circInOut(a, b, t, out var result); return result; }

        public static float backIn(float a, float b, float t) { BurstEaseFloat.backIn(a, b, t, out var result); return result; }
        public static float2 backIn(float2 a, float2 b, float t) { BurstEaseFloat2.backIn(a, b, t, out var result); return result; }
        public static float3 backIn(float3 a, float3 b, float t) { BurstEaseFloat3.backIn(a, b, t, out var result); return result; }
        public static float4 backIn(float4 a, float4 b, float t) { BurstEaseFloat4.backIn(a, b, t, out var result); return result; }

        public static float backOut(float a, float b, float t) { BurstEaseFloat.backOut(a, b, t, out var result); return result; }
        public static float2 backOut(float2 a, float2 b, float t) { BurstEaseFloat2.backOut(a, b, t, out var result); return result; }
        public static float3 backOut(float3 a, float3 b, float t) { BurstEaseFloat3.backOut(a, b, t, out var result); return result; }
        public static float4 backOut(float4 a, float4 b, float t) { BurstEaseFloat4.backOut(a, b, t, out var result); return result; }

        public static float backInOut(float a, float b, float t) { BurstEaseFloat.backInOut(a, b, t, out var result); return result; }
        public static float2 backInOut(float2 a, float2 b, float t) { BurstEaseFloat2.backInOut(a, b, t, out var result); return result; }
        public static float3 backInOut(float3 a, float3 b, float t) { BurstEaseFloat3.backInOut(a, b, t, out var result); return result; }
        public static float4 backInOut(float4 a, float4 b, float t) { BurstEaseFloat4.backInOut(a, b, t, out var result); return result; }

        public static float elasticIn(float a, float b, float t) { BurstEaseFloat.elasticIn(a, b, t, out var result); return result; }
        public static float2 elasticIn(float2 a, float2 b, float t) { BurstEaseFloat2.elasticIn(a, b, t, out var result); return result; }
        public static float3 elasticIn(float3 a, float3 b, float t) { BurstEaseFloat3.elasticIn(a, b, t, out var result); return result; }
        public static float4 elasticIn(float4 a, float4 b, float t) { BurstEaseFloat4.elasticIn(a, b, t, out var result); return result; }

        public static float elasticOut(float a, float b, float t) { BurstEaseFloat.elasticOut(a, b, t, out var result); return result; }
        public static float2 elasticOut(float2 a, float2 b, float t) { BurstEaseFloat2.elasticOut(a, b, t, out var result); return result; }
        public static float3 elasticOut(float3 a, float3 b, float t) { BurstEaseFloat3.elasticOut(a, b, t, out var result); return result; }
        public static float4 elasticOut(float4 a, float4 b, float t) { BurstEaseFloat4.elasticOut(a, b, t, out var result); return result; }

        public static float elasticInOut(float a, float b, float t) { BurstEaseFloat.elasticInOut(a, b, t, out var result); return result; }
        public static float2 elasticInOut(float2 a, float2 b, float t) { BurstEaseFloat2.elasticInOut(a, b, t, out var result); return result; }
        public static float3 elasticInOut(float3 a, float3 b, float t) { BurstEaseFloat3.elasticInOut(a, b, t, out var result); return result; }
        public static float4 elasticInOut(float4 a, float4 b, float t) { BurstEaseFloat4.elasticInOut(a, b, t, out var result); return result; }

        public static float bounceIn(float a, float b, float t) { BurstEaseFloat.bounceIn(a, b, t, out var result); return result; }
        public static float2 bounceIn(float2 a, float2 b, float t) { BurstEaseFloat2.bounceIn(a, b, t, out var result); return result; }
        public static float3 bounceIn(float3 a, float3 b, float t) { BurstEaseFloat3.bounceIn(a, b, t, out var result); return result; }
        public static float4 bounceIn(float4 a, float4 b, float t) { BurstEaseFloat4.bounceIn(a, b, t, out var result); return result; }

        public static float bounceOut(float a, float b, float t) { BurstEaseFloat.bounceOut(a, b, t, out var result); return result; }
        public static float2 bounceOut(float2 a, float2 b, float t) { BurstEaseFloat2.bounceOut(a, b, t, out var result); return result; }
        public static float3 bounceOut(float3 a, float3 b, float t) { BurstEaseFloat3.bounceOut(a, b, t, out var result); return result; }
        public static float4 bounceOut(float4 a, float4 b, float t) { BurstEaseFloat4.bounceOut(a, b, t, out var result); return result; }

        public static float bounceInOut(float a, float b, float t) { BurstEaseFloat.bounceInOut(a, b, t, out var result); return result; }
        public static float2 bounceInOut(float2 a, float2 b, float t) { BurstEaseFloat2.bounceInOut(a, b, t, out var result); return result; }
        public static float3 bounceInOut(float3 a, float3 b, float t) { BurstEaseFloat3.bounceInOut(a, b, t, out var result); return result; }
        public static float4 bounceInOut(float4 a, float4 b, float t) { BurstEaseFloat4.bounceInOut(a, b, t, out var result); return result; }
    }
}
