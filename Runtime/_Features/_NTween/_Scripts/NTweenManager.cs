using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Nextension.Tween
{
    public static class NTweenManager
    {
        private static List<NTweener> _inQueueTweeners;
        private static Dictionary<TweenType, AbsTweenRunner> _runners;
        private static CancelControlManager _cancelControlManager;

        [StartupMethod]
        private static void initialize()
        {
            _inQueueTweeners = new List<NTweener>();
            _runners = new Dictionary<TweenType, AbsTweenRunner>();
            _cancelControlManager = new CancelControlManager();

            NUpdater.onUpdateEvent.add(update);
            Application.quitting -= onApplicationQuit;
            Application.quitting += onApplicationQuit;
        }
        private static void onApplicationQuit()
        {
            foreach (var runner in _runners.Values)
            {
                runner.dispose();
            }
            _runners.Clear();
        }
        static void update()
        {
            _cancelControlManager.cancelInvalid();

            var currentTime = Time.time;
            TweenStaticManager.currentTimeInJob.Data = currentTime;
            TweenStaticManager.currentTime = currentTime;

            if (_inQueueTweeners.Count > 0)
            {
                for (int i = 0; i < _inQueueTweeners.Count; ++i)
                {
                    startTweener(_inQueueTweeners[i]);
                }
                _inQueueTweeners.Clear();
            }

            var capacity = AbsTweenChunk.ChunkCount;
            NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(capacity, AllocatorManager.Temp);
            List<AbsTweenChunk> runningChunks = new List<AbsTweenChunk>(capacity);

            foreach (var runner in _runners.Values)
            {
                runner.runTweenJob(jobHandles, runningChunks);
            }

            if (runningChunks.Count == 0)
            {
                return;
            }

            JobHandle.CompleteAll(jobHandles);

            for (int i = 0; i < runningChunks.Count; ++i)
            {
                runningChunks[i].invokeJobComplete();
            }
        }
        private static AbsTweenRunner getOrCreateRunner(TweenType tweenType)
        {
            AbsTweenRunner runner;
            switch (tweenType)
            {
                case TweenType.Transform_Local_Move:
                case TweenType.Transform_World_Move:
                case TweenType.Transform_Local_Scale:
                    if (!_runners.TryGetValue(TweenType.Transform_Local_Move, out runner))
                    {
                        runner = new TransformFloat3Runner();
                        _runners.Add(TweenType.Transform_Local_Move, runner);
                    }
                    return runner;
                case TweenType.Transform_Local_Rotate:
                case TweenType.Transform_World_Rotate:
                    if (!_runners.TryGetValue(TweenType.Transform_Local_Rotate, out runner))
                    {
                        runner = new TransformFloat4Runner();
                        _runners.Add(TweenType.Transform_Local_Rotate, runner);
                    }
                    return runner;
                case TweenType.Basic_Float_Tween:
                    if (!_runners.TryGetValue(tweenType, out runner))
                    {
                        runner = new BasicFloatTweenRunner();
                        _runners.Add(tweenType, runner);
                    }
                    return runner;
                case TweenType.Basic_Float2_Tween:
                    if (!_runners.TryGetValue(tweenType, out runner))
                    {
                        runner = new BasicFloat2TweenRunner();
                        _runners.Add(tweenType, runner);
                    }
                    return runner;
                case TweenType.Basic_Float3_Tween:
                    if (!_runners.TryGetValue(tweenType, out runner))
                    {
                        runner = new BasicFloat3TweenRunner();
                        _runners.Add(tweenType, runner);
                    }
                    return runner;
                case TweenType.Basic_Float4_Tween:
                    if (!_runners.TryGetValue(tweenType, out runner))
                    {
                        runner = new BasicFloat4TweenRunner();
                        _runners.Add(tweenType, runner);
                    }
                    return runner;
                default:
                    throw new NotImplementedException(tweenType.ToString());

            }
        }
        private static void startTweener(NTweener tweener)
        {
            if (tweener.chunkIndex.chunkId != 0 || tweener.Status != RunState.None)
            {
                return;
            }
            try
            {
                if (tweener.checkCancelFromFunc())
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }
            bool isCompletedTweener = tweener.duration <= 0;
            if (isCompletedTweener)
            {
                tweener.invokeOnStart();
                tweener.doCompleteOnStart();
                tweener.invokeOnComplete();
            }
            else
            {
                AbsTweenRunner runner = getOrCreateRunner(tweener.tweenType);
                runner.addTweener(tweener);
            }
        }

        public static NTweener moveTo(Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            if (isLocalSpace)
            {
                return createTransform3Tweener(target, destination, duration, TweenType.Transform_Local_Move, TweenLoopType.Normal);
            }
            else
            {
                return createTransform3Tweener(target, destination, duration, TweenType.Transform_World_Move, TweenLoopType.Normal);
            }
        }
        public static NTweener rotateTo(Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            var f4Destination = Quaternion.Euler(destination).toFloat4();
            if (isLocalSpace)
            {
                return createTransform4Tweener(target, f4Destination, duration, TweenType.Transform_Local_Rotate, TweenLoopType.Normal);
            }
            else
            {
                return createTransform4Tweener(target, f4Destination, duration, TweenType.Transform_World_Rotate, TweenLoopType.Normal);
            }
        }
        public static NTweener rotateTo(Transform target, Quaternion destination, float duration, bool isLocalSpace = true)
        {
            if (isLocalSpace)
            {
                return createTransform4Tweener(target, destination.toFloat4(), duration, TweenType.Transform_Local_Rotate, TweenLoopType.Normal);
            }
            else
            {
                return createTransform4Tweener(target, destination.toFloat4(), duration, TweenType.Transform_World_Rotate, TweenLoopType.Normal);
            }
        }
        public static NTweener scaleTo(Transform target, Vector3 destination, float duration)
        {
            return createTransform3Tweener(target, destination, duration, TweenType.Transform_Local_Scale, TweenLoopType.Normal);
        }


        public static NTweener punchMove(Transform target, Vector3 punchDestination, float duration, bool isLocalSpace = true)
        {
            if (isLocalSpace)
            {
                return createTransform3Tweener(target, punchDestination, duration, TweenType.Transform_Local_Move, TweenLoopType.Punch);
            }
            else
            {
                return createTransform3Tweener(target, punchDestination, duration, TweenType.Transform_World_Move, TweenLoopType.Punch);
            }
        }
        public static NTweener punchRotate(Transform target, Vector3 destination, float duration, bool isLocalSpace = true)
        {
            var f4Destination = Quaternion.Euler(destination).toFloat4();
            if (isLocalSpace)
            {
                return createTransform4Tweener(target, f4Destination, duration, TweenType.Transform_Local_Rotate, TweenLoopType.Punch);
            }
            else
            {
                return createTransform4Tweener(target, f4Destination, duration, TweenType.Transform_World_Rotate, TweenLoopType.Punch);
            }
        }
        public static NTweener punchRotate(Transform target, Quaternion punchDestination, float duration, bool isLocalSpace = true)
        {
            if (isLocalSpace)
            {
                return createTransform4Tweener(target, punchDestination.toFloat4(), duration, TweenType.Transform_Local_Rotate, TweenLoopType.Punch);
            }
            else
            {
                return createTransform4Tweener(target, punchDestination.toFloat4(), duration, TweenType.Transform_World_Rotate, TweenLoopType.Punch);
            }
        }
        public static NTweener punchScale(Transform target, Vector3 punchDestination, float duration)
        {
            return createTransform3Tweener(target, punchDestination, duration, TweenType.Transform_Local_Scale, TweenLoopType.Punch);
        }

        public static NTweener shakePosition(Transform target, float distance, float duration, bool isLocalSpace = true)
        {
            if (isLocalSpace)
            {
                return createTransform3Tweener(target, new float3(distance, 0, 0), duration, TweenType.Transform_Local_Move, TweenLoopType.Shake);
            }
            else
            {
                return createTransform3Tweener(target, new float3(distance, 0, 0), duration, TweenType.Transform_World_Move, TweenLoopType.Shake);
            }
        }

        public static NTweener fromTo(float from, float destination, Action<float> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float_Tween, TweenLoopType.Normal);
        }
        public static NTweener fromTo(float2 from, float2 destination, Action<float2> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float2_Tween, TweenLoopType.Normal);
        }
        public static NTweener fromTo(float3 from, float3 destination, Action<float3> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float3_Tween, TweenLoopType.Normal);
        }
        public static NTweener fromTo(float4 from, float4 destination, Action<float4> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float4_Tween, TweenLoopType.Normal);
        }
        public static NTweener fromTo(Vector3 from, Vector3 destination, Action<float3> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float3_Tween, TweenLoopType.Normal);
        }
        public static NTweener fromTo(Quaternion from, Quaternion destination, Action<Quaternion> onChanged, float duration)
        {
            return createBasicFromToTweener(from.toFloat4(), destination.toFloat4(), (result) => onChanged(result.toQuaternion()), duration, TweenType.Basic_Float4_Tween, TweenLoopType.Normal);
        }

        public static NTweener punchValue(float from, float destination, Action<float> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float_Tween, TweenLoopType.Punch);
        }
        public static NTweener punchValue(float2 from, float2 destination, Action<float2> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float2_Tween, TweenLoopType.Punch);
        }
        public static NTweener punchValue(float3 from, float3 destination, Action<float3> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float3_Tween, TweenLoopType.Punch);
        }
        public static NTweener punchValue(float4 from, float4 destination, Action<float4> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float4_Tween, TweenLoopType.Punch);
        }
        public static NTweener punchValue(Vector3 from, Vector3 destination, Action<float3> onChanged, float duration)
        {
            return createBasicFromToTweener(from, destination, onChanged, duration, TweenType.Basic_Float3_Tween, TweenLoopType.Punch);
        }
        public static NTweener punchValue(Quaternion from, Quaternion destination, Action<Quaternion> onChanged, float duration)
        {
            return createBasicFromToTweener(from.toFloat4(), destination.toFloat4(), (result) => onChanged(result.toQuaternion()), duration, TweenType.Basic_Float4_Tween, TweenLoopType.Punch);
        }

        private static NTweener createTransform3Tweener(Transform target, float3 destination, float duration, TweenType tweenType, TweenLoopType tweenLoopType)
        {
            var tweener = new TransformFloat3Tweener(target, destination, tweenType);
            tweener.duration = duration;
            tweener.startTime = Time.time;
            tweener.tweenLoopType = tweenLoopType;
            _inQueueTweeners.Add(tweener);
            return tweener;
        }
        private static NTweener createTransform4Tweener(Transform target, float4 destination, float duration, TweenType tweenType, TweenLoopType tweenLoopType)
        {
            var tweener = new TransformFloat4Tweener(target, destination, tweenType);
            tweener.duration = duration;
            tweener.startTime = Time.time;
            tweener.tweenLoopType = tweenLoopType;
            _inQueueTweeners.Add(tweener);
            return tweener;
        }
        private static NTweener createBasicFromToTweener<T>(T from, T destination, Action<T> onChanged, float duration, TweenType tweenType, TweenLoopType tweenLoopType) where T : struct
        {
            NTweener tweener;
            tweener = new BasicTweener<T>(from, destination, onChanged, tweenType);
            tweener.duration = duration;
            tweener.startTime = Time.time;
            tweener.tweenLoopType = tweenLoopType;
            _inQueueTweeners.Add(tweener);
            return tweener;
        }

        internal static void cancelFromTweener(ChunkIndex chunkIndex)
        {
            getOrCreateRunner(chunkIndex.type).cancelTween(chunkIndex);
        }
        internal static void cancelFromControlledTweener(AbsCancelControlKey controlKey)
        {
            _cancelControlManager.cancel(controlKey);
        }
        internal static void addCancelControlledTweener(NTweener tweener)
        {
            _cancelControlManager.addControlledTweener(tweener);
        }
        internal static void removeControlledTweener(NTweener tweener)
        {
            _cancelControlManager.removeControlledTweener(tweener);
        }
    }
}