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
            internal float deltaTime;
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
                                transform.localPosition += (Vector3)data.data * deltaTime;
                            }
                            else
                            {
                                transform.position += (Vector3)data.data * deltaTime;
                            }
                            break;
                        }
                    case AutoTransformType.AutoRotate:
                        {
                            if (data.isLocalSpace)
                            {
                                transform.localRotation *= quaternion.EulerXYZ(data.data * math.radians(deltaTime));
                            }
                            else
                            {
                                transform.rotation *= quaternion.EulerXYZ(data.data * math.radians(deltaTime));
                            }
                            break;
                        }
                }
            }
        }

        private static TransformAccessArray _transformAccessArray;
        private static Job _job;
        private static List<AutoTransformHandle> _handlers;

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
            EditorCheck.checkEditorMode();
            if (!_transformAccessArray.isCreated)
            {
                _transformAccessArray = new TransformAccessArray(4);
                _job.autoTransformDatas = new NativeList<AutoTransformData>(4, AllocatorManager.Persistent);
                _handlers = new List<AutoTransformHandle>(4);
                NUpdater.onUpdateEvent.add(update);
            }
        }
        private static void update()
        {
            int handlerCount = _handlers.Count;
            if (handlerCount > 0)
            {
                _job.deltaTime = Time.deltaTime;
                _job.Schedule(_transformAccessArray).Complete();
            }
        }

        public static AutoTransformHandle start(AbsAutoTransform autoTransform)
        {
            var handler = start(autoTransform.transform, autoTransform.AutoTransformType, autoTransform.AutoValue, autoTransform.IsLocalSpace);
            return handler;
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
            var index = _transformAccessArray.length - 1;
            var handler = AutoTransformHandle.create(index);
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
                AutoTransformHandle.release(handler);
            }
        }
        public static void updateAutoValue(AbsAutoTransform autoTransform)
        {
            updateAutoValue(autoTransform.handler, autoTransform.AutoTransformType, autoTransform.AutoValue, autoTransform.IsLocalSpace);
        }
        public static void updateAutoValue(AutoTransformHandle handler, AutoTransformType type, float3 data, bool isLocalSpace)
        {
            _job.autoTransformDatas[handler.Index] = new AutoTransformData()
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