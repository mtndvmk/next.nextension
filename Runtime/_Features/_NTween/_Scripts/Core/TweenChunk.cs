using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.Tween
{
    internal abstract class TweenChunk
    {
        #region Const & Static
        public const ushort CHUNK_SIZE = 256;
        public const float NOT_ACCESS_TIME_LIMIT = 10; // in seconds

        static TweenChunk()
        {
            _chunkIdOrder = 0;
            ChunkCount = 0;
            TweenStaticManager.runningTweenerCount = 0;
            _defaultEmptyIndices = new short[CHUNK_SIZE];
            var chunkSizeMinusOne = CHUNK_SIZE - 1;
            for (short i = 0; i < CHUNK_SIZE; ++i)
            {
                _defaultEmptyIndices[i] = (short)(chunkSizeMinusOne - i);
            }
        }

        private static short[] _defaultEmptyIndices;
        private static ushort _chunkIdOrder;

        public static short ChunkCount { get; private set; }

        #endregion

        protected NativeArray<byte> _mask;
        protected NNativeListFixedSize<short> _emptyIndices;
        protected float _lastEmptyTime;

        public readonly ushort chunkId;
        public Action<TweenChunk> onChunkBecomeNotFull;

        public unsafe TweenChunk()
        {
            chunkId = ++_chunkIdOrder;
            ChunkCount++;
            _mask = new NativeArray<byte>(CHUNK_SIZE, Allocator.Persistent);
            _emptyIndices = new(_defaultEmptyIndices, Allocator.Persistent);
        }
        public virtual void dispose()
        {
            _emptyIndices.Dispose();
            _mask.Dispose();
            ChunkCount--;
        }
        public bool isFull() => _emptyIndices.Count == 0;
        public bool isEmpty() => _emptyIndices.Count == CHUNK_SIZE;
        public bool isDisposed() => !_mask.IsCreated;
        public bool isUnused()
        {
            if (_lastEmptyTime == -1 || TweenStaticManager.currentTime - _lastEmptyTime < NOT_ACCESS_TIME_LIMIT)
            {
                return false;
            }
            return true;
        }

        protected int getNextMaskIndex()
        {
            return _emptyIndices.TakeAndRemoveLast();
        }

        public abstract void addTweener(NRunnableTweener tweener);
        public abstract JobHandle runJob();
        public abstract void invokeJobComplete();
        public abstract void cancelTween(int maskIndex);
    }

    internal abstract class GenericTweenChunk<TNTweener, TJob, TJobData> : TweenChunk
        where TNTweener : GenericNRunnableTweener<TJobData>
        where TJob : struct
        where TJobData : struct
    {
        protected bool _hasJob;
        protected TJob _job;
        protected readonly TNTweener[] _tweeners;
        protected NativeArray<TJobData> _jobDataNativeArr;

        public GenericTweenChunk() : base()
        {
            _tweeners = new TNTweener[CHUNK_SIZE];
            _jobDataNativeArr = new NativeArray<TJobData>(CHUNK_SIZE, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        private void removeAt(int maskIndex)
        {
#if UNITY_EDITOR
            if (_tweeners[maskIndex] == null)
            {
                Debug.LogError("Remove empty maskIndex?");
                return;
            }
#endif
            var isFull = this.isFull();
            _mask.setBit0(maskIndex);
            _emptyIndices.AddNoResize((short)maskIndex);
            if (isEmpty())
            {
                _lastEmptyTime = TweenStaticManager.currentTime;
            }
            _tweeners[maskIndex] = null;
            TweenStaticManager.runningTweenerCount--;
            if (isFull && !this.isFull())
            {
                onChunkBecomeNotFull.Invoke(this);
            }
        }

        public sealed override void addTweener(NRunnableTweener inTweener)
        {
            var tweener = inTweener as TNTweener;

#if UNITY_EDITOR
            if (tweener == null)
            {
                throw new Exception($"Not match type: {inTweener.GetType()} vs {typeof(TNTweener)}");
            }
#endif

            var maskIndex = getNextMaskIndex();
            _tweeners[maskIndex] = tweener;
            TweenStaticManager.runningTweenerCount++;
            _mask.setBit1(maskIndex);
            _lastEmptyTime = -1;
            tweener.chunkIndex = new ChunkIndex(chunkId, (ushort)maskIndex);
            _jobDataNativeArr[tweener.chunkIndex.maskIndex] = tweener.getJobData();
            onAddNewTweener(tweener);
        }
        public sealed override void invokeJobComplete()
        {
            for (int i = 0; i < CHUNK_SIZE; ++i)
            {
                var tweener = _tweeners[i];
                if (tweener == null)
                {
                    continue;
                }
                var startTime = tweener.startTime;
                var currentTime = tweener.updateMode == NTweener.UpdateMode.ScaleTime ? TweenStaticManager.currentTime : TweenStaticManager.currentUnscaledTime;
                if (currentTime > startTime)
                {
                    tweener.Time = currentTime - startTime;
                    var endTime = startTime + tweener.duration;
                    if (tweener.Status == RunState.None)
                    {
                        tweener.invokeOnStart();
                    }
                    onTweenerUpdated(i);
                    tweener.invokeOnUpdate();

                    if (tweener == _tweeners[i] && currentTime >= endTime)
                    {
                        removeAt(i);
                        tweener.invokeOnComplete();
                    }
                }
            }
        }
        public sealed override void cancelTween(int maskIndex)
        {
            if (_tweeners[maskIndex] != null)
            {
                removeAt(maskIndex);
            }
        }
        public sealed override JobHandle runJob()
        {
            if (!_hasJob)
            {
                _job = createNewJob();
                _hasJob = true;
            }
            return onScheduleJob();
        }

        public override void dispose()
        {
            base.dispose();
            _jobDataNativeArr.Dispose();
        }

        protected virtual void onTweenerUpdated(int maskIndex) { }
        protected virtual void onAddNewTweener(TNTweener tweener) { }

        protected abstract TJob createNewJob();
        protected abstract JobHandle onScheduleJob();
    }
}