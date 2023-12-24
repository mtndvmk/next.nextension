using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsTweenRunner
    {
        public static ushort maxId = 0;
        protected const int MIN_COUNT_OF_CHUNK = 1;

        public ushort runnerId;

        public abstract void addTweener(NRunnableTweener inTweener);
        public abstract void runTweenJob(ref NNativeListFixedSize<JobHandle> jobHandles, ref NNativeListFixedSize<(ushort runnerId, ushort chunkId)> runningChunks);
        public abstract void dispose();
        public abstract TweenChunk getChunk(ushort chunkId);
    }

    internal static class TweenRunnerIdCache<TRunner> where TRunner : AbsTweenRunner
    {
        public readonly static ushort id = ++AbsTweenRunner.maxId;
    }

    internal sealed class TweenRunner<TChunk> : AbsTweenRunner where TChunk : TweenChunk
    {
        private readonly Dictionary<ushort, TweenChunk> _chunks = new(1);
        private readonly NArray<TweenChunk> _notFullChunks = new();
        private readonly Action<TweenChunk> onChunkBecomeNotFullFunc;

        public TweenRunner()
        {
            onChunkBecomeNotFullFunc = (chunk) =>
            {
                _notFullChunks.Add(chunk);
            };
        }

        public int ChunkCount => _chunks.Count;

        public sealed override void addTweener(NRunnableTweener tweener)
        {
            TweenChunk newChunk;
            int lastIndex = _notFullChunks.Count - 1;
            if (lastIndex < 0)
            {
                newChunk = NUtils.createInstance<TChunk>();     
                newChunk.onChunkBecomeNotFull = onChunkBecomeNotFullFunc;
                _chunks.Add(newChunk.chunkId, newChunk);
                _notFullChunks.Add(newChunk);
                lastIndex = 0;
            }
            else
            {
                newChunk = _notFullChunks.getWithoutChecks(lastIndex);
            }

            newChunk.addTweener(tweener);
            if (newChunk.isFull())
            {
                _notFullChunks.removeWithoutChecks(lastIndex);
            }
        }
        public sealed override void runTweenJob(ref NNativeListFixedSize<JobHandle> jobHandles, ref NNativeListFixedSize<(ushort runnerId, ushort chunkId)> runningChunks)
        {
            int chunksCount = _chunks.Count;
            using var chunkIds = _chunks.Keys.toNPArray();

            foreach (var chunkId in chunkIds.asSpan())
            {
                var chunk = _chunks[chunkId];
                if (chunk.isUnused())
                {
                    if (chunksCount > MIN_COUNT_OF_CHUNK)
                    {
                        chunk.dispose();
                        _chunks.Remove(chunkId);
                        chunksCount--;
                    }
                    continue;
                }
                var jobHandle = chunk.runJob();
                jobHandles.AddNoResize(jobHandle);
                runningChunks.AddNoResize((runnerId, chunkId));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override TweenChunk getChunk(ushort chunkId)
        {
            return _chunks[chunkId];
        }
        public sealed override void dispose()
        {
            using var chunkIds = _chunks.Keys.toNPArray();
            foreach (var chunkId in chunkIds.asSpan())
            {
                _chunks[chunkId].dispose();
            }
            _chunks.Clear();
            _notFullChunks.Clear();
        }
    }
}