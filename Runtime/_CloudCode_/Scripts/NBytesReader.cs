using System;
using System.Buffers.Binary;
using System.Text;

namespace Nextension
{
    public ref struct NBytesReader
    {
        private int _offset;
        public readonly ReadOnlySpan<byte> content;

        public NBytesReader(ReadOnlySpan<byte> content, int offset = 0)
        {
            _offset = offset;
            this.content = content;
        }

        public readonly bool isEof => _offset >= content.Length;

        public byte readByte() { var r = content[_offset]; _offset += 1; return r; }
        public sbyte readSByte() { var r = (sbyte)content[_offset]; _offset += 1; return r; }
        public bool readBool() { var r = content[_offset] != 0; _offset += 1; return r; }
        public short readInt16() { var r = BinaryPrimitives.ReadInt16LittleEndian(content[_offset..]); _offset += 2; return r; }
        public ushort readUInt16() { var r = BinaryPrimitives.ReadUInt16LittleEndian(content[_offset..]); _offset += 2; return r; }
        public int readInt32() { var r = BinaryPrimitives.ReadInt32LittleEndian(content[_offset..]); _offset += 4; return r; }
        public uint readUInt32() { var r = BinaryPrimitives.ReadUInt32LittleEndian(content[_offset..]); _offset += 4; return r; }
        public long readInt64() { var r = BinaryPrimitives.ReadInt64LittleEndian(content[_offset..]); _offset += 8; return r; }
        public ulong readUInt64() { var r = BinaryPrimitives.ReadUInt64LittleEndian(content[_offset..]); _offset += 8; return r; }
        public float readSingle() { var r = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(content[_offset..])); _offset += 4; return r; }
        public double readDouble() { var r = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(content[_offset..])); _offset += 8; return r; }

        public ulong readQUIC2bitUInt64()
        {
            var first = content[_offset++];

            if (first < 0x40)
            {
                return first;
            }
            if (first < 0x80)
            {
                return ((ulong)(first & 0x3F) << 8) | content[_offset++];
            }
            if (first < 0xC0)
            {
                var r = (ulong)(first & 0x3F) << 24;
                r |= (ulong)content[_offset++] << 16;
                r |= (ulong)content[_offset++] << 8;
                r |= content[_offset++];
                return r;
            }
            if (first == 0xFF)
            {
                var r = BinaryPrimitives.ReadUInt64BigEndian(content[_offset..]);
                _offset += 8;
                return r;
            }
            else
            {
                var r = BinaryPrimitives.ReadUInt64BigEndian(content.Slice(_offset - 1, 8)) & 0x3FFFFFFFFFFFFFFFUL;
                _offset += 7;
                return r;
            }
        }
        public uint readQUIC2bitUInt32() => (uint)readQUIC2bitUInt64();
        public ushort readQUIC2bitUInt16() => (ushort)readQUIC2bitUInt64();

        public long readQUIC2bitSignedInt64() => NMath.zigZagDecode(readQUIC2bitUInt64());
        public int readQUIC2bitSignedInt32() => (int)readQUIC2bitSignedInt64();
        public short readQUIC2bitSignedInt16() => (short)readQUIC2bitSignedInt64();

        public int readQUIC2bitCount() => (int)readQUIC2bitUInt32();

        public ReadOnlySpan<byte> readBytes()
        {
            var len = readQUIC2bitCount();
            var bytes = content.Slice(_offset, len);
            _offset += len;
            return bytes;
        }

        public string readUTF8()
        {
            var len = readQUIC2bitCount();
            var str = Encoding.UTF8.GetString(content.Slice(_offset, len));
            _offset += len;
            return str;
        }

        public string readUnicode()
        {
            var len = readQUIC2bitCount();
            var str = Encoding.Unicode.GetString(content.Slice(_offset, len));
            _offset += len;
            return str;
        }

        public readonly byte peekByte()
        {
            return content[_offset];
        }

        public readonly int peekInt32()
        {
            return BinaryPrimitives.ReadInt32LittleEndian(content[_offset..]);
        }

        public readonly NBytesReader clone()
        {
            return new NBytesReader(content, _offset);
        }
    }
}