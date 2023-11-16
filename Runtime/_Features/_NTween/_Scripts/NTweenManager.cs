using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.Tween
{
    internal static class NTweenManager
    {
        private static List<NRunnableTweener> _inQueueRunnableTweeners;
        private static List<CombinedNTweener> _inCombinedTweeners;

        private static Dictionary<uint, AbsTweenRunner> _runners;
        private static CancelControlManager _cancelControlManager;
        private static bool _isInitialized;
        private static void initalize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                _inQueueRunnableTweeners = new List<NRunnableTweener>();
                _inCombinedTweeners = new List<CombinedNTweener>();
                _runners = new Dictionary<uint, AbsTweenRunner>();
                _cancelControlManager = new CancelControlManager();

                NUpdater.onUpdateEvent.add(update);
            }
        }
        [EditorQuittingMethod]
        private static void onApplicationQuit()
        {
            if (_isInitialized)
            {
                _isInitialized = false;
                foreach (var runner in _runners.Values)
                {
                    runner.dispose();
                }
                _runners = null;
                _cancelControlManager = null;
                _inQueueRunnableTweeners = null;
                _inCombinedTweeners = null;
            }
        }
        static void update()
        {
            var currentTime = TweenStaticManager.currentTime = TweenStaticManager.currentTimeInJob.Data = Time.time;

            _cancelControlManager.cancelInvalid();

            int combinedCount = _inCombinedTweeners.Count;
            if (combinedCount > 0)
            {
                var inCombinedSpan = _inCombinedTweeners.asSpan();
                for (int i = combinedCount - 1; i >= 0; i--)
                {
                    var tweener = inCombinedSpan[i];
                    if (tweener.startTime <= currentTime)
                    {
                        tweener.invokeOnStart();
                        _inCombinedTweeners.removeAtSwapBack(i);
                    }
                }
            }

            int runnableCount = _inQueueRunnableTweeners.Count;
            if (runnableCount > 0)
            {
                var runnableSpan = _inQueueRunnableTweeners.asSpan();
                for (int i = 0; i < runnableCount; ++i)
                {
                    startRunnableTweener(runnableSpan[i]);
                }
                _inQueueRunnableTweeners.Clear();
            }

            if (TweenStaticManager.runningTweenerCount > 0)
            {
                NativeList<JobHandle> jobHandles = new(TweenChunk.ChunkCount, AllocatorManager.Temp);
                NativeList<(uint runnerId, uint chunkId)> runningChunks = new(TweenChunk.ChunkCount, AllocatorManager.Temp);

                foreach (var runner in _runners.Values)
                {
                    runner.runTweenJob(jobHandles, runningChunks);
                }

                int runningChunkCount = runningChunks.Length;
                if (runningChunkCount > 0)
                {
                    JobHandle.CompleteAll(jobHandles);
                    for (int i = 0; i < runningChunkCount; ++i)
                    {
                        var (runnerId, chunkId) = runningChunks[i];
                        _runners[runnerId].getChunk(chunkId).invokeJobComplete();
                    }
                }
            }
        }
        private static AbsTweenRunner getOrCreateRunner(NRunnableTweener tweener)
        {
            var runnerId = tweener.createRunnerId();
            if (_runners.TryGetValue(runnerId, out var runner))
            {
                return runner;
            }
            runner = tweener.createRunner();
            runner.runnerId = runnerId;
            return _runners[runnerId] = runner;
        }
        private static void startRunnableTweener(NRunnableTweener tweener)
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
                tweener.forceComplete();
                tweener.invokeOnComplete();
            }
            else
            {
                AbsTweenRunner runner = getOrCreateRunner(tweener);
                runner.addTweener(tweener);
            }
        }

        internal static void run(NTweener tweener)
        {
            initalize();
            if (tweener is NRunnableTweener runnableTweener)
            {
                startRunnableTweener(runnableTweener);
            }
            else if (tweener is CombinedNTweener combinedTweener)
            {
                _inCombinedTweeners.Add(combinedTweener);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        internal static void schedule(NTweener tweener)
        {
            initalize();
            if (tweener is NRunnableTweener runnableTweener)
            {
                _inQueueRunnableTweeners.Add(runnableTweener);
            }
            else if (tweener is CombinedNTweener combinedTweener)
            {
                _inCombinedTweeners.Add(combinedTweener);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal static void cancelFromTweener(NRunnableTweener tweener)
        {
            if (_isInitialized)
            {
                getOrCreateRunner(tweener).getChunk(tweener.chunkIndex.chunkId).cancelTween(tweener.chunkIndex.maskIndex);
            }
        }
        internal static void cancelFromControlledTweener(AbsCancelControlKey controlKey)
        {
            if (_isInitialized)
            {
                _cancelControlManager.cancel(controlKey);
            }
        }
        internal static void addCancelControlledTweener(NTweener tweener)
        {
            if (_isInitialized)
            {
                _cancelControlManager.addControlledTweener(tweener);
            }
        }
        internal static void removeControlledTweener(NTweener tweener)
        {
            if (_isInitialized)
            {
                _cancelControlManager.removeControlledTweener(tweener);
            }
        }
    }
}