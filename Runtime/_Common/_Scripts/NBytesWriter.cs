using System;

namespace Nextension
{
    public struct NBytesWriter
    {
        private NList<byte> buffer;
        public readonly bool IsCreated => buffer != null;

        public readonly ReadOnlySpan<byte> Buffer => buffer.AsSpan();

        public void write<T>(T val) where T : unmanaged
        {
            buffer ??= new NList<byte>();
            buffer.AddRange(NConverter.getBytes(val));
        }
        public void write(ReadOnlySpan<byte> bytes)
        {
            buffer ??= new NList<byte>();
            buffer.InsertRangeWithoutChecks(buffer.Count, bytes);
        }
        public NBytesRef asNBytesRef()
        {
            if (!IsCreated)
            {
                throw new InvalidOperationException("NBytesWriter is not created");
            }
            return new NBytesRef(buffer.AsSpan());
        }
    }
}