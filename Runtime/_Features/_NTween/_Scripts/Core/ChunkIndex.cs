using System;

namespace Nextension.Tween
{
    internal readonly struct ChunkIndex : IComparable<ChunkIndex>
    {
        public readonly uint chunkId => (uint)chunkH << 16 | chunkL;

        private readonly ushort chunkH;
        private readonly ushort chunkL;
        public readonly ushort maskIndex;

        public ChunkIndex(uint chunkId, ushort maskIndex)
        {
            this.chunkL = (ushort)(chunkId & 0xffff);
            this.chunkH = (ushort)(chunkId >> 16);
            this.maskIndex = maskIndex;
        }

        public int CompareTo(ChunkIndex other)
        {
            if (chunkH != other.chunkH)
            {
                return chunkH.CompareTo(other.chunkH);
            }
            if (chunkL != other.chunkL)
            {
                return chunkL.CompareTo(other.chunkL);
            }
            return maskIndex.CompareTo(other.maskIndex);
        }
    }
}
