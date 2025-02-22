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
        private readonly SimpleDictionary<ushort, TweenChunk> _chunks = new(1);
        private readonly NList<TweenChunk> _notFullChunks = new();

        public int ChunkCount => _chunks.Count;

        public sealed override void addTweener(NRunnableTweener tweener)
        {
            TweenChunk nextChunk;
            int lastIndex = _notFullChunks.Count - 1;
            if (lastIndex < 0)
            {
                nextChunk = NUtils.createInstance<TChunk>();
                nextChunk.onChunkBecomeNotFull = _notFullChunks.Add;
                _chunks.Add(nextChunk.chunkId, nextChunk);
                _notFullChunks.Add(nextChunk);
                lastIndex = 0;
            }
            else
            {
                nextChunk = _notFullChunks.getAtWithoutChecks(lastIndex);
            }

            nextChunk.addTweener(tweener);
            if (nextChunk.isFull())
            {
                _notFullChunks.removeAtWithoutChecks(lastIndex);
            }
        }
        public sealed override void runTweenJob(ref NNativeListFixedSize<JobHandle> jobHandles, ref NNativeListFixedSize<(ushort runnerId, ushort chunkId)> runningChunks)
        {
            int chunksCount = _chunks.Count;
            using var unusedchunkIds = NPUArray<ushort>.get();
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
                    _chunks.takeAndRemove(chunkId).dispose();
                    chunksCount--;
                }

                for (int i = _notFullChunks.Count - 1; i >= 0; i--)
                {
                    if (_notFullChunks.getAtWithoutChecks(i).isDisposed())
                    {
                        _notFullChunks.removeAtSwapBackWithoutChecks(i);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override TweenChunk getChunk(ushort chunkId)
        {
            return _chunks[chunkId];
        }
        public sealed override void dispose()
        {
            foreach ((_, var chunk) in _chunks)
            {
                chunk.dispose();
            }
            _chunks.Clear();
            _notFullChunks.Clear();
        }
    }
}