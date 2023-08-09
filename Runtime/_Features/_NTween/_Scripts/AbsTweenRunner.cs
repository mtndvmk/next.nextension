using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Nextension.Tween
{
    internal abstract class AbsTweenRunner
    {
        protected const float CHUNK_NOT_ACCESS_LIFE_TIME = 10;
        protected const int MIN_COUNT_OF_CHUNK = 1;

        public abstract void addTweener(NTweener inTweener);
        public abstract void cancelTween(ChunkIndex chunkIndex);
        public abstract void runTweenJob(NativeList<JobHandle> jobHandles, List<AbsTweenChunk> runningChunks);
        public abstract void dispose();
    }
    internal abstract class AbsTweenRunner<TChunk> : AbsTweenRunner where TChunk : AbsTweenChunk, new()
    {
        private Dictionary<uint, TChunk> _chunks = new();
        private List<TChunk> _notFullChunks = new();

        public int ChunkCount => _chunks.Count;

        public sealed override void addTweener(NTweener tweener)
        {
            TChunk chunk;
            int lastIndex = _notFullChunks.Count - 1;
            if (lastIndex < 0)
            {
                chunk = NUtils.createInstance<TChunk>();
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
        public sealed override void cancelTween(ChunkIndex chunkIndex)
        {
            _chunks[chunkIndex.chunkId].cancelTween(chunkIndex.maskIndex);
        }
        public sealed override void runTweenJob(NativeList<JobHandle> jobHandles, List<AbsTweenChunk> runningChunks)
        {
            var chunks = _chunks.Values.ToArray();

            for (int i = chunks.Length - 1; i >= 0; i--)
            {
                var chunk = chunks[i];
                if (chunk.isEmpty())
                {
                    if (_chunks.Count > MIN_COUNT_OF_CHUNK)
                    {
                        if (isUnuseChunk(chunk))
                        {
                            chunk.dispose();
                            _chunks.Remove(chunk.chunkId);
                        }
                    }
                    continue;
                }
                var jobHandle = chunk.runJob();
                jobHandles.Add(jobHandle);
                runningChunks.Add(chunk);
            }
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
        private bool isUnuseChunk(AbsTweenChunk chunk)
        {
            if (chunk.lastAccessTime == -1)
            {
                return false;
            }
            if (TweenStaticManager.currentTime - chunk.lastAccessTime < CHUNK_NOT_ACCESS_LIFE_TIME)
            {
                return false;
            }
            return true;
        }
    }
}