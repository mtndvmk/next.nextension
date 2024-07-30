using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.Tween
{
    internal static class NTweenManager
    {
        private static NArray<NRunnableTweener> _queuedRunnableTweeners;
        private static NArray<CombinedNTweener> _queuedCombinedTweeners;

        private static Dictionary<ushort, AbsTweenRunner> _runners;
        private static CancelControlManager _cancelControlManager;

        static NTweenManager()
        {
            _queuedRunnableTweeners = new();
            _queuedCombinedTweeners = new();
            _runners = new Dictionary<ushort, AbsTweenRunner>();
            _cancelControlManager = new CancelControlManager();

            NUpdater.onUpdateEvent.add(update);
        }
#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void onApplicationQuit()
        {
            foreach (var runner in _runners.Values)
            {
                runner.dispose();
            }
            _runners.Clear();
            _cancelControlManager.clear();
            _queuedRunnableTweeners.Clear();
            _queuedCombinedTweeners.Clear();
        }
#endif
        static void update()
        {
            var currentTime = Time.time;
            TweenStaticManager.currentTime = TweenStaticManager.currentTimeInJob.Data = currentTime;

            _cancelControlManager.cancelInvalid();

            int combinedCount = _queuedCombinedTweeners.Count;
            if (combinedCount > 0)
            {
                var inCombinedSpan = _queuedCombinedTweeners.asSpan();
                for (int i = combinedCount - 1; i >= 0; i--)
                {
                    var tweener = inCombinedSpan[i];
                    if (tweener.startTime <= currentTime)
                    {
                        tweener.invokeOnStart();
                        _queuedCombinedTweeners.removeAtSwapBackWithoutChecks(i);
                    }
                }
            }

            int runnableCount = _queuedRunnableTweeners.Count;
            if (runnableCount > 0)
            {
                var runnableSpan = _queuedRunnableTweeners.asSpan();
                for (int i = 0; i < runnableCount; ++i)
                {
                    startRunnableTweener(runnableSpan[i]);
                }
                _queuedRunnableTweeners.Clear();
            }

            if (TweenStaticManager.runningTweenerCount > 0)
            {
                NNativeListFixedSize<JobHandle> jobHandles = new(TweenChunk.ChunkCount);
                NNativeListFixedSize<(ushort runnerId, ushort chunkId)> runningChunks = new(TweenChunk.ChunkCount);

                foreach (var runner in _runners.Values)
                {
                    runner.runTweenJob(ref jobHandles, ref runningChunks);
                }

                int runningChunkCount = runningChunks.Count;
                if (runningChunkCount > 0)
                {
                    JobHandle.CombineDependencies(jobHandles.Slice()).Complete();
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
            var runnerId = tweener.getRunnerId();
            if (!_runners.TryGetValue(runnerId, out var runner))
            {
                runner = tweener.createRunner();
                runner.runnerId = runnerId;
                _runners[runnerId] = runner;
            }
            return runner;
        }
        private static void startRunnableTweener(NRunnableTweener tweener)
        {
            if (tweener.chunkIndex.chunkId != 0 || tweener.Status != RunState.None)
            {
                return;
            }
            try
            {
                if (tweener.isCanceledFromFunc())
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
        internal static void run(NRunnableTweener runnableTweener)
        {
            startRunnableTweener(runnableTweener);
        }
        internal static void run(CombinedNTweener combinedTweener)
        {
            _queuedCombinedTweeners.Add(combinedTweener);
        }
        internal static void schedule(NRunnableTweener runnableTweener)
        {
            _queuedRunnableTweeners.Add(runnableTweener);
        }
        internal static void schedule(CombinedNTweener combinedTweener)
        {
            _queuedCombinedTweeners.Add(combinedTweener);
        }

        internal static void cancelFromTweener(NRunnableTweener tweener)
        {
            if (_runners.TryGetValue(tweener.getRunnerId(), out var runner))
            {
                runner.getChunk(tweener.chunkIndex.chunkId).cancelTween(tweener.chunkIndex.maskIndex);
            }
        }
        internal static void cancelFromUintControlKey(uint uintKey)
        {
            _cancelControlManager.cancel(CancelControlKey.getLongKey(uintKey));
        }
        internal static void cancelFromObjectControlKey(UnityEngine.Object objectKey)
        {
            _cancelControlManager.cancel(CancelControlKey.getLongKey(objectKey));
        }
        internal static bool isInvalidKey(CancelControlKey controlKey)
        {
            _cancelControlManager.isInvalid(controlKey);
            return false;
        }
        internal static CancelControlKey createKey(UnityEngine.Object objectKey)
        {
            return _cancelControlManager.createKey(objectKey);
        }
        internal static CancelControlKey createKey(uint uintKey)
        {
            return _cancelControlManager.createKey(uintKey);
        }
        internal static void addCancelControlledTweener(NTweener tweener)
        {
            _cancelControlManager.addTweener(tweener);
        }
        internal static void removeControlledTweener(NTweener tweener)
        {
            _cancelControlManager.removeTweener(tweener);
        }
    }
}