using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension.Tween
{
    internal static class NTweenUtils
    {
        static NTweenUtils()
        {
            _supportedDataTypeTable = new()
            {
                { typeof(float), SupportedDataType.Float },
                { typeof(float2), SupportedDataType.Float2 },
                { typeof(float3), SupportedDataType.Float3 },
                { typeof(float4), SupportedDataType.Float4 },
                { typeof(Vector2), SupportedDataType.Float2 },
                { typeof(Vector3), SupportedDataType.Float3 },
                { typeof(Vector4), SupportedDataType.Float4 },
                { typeof(Color), SupportedDataType.Float4 },
                { typeof(Quaternion), SupportedDataType.Float4 },
                { typeof(quaternion), SupportedDataType.Float4 },
            };
        }
        private readonly static Dictionary<Type, SupportedDataType> _supportedDataTypeTable;
        public static SupportedDataType getSupportedDataType<T>() where T : unmanaged
        {
            if (_supportedDataTypeTable.TryGetValue(typeof(T), out var type))
            {
                return type;
            }
            return SupportedDataType.NotSupported;
        }
        [BurstCompile]
        public static void ease<T>(T a, T b, float t, EaseType easeType, out T result) where T : unmanaged
        {
            switch (ISupportedDataType<T>.type)
            {
                case SupportedDataType.Float:
                    {
                        BurstEaseFloat.ease(NConverter.bitConvert<T, float>(a), NConverter.bitConvert<T, float>(b), t, easeType, out var fResult);
                        result = NConverter.bitConvert<float, T>(fResult);
                        break;
                    }
                case SupportedDataType.Float2:
                    {
                        BurstEaseFloat2.ease(NConverter.bitConvert<T, float2>(a), NConverter.bitConvert<T, float2>(b), t, easeType, out var fResult);
                        result = NConverter.bitConvert<float2, T>(fResult);
                        break;
                    }
                case SupportedDataType.Float3:
                    {
                        BurstEaseFloat3.ease(NConverter.bitConvert<T, float3>(a), NConverter.bitConvert<T, float3>(b), t, easeType, out var fResult);
                        result = NConverter.bitConvert<float3, T>(fResult);
                        break;
                    }
                case SupportedDataType.Float4:
                    {
                        BurstEaseFloat4.ease(NConverter.bitConvert<T, float4>(a), NConverter.bitConvert<T, float4>(b), t, easeType, out var fResult);
                        result = NConverter.bitConvert<float4, T>(fResult);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
        [BurstCompile]
        public static T randShakeValue<T>(uint seed, float range) where T : unmanaged
        {
            Unity.Mathematics.Random rand = new(seed);
            return ISupportedDataType<T>.type switch
            {
                SupportedDataType.Float => NConverter.bitConvert<float, T>((rand.NextFloat() - 0.5f) * range),
                SupportedDataType.Float2 => NConverter.bitConvert<float2, T>((rand.NextFloat2() - 0.5f) * range),
                SupportedDataType.Float3 => NConverter.bitConvert<float3, T>((rand.NextFloat3() - 0.5f) * range),
                SupportedDataType.Float4 => NConverter.bitConvert<float4, T>((rand.NextFloat4() - 0.5f) * range),
                _ => throw new NotImplementedException(),
            };
        }
        [BurstCompile]
        public static T addValue<T>(T a, T b) where T : unmanaged
        {
            return ISupportedDataType<T>.type switch
            {
                SupportedDataType.Float => NConverter.bitConvert<float, T>(NConverter.bitConvert<T, float>(a) + NConverter.bitConvert<T, float>(b)),
                SupportedDataType.Float2 => NConverter.bitConvert<float2, T>(NConverter.bitConvert<T, float2>(a) + NConverter.bitConvert<T, float2>(b)),
                SupportedDataType.Float3 => NConverter.bitConvert<float3, T>(NConverter.bitConvert<T, float3>(a) + NConverter.bitConvert<T, float3>(b)),
                SupportedDataType.Float4 => NConverter.bitConvert<float4, T>(NConverter.bitConvert<T, float4>(a) + NConverter.bitConvert<T, float4>(b)),
                _ => throw new NotImplementedException(),
            };
        }
        [BurstCompile]
        public static void applyTransformAccessJobData<TValue>(TransformTweenType transformTweenType, TransformAccess transform, TValue result) where TValue : unmanaged
        {
            switch (transformTweenType)
            {
                case TransformTweenType.Local_Position:
                    transform.localPosition = NConverter.bitConvert<TValue, Vector3>(result);
                    break;
                case TransformTweenType.World_Position:
                    transform.position = NConverter.bitConvert<TValue, Vector3>(result);
                    break;
                case TransformTweenType.Local_Scale:
                    transform.localScale = NConverter.bitConvert<TValue, Vector3>(result);
                    break;
                case TransformTweenType.Local_Rotation:
                    transform.localRotation = NConverter.bitConvert<TValue, Quaternion>(result);
                    break;
                case TransformTweenType.World_Rotation:
                    transform.rotation = NConverter.bitConvert<TValue, Quaternion>(result);
                    break;
            }
        }
    }

    internal interface ISupportedDataType<T> where T : unmanaged
    {
        static readonly SupportedDataType type = NTweenUtils.getSupportedDataType<T>();
    }
}