using System;

namespace Nextension.Tween
{
    internal struct ChunkIndex : IComparable<ChunkIndex>
    {
        public readonly TweenType type;
        public readonly uint chunkId;
        public readonly ushort maskIndex;

        public ChunkIndex(TweenType type, uint chunkId, ushort maskIndex)
        {
            this.type = type;
            this.chunkId = chunkId;
            this.maskIndex = maskIndex;
        }

        public int CompareTo(ChunkIndex other)
        {
            if (type != other.type) 
            {
                return type.CompareTo(other.type);
            }
            if (chunkId != other.chunkId)
            {
                return chunkId.CompareTo(other.chunkId);
            }
            return maskIndex.CompareTo(other.maskIndex);
        }
    }
}
