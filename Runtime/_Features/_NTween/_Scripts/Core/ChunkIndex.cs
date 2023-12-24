using System;

namespace Nextension.Tween
{
    internal readonly struct ChunkIndex : IComparable<ChunkIndex>
    {
        public readonly ushort chunkId;
        public readonly ushort maskIndex;

        public ChunkIndex(ushort chunkId, ushort maskIndex)
        {
            this.chunkId = chunkId;
            this.maskIndex = maskIndex;
        }

        public int CompareTo(ChunkIndex other)
        {
            if (chunkId != other.chunkId)
            {
                return chunkId.CompareTo(other.chunkId);
            }
            return maskIndex.CompareTo(other.maskIndex);
        }
    }
}
