using System;
using System.Buffers.Binary;
using System.Text;

namespace Nextension
{
    public ref struct NBytesWriter
    {
        private PNList<byte> _list;

        public readonly bool IsCreated => _list != null;

        public readonly ReadOnlySpan<byte> Buffer => _list.AsSpan();

        private void __ensureCapacity(int additionalBytes)
      {
            if (_list == null)
            {
                _list = PNList<byte>.get();
            }
            _list.Collection.EnsureCapacity(_list.Collection.i_Count + additionalBytes);
        }

        public void Dispose()
        {
            if (_list != null)
            {
                _list.Dispose();
                _list = null;
            }
        }

        public void write(byte val)
        {
            __ensureCapacity(1);
            _list.Collection.i_Items[_list.Collection.i_Count++] = val;
        }
        public void write(sbyte val)
        {
            __ensureCapacity(1);
            _list.Collection.i_Items[_list.Collection.i_Count++] = (byte)val;
        }
        public void write(bool val)
        {
            __ensureCapacity(1);
            _list.Collection.i_Items[_list.Collection.i_Count++] = val ? (byte)1 : (byte)0;
        }
        public void write(short val)
        {
            __ensureCapacity(2);
            BinaryPrimitives.WriteInt16LittleEndian(_list.AsSpan(_list.Collection.i_Count, 2), val);
            _list.Collection.i_Count += 2;
        }
        public void write(ushort val)
        {
            __ensureCapacity(2);
            BinaryPrimitives.WriteUInt16LittleEndian(_list.AsSpan(_list.Collection.i_Count, 2), val);
            _list.Collection.i_Count += 2;
        }
        public void write(int val)
        {
            __ensureCapacity(4);
            BinaryPrimitives.WriteInt32LittleEndian(_list.AsSpan(_list.Collection.i_Count, 4), val);
            _list.Collection.i_Count += 4;
        }
        public void write(uint val)
        {
            __ensureCapacity(4);
            BinaryPrimitives.WriteUInt32LittleEndian(_list.AsSpan(_list.Collection.i_Count, 4), val);
            _list.Collection.i_Count += 4;
        }
        public void write(long val)
        {
            __ensureCapacity(8);
            BinaryPrimitives.WriteInt64LittleEndian(_list.AsSpan(_list.Collection.i_Count, 8), val);
            _list.Collection.i_Count += 8;
        }
        public void write(ulong val)
        {
            __ensureCapacity(8);
            BinaryPrimitives.WriteUInt64LittleEndian(_list.AsSpan(_list.Collection.i_Count, 8), val);
            _list.Collection.i_Count += 8;
        }
        public void write(float val)
        {
            __ensureCapacity(4);
            BinaryPrimitives.WriteInt32LittleEndian(_list.AsSpan(_list.Collection.i_Count, 4), BitConverter.SingleToInt32Bits(val));
            _list.Collection.i_Count += 4;
        }
        public void write(double val)
        {
            __ensureCapacity(8);
            BinaryPrimitives.WriteInt64LittleEndian(_list.AsSpan(_list.Collection.i_Count, 8), BitConverter.DoubleToInt64Bits(val));
            _list.Collection.i_Count += 8;
        }

        public void writeQUIC2bit(ulong val)
        {
            if (val < 1UL << 6)
            {
                __ensureCapacity(1);
                _list.Collection.i_Items[_list.Collection.i_Count++] = (byte)val;
            }
            else if (val < 1UL << 14)
            {
                __ensureCapacity(2);
                BinaryPrimitives.WriteUInt16BigEndian(_list.AsSpan(_list.Collection.i_Count, 2), (ushort)(0x4000 | val));
                _list.Collection.i_Count += 2;
            }
            else if (val < 1UL << 30)
            {
                __ensureCapacity(4);
                BinaryPrimitives.WriteUInt32BigEndian(_list.AsSpan(_list.Collection.i_Count, 4), (uint)(0x80000000 | val));
                _list.Collection.i_Count += 4;
            }
            else if (val < 0x3F00000000000000UL)
            {
                __ensureCapacity(8);
                BinaryPrimitives.WriteUInt64BigEndian(_list.AsSpan(_list.Collection.i_Count, 8), 0xC000000000000000 | val);
                _list.Collection.i_Count += 8;
            }
            else
            {
                __ensureCapacity(9);
                _list.Collection.i_Items[_list.Collection.i_Count++] = 0xFF;
                BinaryPrimitives.WriteUInt64BigEndian(_list.AsSpan(_list.Collection.i_Count, 8), val);
                _list.Collection.i_Count += 8;
            }
        }
        public void writeQUIC2bit(ushort val) => writeQUIC2bit((ulong)val);
        public void writeQUIC2bit(uint val) => writeQUIC2bit((ulong)val);
        public void writeQUIC2bit(int val) => writeQUIC2bit((uint)val);

        public void writeQUIC2bitSigned(short val) => writeQUIC2bitSigned((long)val);
        public void writeQUIC2bitSigned(int val) => writeQUIC2bitSigned((long)val);
        public void writeQUIC2bitSigned(long val) => writeQUIC2bit(NMath.zigZagEncode(val));

        public void writeUTF8(ReadOnlySpan<char> chars)
        {
            var bytesCount = Encoding.UTF8.GetByteCount(chars);
            __writeString(Encoding.UTF8, chars, bytesCount);
        }

        public void writeUnicode(ReadOnlySpan<char> chars)
        {
            var bytesCount = Encoding.Unicode.GetByteCount(chars);
            __writeString(Encoding.Unicode, chars, bytesCount);
        }

        private void __writeString(Encoding encoding, ReadOnlySpan<char> chars, int bytesCount)
        {
            var prefixSize = __quic2bitSize((ulong)bytesCount);
            __ensureCapacity(prefixSize + bytesCount);
            __writeQUIC2bitAt(_list.Collection.i_Count, (ulong)bytesCount);
            encoding.GetBytes(chars, _list.AsSpan(_list.Collection.i_Count + prefixSize, bytesCount));
            _list.Collection.i_Count += prefixSize + bytesCount;
        }

        public void writeBytes(ReadOnlySpan<byte> bytes)
        {
            writeQUIC2bit(bytes.Length);
            if (bytes.Length > 0)
            {
                __ensureCapacity(bytes.Length);
                bytes.CopyTo(_list.AsSpan(_list.Collection.i_Count, bytes.Length));
                _list.Collection.i_Count += bytes.Length;
            }
        }

        public void writeBytesWithoutLength(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length > 0)
            {
                __ensureCapacity(bytes.Length);
                bytes.CopyTo(_list.AsSpan(_list.Collection.i_Count, bytes.Length));
                _list.Collection.i_Count += bytes.Length;
            }
        }

