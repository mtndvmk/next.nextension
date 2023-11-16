using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace Nextension.Tween
{
    internal abstract class AbsTweenRunner
    {
        protected const int MIN_COUNT_OF_CHUNK = 1;

        public uint runnerId;

        public abstract void addTweener(NRunnableTweener inTweener);
        public abstract void runTweenJob(NativeList<JobHandle> jobHandles, NativeList<(uint runnerId, uint chunkId)> runningChunks);
        public abstract void dispose();
        public abstract TweenChunk getChunk(uint chunkId);
    }
    internal sealed class TweenRunner<TChunk> : AbsTweenRunner where TChunk : TweenChunk, new()
    {
        private readonly Dictionary<uint, TChunk> _chunks = new(1);
        private readonly List<TChunk> _notFullChunks = new(1);

        public int ChunkCount => _chunks.Count;

        public sealed override void addTweener(NRunnableTweener tweener)
        {
            TChunk chunk;
            int lastIndex = _notFullChunks.Count - 1;
            if (lastIndex < 0)
            {
                chunk = new();
                chunk.onChunkBecomeNotFull = () => _notFullChunks.Add(chunk);
                _chunks.Add(chunk.chunkId, chunk);
                _notFullChunks.Add(chunk);
                lastIndex = 0;
            }
            else
            {
                chunk = _notFullChunks[lastIndex];
            }

            chunk.addTweener(tweener);
            if (chunk.isFull())
            {
                _notFullChunks.RemoveAt(lastIndex);
            }
        }
        public sealed override void runTweenJob(NativeList<JobHandle> jobHandles, NativeList<(uint runnerId, uint chunkId)> runningChunks)
        {
            int chunksCount = _chunks.Count;
            NativeArray<uint> chunkIds = new(chunksCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int count = 0;

            foreach (var k in _chunks.Keys)
            {
                chunkIds[count++] = k;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                var chunkId = chunkIds[i];
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
        public sealed override TweenChunk getChunk(uint chunkId)
        {
            return _chunks[chunkId];
        }
        public sealed override void dispose()
        {
            foreach (var chunk in _chunks.Values)
            {
                chunk.dispose();
            }
            _chunks.Clear();
            _notFullChunks.Clear();
        }
    }
}