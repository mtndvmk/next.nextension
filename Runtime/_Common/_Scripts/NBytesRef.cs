using System;
using System.Collections.Generic;

namespace Nextension
{
    public readonly ref struct NBytesRef
    {
        public NBytesRef(ReadOnlySpan<byte> content)
        {
            this.content = content;
        }

        public readonly ReadOnlySpan<byte> content;

        public int ContentLength => content.Length;
        public int SizeInBytes => content.Length + NInteger.fromBytes(content).estNumBytesLength() + 1;

        public byte[] toBytes()
        {
            var nLength = new NInteger(content.Length);
            var numBytesLength = nLength.estNumBytesLength();
            byte[] result = new byte[numBytesLength + 1 + content.Length];
            int startIndex = 0;
            nLength.writeTo(result, numBytesLength, ref startIndex);
            content.CopyTo(result.AsSpan(startIndex, content.Length));
            return result;
        }
        public NBytes toNBytes()
        {
            return new NBytes(content.ToArray());
        }
        public string toUTF8String()
        {
            return NConverter.getUTF8String(content);
        }
        public static NBytesRef parse(ReadOnlySpan<byte> src, int contentOffset = 0)
        {
            return NBytesUtils.readNextNBytes(src, ref contentOffset);
        }
        public static NBytesRef parse(ReadOnlySpan<byte> src, ref int contentOffset)
        {
            return NBytesUtils.readNextNBytes(src, ref contentOffset);
        }
        public NBytesReader asReader()
        {
            return new NBytesReader(this);
        }
    }
}