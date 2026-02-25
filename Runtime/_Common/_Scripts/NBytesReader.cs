using System;

namespace Nextension
{
    public ref struct NBytesReader
    {
        private readonly ReadOnlySpan<byte> content;
        private int offset;

        public NBytesReader(NBytesRef nBytesRef)
        {
            this.content = nBytesRef.content;
            offset = 0;
        }

        public NBytesReader(ReadOnlySpan<byte> content)
        {
            this.content = content;
            offset = 0;
        }

        public T read<T>() where T : unmanaged
        {
            return NConverter.fromBytes<T>(content, ref offset);
        }
        public NBytesRef readNBytesRef()
        {
            return NBytesUtils.readNextNBytes(content, ref offset);
        }

        public bool isEnd()
        {
            return offset >= content.Length;
        }
    }
}