using System;
using System.IO;

namespace Nextension
{
    public sealed class PooledMemoryStream : AbsPoolable, IDisposable
    {
        public const int DEFAULT_CAPACITY_THRESHOLD = 1 << 20;

        private static int _capacityThreshold = DEFAULT_CAPACITY_THRESHOLD;

        public static int CapacityThreshold
        {
            get => _capacityThreshold;
            set => _capacityThreshold = value < 0 ? 0 : value;
        }

        private static NPool<MemoryStream> _valuePool => NPool<MemoryStream>.Shared;

        private static NPool<PooledMemoryStream> _pPool => NPool<PooledMemoryStream>.Shared;

        public static PooledMemoryStream get()
        {
            var p = _pPool.Rent().value;
            p.Stream = _valuePool.Rent().value;
            return p;
        }
        public static PooledMemoryStream getWithoutTracking() => get();

        public MemoryStream Stream { get; private set; }

        public int Length => (int)Stream.Length;
        public int Capacity { get => Stream.Capacity; set => Stream.Capacity = value; }
        public long Position { get => Stream.Position; set => Stream.Position = value; }

        private PooledMemoryStream() { }

        public void Reset()
        {
            Stream.Position = 0;
            Stream.SetLength(0);
        }
        public void SetLength(int length) => Stream.SetLength(length);
        public void EnsureCapacity(int capacity)
        {
            if (Stream.Capacity < capacity) Stream.Capacity = capacity;
        }

        public byte[] GetBuffer() => Stream.GetBuffer();
        public byte[] ToArray() => Stream.ToArray();
        public Span<byte> AsSpan() => Stream.GetBuffer().AsSpan(0, (int)Stream.Length);
        public Span<byte> AsSpan(int start, int length) => Stream.GetBuffer().AsSpan(start, length);
        public Memory<byte> AsMemory() => Stream.GetBuffer().AsMemory(0, (int)Stream.Length);

        public void Write(ReadOnlySpan<byte> buffer) => Stream.Write(buffer);
        public void WriteByte(byte value) => Stream.WriteByte(value);
        public int Read(Span<byte> buffer) => Stream.Read(buffer);
        public void CopyTo(Stream destination) => Stream.CopyTo(destination);

        protected override void onDespawn()
        {
            base.onDespawn();

            var stream = Stream;
            var isOversized = stream.Capacity > _capacityThreshold;
            Stream = null;

            if (isOversized)
            {
                stream.Dispose();
                return;
            }

            stream.Position = 0;
            stream.SetLength(0);
            _valuePool.Return(stream);
        }

        public void Dispose()
        {
            if (IsRented)
            {
                _pPool.Return(this);
            }
        }

        public void SafeReturn(long returnToken)
        {
            if (IsRented)
            {
                _pPool.SafeReturn(this, returnToken);
            }
        }

        public static implicit operator MemoryStream(PooledMemoryStream poolable) => poolable.Stream;
    }
}
