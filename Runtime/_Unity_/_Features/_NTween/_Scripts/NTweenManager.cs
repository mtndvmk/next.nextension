using System;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.Tween
{
    internal static class NTweenManager
    {
        private static NList<NTweener> _queuedRunnableTweeners;
        private static NList<NTweener> _completedTweeners;

        private static SimpleDictionary<ushort, TweenRunner> _runners;
        private static CancelControlManager _cancelControlManager;

        static NTweenManager()
        {
            _queuedRunnableTweeners = new NList<NTweener>();
            _completedTweeners = new NList<NTweener>();
            _runners = new SimpleDictionary<ushort, TweenRunner>();
            _cancelControlManager = new CancelControlManager();

            NUpdater.onUpdateEvent.add(update);
        }
#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void onApplicationQuit()
        {
            foreach ((_, var runner) in _runners)
            {
                runner.dispose();
            }
            _runners.Clear();
            _cancelControlManager.clear();
            _queuedRunnableTweeners.Clear();
        }
#endif
        static void update()
        {
            var currentTime = Time.time;
            TweenStaticManager.currentTime = TweenStaticManager.currentTimeInJob.Data = currentTime;

            var currentUnscaledTime = Time.unscaledTime;
            TweenStaticManager.currentUnscaledTime = TweenStaticManager.currentUnscaledTimeInJob.Data = currentUnscaledTime;

            _cancelControlManager.cancelInvalid();

            int runnableCount = _queuedRunnableTweeners.Count;
            if (runnableCount > 0)
            {
                for (int i = 0; i < runnableCount; ++i)
                {
                    startRunnableTweener(_queuedRunnableTweeners.GetAtWithoutChecks(i));
                }
                _queuedRunnableTweeners.Clear();
            }

            if (_completedTweeners.Count > 0)
            {
                foreach (var tweener in _completedTweeners)
                {
                    tweener.invokeOnStart();
                    tweener.forceComplete();
                }
                _completedTweeners.Clear();
            }

            if (TweenStaticManager.runningTweenerCount > 0)
            {
                NNativeListFixedSize<JobHandle> jobHandles = new NNativeListFixedSize<JobHandle>(TweenChunk.ChunkCount);
                NNativeListFixedSize<(ushort runnerId, ushort chunkId)> runningChunks = new NNativeListFixedSize<(ushort runnerId, ushort chunkId)>(TweenChunk.ChunkCount);

                foreach ((_, var runner) in _runners)
                {
                    runner.runTweenJob(ref jobHandles, ref runningChunks);
                }

                int runningChunkCount = runningChunks.Count;
                if (runningChunkCount > 0)
                {
                    JobHandle.CombineDependencies(jobHandles.Slice()).Complete();
                    for (int i = 0; i < runningChunkCount; ++i)
                    {
                        var (runnerId, chunkId) = runningChunks.getWithoutCheck(i);
                        _runners[runnerId].getChunk(chunkId).invokeJobComplete();
                    }
                }

                jobHandles.Dispose();
                runningChunks.Dispose();
            }
        }
        private static TweenRunner getOrCreateRunner(NTweener tweener)
        {
            var runnerId = tweener.getRunnerId();
            if (!_runners.TryGetValue(runnerId, out var runner))
            {
                runner = new TweenRunner(runnerId);
                _runners.Add(runnerId, runner);
            }
            return runner;
        }
        private static void startRunnableTweener(NTweener tweener)
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
                NDebug.LogException(e);
                return;
            }
            tweener.invokeBeforeStart();
            bool isCompletedTweener = tweener.duration <= 0;
            if (isCompletedTweener)
            {
                _completedTweeners.Add(tweener);
            }
            else
            {
                TweenRunner runner = getOrCreateRunner(tweener);
                runner.addTweener(tweener);
            }
        }
        internal static void run(NTweener tweener)
        {
            startRunnableTweener(tweener);
        }
        internal static void schedule(NTweener tweener)
        {
            _queuedRunnableTweeners.Add(tweener);
        }
        internal static void cancelFromTweener(NTweener tweener)
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