        public void clear()
        {
            _list?.Collection.Clear();
        }

        public Span<byte> asSpan()
        {
            return _list.AsSpan();
        }

        public byte[] ToArray()
        {
            return asSpan().ToArray();
        }

        private static int __quic2bitSize(ulong val)
        {
            if (val < 1UL << 6) return 1;
            if (val < 1UL << 14) return 2;
            if (val < 1UL << 30) return 4;
            if (val < 0x3F00000000000000UL) return 8;
            return 9;
        }

        private void __writeQUIC2bitAt(int index, ulong val)
        {
            if (val < 1UL << 6)
            {
                _list.Collection.i_Items[index] = (byte)val;
            }
            else if (val < 1UL << 14)
            {
                BinaryPrimitives.WriteUInt16BigEndian(_list.AsSpan(index, 2), (ushort)(0x4000 | val));
            }
            else if (val < 1UL << 30)
            {
                BinaryPrimitives.WriteUInt32BigEndian(_list.AsSpan(index, 4), (uint)(0x80000000 | val));
            }
            else if (val < 0x3F00000000000000UL)
            {
                BinaryPrimitives.WriteUInt64BigEndian(_list.AsSpan(index, 8), 0xC000000000000000 | val);
            }
            else
            {
                _list.Collection.i_Items[index] = 0xFF;
                BinaryPrimitives.WriteUInt64BigEndian(_list.AsSpan(index + 1, 8), val);
            }
        }
    }
}
