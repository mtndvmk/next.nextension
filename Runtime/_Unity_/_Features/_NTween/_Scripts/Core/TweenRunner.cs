using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal class TweenRunner
    {
        protected const int MIN_COUNT_OF_CHUNK = 1;
        protected readonly SimpleDictionary<ushort, TweenChunk> _chunks = new SimpleDictionary<ushort, TweenChunk>(1);
        protected readonly NList<TweenChunk> _notFullChunks = new NList<TweenChunk>();
        protected Action<TweenChunk> _addToNotFullChunk;

        public readonly ushort runnerId;

        internal TweenRunner(ushort runnerId)
        {
            this.runnerId = runnerId;
        }

        public void addTweener(NTweener tweener)
        {
            TweenChunk nextChunk;
            int lastIndex = _notFullChunks.Count - 1;
            if (lastIndex < 0)
            {
                nextChunk = TweenRunnerUtil.createChunk(runnerId);
                _addToNotFullChunk ??= _notFullChunks.Add;
                nextChunk.onChunkBecomeNotFull = _addToNotFullChunk;
                _chunks.Add(nextChunk.chunkId, nextChunk);
                _notFullChunks.Add(nextChunk);
                lastIndex = 0;
            }
            else
            {
                nextChunk = _notFullChunks.GetAtWithoutChecks(lastIndex);
            }

            nextChunk.addTweener(tweener);
            if (nextChunk.isFull())
            {
                _notFullChunks.RemoveAtWithoutChecks(lastIndex);
            }
        }

        public void runTweenJob(ref NNativeListFixedSize<JobHandle> jobHandles, ref NNativeListFixedSize<(ushort runnerId, ushort chunkId)> runningChunks)
        {
            int chunksCount = _chunks.Count;
            using var unusedchunkIds = PUList<ushort>.get();
            foreach (var (chunkId, chunk) in _chunks)
            {
                if (chunk.isUnused())
                {
                    unusedchunkIds.Add(chunkId);
                }
                else
                {
                    var jobHandle = chunk.runJob();
                    jobHandles.AddNoResize(jobHandle);
                    runningChunks.AddNoResize((runnerId, chunkId));
                }
            }

            if (chunksCount > MIN_COUNT_OF_CHUNK && unusedchunkIds.Count > 0)
            {
                foreach (var chunkId in unusedchunkIds)
                {
                    if (chunksCount <= MIN_COUNT_OF_CHUNK) break;
                    _chunks.TakeAndRemove(chunkId).dispose();
                    chunksCount--;
                }

                for (int i = _notFullChunks.Count - 1; i >= 0; i--)
                {
                    if (_notFullChunks.GetAtWithoutChecks(i).isDisposed())
                    {
                        _notFullChunks.RemoveAtSwapBackWithoutChecks(i);
                    }
                }
            }
        }

        public TweenChunk getChunk(ushort chunkId)
        {
            return _chunks[chunkId];
        }

        public void dispose()
        {
            foreach ((_, var chunk) in _chunks)
            {
                chunk.dispose();
            }
            _chunks.Clear();
            _notFullChunks.Clear();
        }
    }
    internal static class TweenRunnerUtil
    {
        private static List<Type> _typeTable = new();
        internal static ushort getNext(Type type)
        {
            _typeTable.Add(type);
            return (ushort)_typeTable.Count;
        }
        internal static Type getType(ushort id)
        {
            return _typeTable[id - 1];
        }
        internal static TweenChunk createChunk(ushort id)
        {
            return (TweenChunk)NUtils.createInstance(getType(id));
        }
    }

    internal static class TweenRunnerId<TChunk> where TChunk : TweenChunk
    {
        public readonly static ushort id = TweenRunnerUtil.getNext(typeof(TChunk));
    }
}