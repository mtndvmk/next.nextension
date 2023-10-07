using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.Tween
{
    internal abstract class TweenChunk
    {
        #region Const & Static
        private static short[] _defaultEmptyIndices;

        [StartupMethod]
        static void initialize()
        {
            _chunkIdOrder = 0;
            ChunkCount = 0;
            TweenStaticManager.runningTweenerCount = 0;
            _defaultEmptyIndices = new short[ChunkSize];
            var chunkSizeMinusOne = ChunkSize - 1;
            for (short i = 0; i < ChunkSize; ++i)
            {
                _defaultEmptyIndices[i] = (short)(chunkSizeMinusOne - i);
            }
        }
        private static uint _chunkIdOrder;
        public static short ChunkCount { get; private set; }
        public const ushort ChunkSize = 256;
        public const float NotAccessTimeLimit = 10;
        #endregion

        protected float _lastEmptyTime;    
        protected NativeArray<byte> _mask;
        protected readonly NativeList<short> _emptyIndices;
        
        public readonly uint chunkId;
        public Action onChunkBecomeNotFull;

        public unsafe TweenChunk()
        {
            chunkId = ++_chunkIdOrder;
            ChunkCount++;
            _mask = new NativeArray<byte>(ChunkSize, Allocator.Persistent);
            _emptyIndices = new(ChunkSize, AllocatorManager.Persistent);
            fixed (void* ptr = &_defaultEmptyIndices[0])
            {
                _emptyIndices.AddRangeNoResize(ptr, ChunkCount);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void dispose()
        {
            _emptyIndices.Dispose();
            _mask.Dispose();
            ChunkCount--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFull() => _emptyIndices.Length == 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isEmpty() => _emptyIndices.Length == ChunkSize;

        public bool isUnused()
        {
            if (_lastEmptyTime == -1 ||
               TweenStaticManager.currentTime - _lastEmptyTime < NotAccessTimeLimit)
            {
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int getNextMaskIndex()
        {
            return _emptyIndices.takeAndRemoveLast();
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
        protected readonly TNTweener[] _tweeners;
        protected bool _hasJob;
        protected TJob _job;
        protected NativeArray<TJobData> _jobDataNativeArr;

        public GenericTweenChunk() : base()
        {
            _tweeners = new TNTweener[ChunkSize];
            _jobDataNativeArr = new NativeArray<TJobData>(ChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        private void removeAt(int maskIndex)
        {
#if UNITY_EDITOR
            if (_tweeners[maskIndex] == null)
            {
                Debug.LogError("Remove empty maskIndex?");
            }
#endif
            var isFull = this.isFull();
            _mask.setBit0(maskIndex);
            _emptyIndices.Add((short)maskIndex);
            if (isEmpty())
            {
                _lastEmptyTime = TweenStaticManager.currentTime;
            }
            _tweeners[maskIndex] = null;
            TweenStaticManager.runningTweenerCount--;
            if (isFull && !this.isFull())
            {
                onChunkBecomeNotFull.Invoke();
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
            for (int i = 0; i < ChunkSize; ++i)
            {
                var tweener = _tweeners[i];
                if (tweener == null)
                {
                    continue;
                }
                var startTime = tweener.startTime;
                if (TweenStaticManager.currentTime > startTime)
                {
                    var endTime = startTime + tweener.duration;
                    if (tweener.Status == RunState.None)
                    {
                        tweener.invokeOnStart();
                    }
                    onTweenerUpdated(i);
                    tweener.invokeOnUpdate();
                    if (TweenStaticManager.currentTime >= endTime)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void onTweenerUpdated(int maskIndex) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void onAddNewTweener(TNTweener tweener) { }

        protected abstract TJob createNewJob();
        protected abstract JobHandle onScheduleJob();
    }
}