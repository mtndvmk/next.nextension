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
                    case AutoTransformType.AutoRotate:
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

        private static TransformAccessArray _transformAccessArray;
        private static Job _job;
        private static List<AbsAutoTransform> _autoTransformList;

        private readonly static SharedStatic<float> _deltaTime = SharedStatic<float>.GetOrCreate<Job>();

#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void reset()
        {
            if (_transformAccessArray.isCreated)
            {
                _transformAccessArray.Dispose();
                _job.autoTransformDatas.Dispose();
                _autoTransformList.Clear();
                NUpdater.onUpdateEvent.remove(update);
            }
        }
#endif
        private static void ensureSetup()
        {
            if (!_transformAccessArray.isCreated)
            {
                _transformAccessArray = new TransformAccessArray(4);
                _job.autoTransformDatas = new NativeList<AutoTransformData>(4, AllocatorManager.Persistent);
                _autoTransformList = new List<AbsAutoTransform>(4);
                NUpdater.onUpdateEvent.add(update);
            }
        }
        private static void update()
        {
            if (_autoTransformList.Count > 0)
            {
                _deltaTime.Data = Time.deltaTime;
                _job.Schedule(_transformAccessArray).Complete();
            }
        }

        public static int add(AbsAutoTransform autoTransform)
        {
            InternalCheck.checkEditorMode();
            ensureSetup();
            _transformAccessArray.Add(autoTransform.transform);
            _job.autoTransformDatas.Add(new()
            {
                type = autoTransform.AutoTransformType,
                data = autoTransform.AutoValue,
                isLocalSpace = autoTransform.IsWorldSpace
            });
            _autoTransformList.Add(autoTransform);
            return _transformAccessArray.length - 1;
        }
        public static void remove(int autoIndex)
        {
            if (_transformAccessArray.isCreated)
            {
                _autoTransformList[^1].autoIndex = autoIndex;
                _job.autoTransformDatas.RemoveAtSwapBack(autoIndex);
                _transformAccessArray.RemoveAtSwapBack(autoIndex);
                _autoTransformList.removeAtSwapBack(autoIndex);
            }
        }
        public static void updateAutoValue(AbsAutoTransform autoTransform)
        {
            _job.autoTransformDatas[autoTransform.autoIndex] = new()
            {
                type = autoTransform.AutoTransformType,
                data = autoTransform.AutoValue,
                isLocalSpace = autoTransform.IsWorldSpace
            };
        }
    }
    internal enum AutoTransformType
    {
        AutoRotate,
    }
    internal struct AutoTransformData
    {
        public AutoTransformType type;
        public bool isLocalSpace;
        public float3 data;
    }
}