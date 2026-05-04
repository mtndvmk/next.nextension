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
            buffer.AddRange(NConverter.getBytes(val).AsSpan());
        }
        public void write(ReadOnlySpan<byte> bytes)
        {
            buffer ??= new NList<byte>();
            buffer.InsertRangeWithoutChecks(buffer.Count, bytes);
        }
        public void writeUTF8(ReadOnlySpan<char> chars)
        {
            var byteCount = chars.Length;
            buffer.ensureCapacity(buffer.Count + byteCount);
            var span = new Span<byte>(buffer.i_Items, buffer.Count, byteCount);
            System.Text.Encoding.UTF8.GetBytes(chars, span);
            buffer.i_Count += byteCount;
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