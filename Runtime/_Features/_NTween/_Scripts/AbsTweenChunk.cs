using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal struct TweenStaticManager
    {
        public readonly static SharedStatic<float> currentTimeInJob = SharedStatic<float>.GetOrCreate<TweenStaticManager>(0);
        public static float currentTime;
    }
    internal abstract class AbsTweenChunk
    {
        private static int[] _defaultEmptyIndices;

        [StartupMethod]
        static void initialize()
        {
            _chunkIdOrder = 1;
            ChunkCount = 0;
            _defaultEmptyIndices = new int[ChunkSize];
            for (int i = 0; i < ChunkSize; ++i)
            {
                _defaultEmptyIndices[i] = ChunkSize - i - 1;
            }
        }
        private static uint _chunkIdOrder;
        public static int ChunkCount { get; private set; }
        public const int ChunkSize = 256;
        public AbsTweenChunk()
        {
            chunkId = _chunkIdOrder++;
            ChunkCount++;
            mask = new NativeArray<byte>(ChunkSize, Allocator.Persistent);
            _emptyIndices = new List<int>(_defaultEmptyIndices);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void dispose()
        {
            mask.Dispose();
            ChunkCount--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFull() => _emptyIndices.Count == 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isEmpty() => _emptyIndices.Count == ChunkSize;

        public readonly uint chunkId;
        public float lastAccessTime;
        public Action onChunkBecomeNotFull;
        protected NativeArray<byte> mask;

        protected List<int> _emptyIndices;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int getNextMaskIndex()
        {
            return _emptyIndices.takeAndRemoveLast();
        }

        public abstract void addTweener(NTweener tweener);
        public abstract JobHandle runJob();
        public abstract void invokeJobComplete();
        public abstract void cancelTween(int maskIndex);
    }
    internal abstract class AbsTweenChunk<TNTweener, TJob> : AbsTweenChunk
        where TNTweener : NTweener
        where TJob : struct
    {
        public AbsTweenChunk() : base()
        {
            tweeners = new TNTweener[ChunkSize];
        }

        public TNTweener[] tweeners;

        protected void removeAt(int maskIndex)
        {
            var isFull = this.isFull();
            mask.setBit0(maskIndex);
            _emptyIndices.Add(maskIndex);
            if (isEmpty())
            {
                lastAccessTime = TweenStaticManager.currentTime;
            }
            tweeners[maskIndex] = null;
            if (isFull && !this.isFull())
            {
                onChunkBecomeNotFull.Invoke();
            }
        }

        public sealed override void addTweener(NTweener inTweener)
        {
            var tweener = inTweener as TNTweener;

#if UNITY_EDITOR
            if (tweener == null)
            {
                throw new Exception($"Not match type: {inTweener.GetType()} vs {typeof(TNTweener)}");
            }
#endif

            var maskIndex = getNextMaskIndex();
            tweeners[maskIndex] = tweener;
            mask.setBit1(maskIndex);
            lastAccessTime = -1;
            tweener.chunkIndex = new ChunkIndex(tweener.tweenType, chunkId, (ushort)maskIndex);
            onAddNewTweener(tweener);
        }
        public override void invokeJobComplete()
        {
            for (int i = 0; i < ChunkSize; ++i)
            {
                var tweener = tweeners[i];
                if (tweener != null && TweenStaticManager.currentTime > tweener.startTime)
                {
                    if (tweener.Status == RunState.None)
                    {
                        tweener.invokeOnStart();
                    }
                    onTweenerUpdated(i);
                    tweener.invokeOnUpdate();
                    if (TweenStaticManager.currentTime >= tweener.startTime + tweener.duration)
                    {
                        removeAt(i);
                        onTweenerCompleted(i);
                        tweener.invokeOnComplete();
                    }
                }
            }
        }
        public override void cancelTween(int maskIndex)
        {
            if (tweeners[maskIndex] != null)
            {
                removeAt(maskIndex);
            }
        }
        public override void dispose()
        {
            base.dispose();
            tweeners = null;
        }

        protected bool _hasJob;
        protected TJob _job;
        public sealed override JobHandle runJob()
        {
            if (!_hasJob)
            {
                _job = createNewJob();
                _hasJob = true;
            }
            return onScheduleJob();
        }

        protected virtual void onTweenerUpdated(int maskIndex)
        {
            
        }
        protected virtual void onTweenerCompleted(int maskIndex)
        {

        }

        protected abstract void onAddNewTweener(TNTweener tweener);
        protected abstract TJob createNewJob();
        protected abstract JobHandle onScheduleJob();
    }
}