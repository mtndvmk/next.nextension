using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Nextension
{
    internal static class AutoTransformSystem
    {
        [BurstCompile]
        internal struct Job : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableParallelForRestriction] internal NativeList<AutoTransformData> autoTransformDatas;
            [BurstCompile]
            public void Execute(int index, TransformAccess transform)
            {
                var data = autoTransformDatas[index];
                switch (data.type)
                {
                    case AutoTransformType.AutoMove:
                        {
                            if (data.isLocalSpace)
                            {
                                transform.localPosition += (Vector3)data.data * _deltaTime.Data;
                            }
                            else
                            {
                                transform.position += (Vector3)data.data * _deltaTime.Data;
                            }
                            break;
                        }
                    case AutoTransformType.AutoRotate:
                        {
                            if (data.isLocalSpace)
                            {
                                transform.localRotation *= quaternion.EulerXYZ(data.data * math.radians(_deltaTime.Data));
                            }
                            else
                            {
                                transform.rotation *= quaternion.EulerXYZ(data.data * math.radians(_deltaTime.Data));
                            }
                            break;
                        }
                }
            }
        }

        private static TransformAccessArray _transformAccessArray;
        private static Job _job;
        private static List<AutoTransformHandle> _handlers;

        private readonly static SharedStatic<float> _deltaTime = SharedStatic<float>.GetOrCreate<Job>();

#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void reset()
        {
            if (_transformAccessArray.isCreated)
            {
                _transformAccessArray.Dispose();
                _job.autoTransformDatas.Dispose();
                _handlers.Clear();
                NUpdater.onUpdateEvent.remove(update);
            }
        }
#endif
        private static void ensureSetup()
        {
            InternalCheck.checkEditorMode();
            if (!_transformAccessArray.isCreated)
            {
                _transformAccessArray = new TransformAccessArray(4);
                _job.autoTransformDatas = new NativeList<AutoTransformData>(4, AllocatorManager.Persistent);
                _handlers = new(4);
                NUpdater.onUpdateEvent.add(update);
            }
        }
        private static void update()
        {
            if (_handlers.Count > 0)
            {
                _deltaTime.Data = Time.deltaTime;
                _job.Schedule(_transformAccessArray).Complete();
            }
        }

        public static AutoTransformHandle start(AbsAutoTransform autoTransform)
        {
            return start(autoTransform.transform, autoTransform.AutoTransformType, autoTransform.AutoValue, autoTransform.IsLocalSpace);
        }
        public static AutoTransformHandle start(Transform transform, AutoTransformType type, float3 data, bool isLocalSpace)
        {
            ensureSetup();
            _transformAccessArray.Add(transform);
            _job.autoTransformDatas.Add(new()
            {
                type = type,
                data = data,
                isLocalSpace = isLocalSpace
            });
            var handler = new AutoTransformHandle(_transformAccessArray.length - 1);
            _handlers.Add(handler);
            return handler;
        }

        public static void stop(AutoTransformHandle handler)
        {
            if (_transformAccessArray.isCreated)
            {
                var index = handler.Index;
                _handlers[^1].Index = index;
                _job.autoTransformDatas.RemoveAtSwapBack(index);
                _transformAccessArray.RemoveAtSwapBack(index);
                _handlers.removeAtSwapBack(index);
            }
        }
        public static void updateAutoValue(AbsAutoTransform autoTransform)
        {
            updateAutoValue(autoTransform.handler, autoTransform.AutoTransformType, autoTransform.AutoValue, autoTransform.IsLocalSpace);
        }
        public static void updateAutoValue(AutoTransformHandle handler, AutoTransformType type, float3 data, bool isLocalSpace)
        {
            _job.autoTransformDatas[handler.Index] = new()
            {
                type = type,
                data = data,
                isLocalSpace = isLocalSpace
            };
        }
    }
    internal enum AutoTransformType
    {
        AutoMove,
        AutoRotate,
    }
    internal struct AutoTransformData
    {
        public AutoTransformType type;
        public bool isLocalSpace;
        public float3 data;
    }
}