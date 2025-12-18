using System;

namespace Nextension
{
    public class NBytes
    {
        public NBytes(byte[] content)
        {
            this.content = content;
        }
        public NBytes(params NBytes[] nBytes)
        {
            content = NBytesUtils.mergeToBytes(nBytes);
        }
        public readonly byte[] content;
        public int ContentLength => content.Length;
        public int SizeInBytes => content.Length + new NInteger(content.Length).estNumBytesLength() + 1;

        public byte[] toBytes()
        {
            var nLength = new NInteger(content.Length);
            var numBytesLength = nLength.estNumBytesLength();
            byte[] result = new byte[numBytesLength + 1 + content.Length];
            int startIndex = 0;
            nLength.writeTo(result, numBytesLength, ref startIndex);
            Buffer.BlockCopy(content, 0, result, startIndex, content.Length);
            return result;
        }
        public string toUTF8String()
        {
            return NConverter.getUTF8String(content);
        }
        public NBytesRef asNBytesRef()
        {
            return new NBytesRef(content);
        }
    }

    public readonly ref struct NBytesRef
    {
        public NBytesRef(ReadOnlySpan<byte> content)
        {
            this.content = content;
        }

        public readonly ReadOnlySpan<byte> content;
        
        public int ContentLength => content.Length;
        public int SizeInBytes => content.Length + NInteger.fromBytes(content).estNumBytesLength() + 1;

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
    }

    public static class NBytesUtils
    {
        public static NBytesRef readChild(this NBytesRef nBytesRef, int contentOffset = 0)
        {
            return readNextNBytes(nBytesRef.content, ref contentOffset);
        }
        public static NBytesRef readChild(this NBytesRef nBytesRef, ref int contentOffset)
        {
            return readNextNBytes(nBytesRef.content, ref contentOffset);
        }
        public static NBytesRef readNextNBytes(this ReadOnlySpan<byte> inData, ref int contentOffset)
        {
            var length = (int)NInteger.fromBytes(inData, ref contentOffset).Value;
            var newSpan = inData.Slice(contentOffset, length);
            contentOffset += length;
            return new NBytesRef(newSpan);
        }

        public static NBytes toNBytes(this string value)
        {
            return new NBytes(NConverter.getUTF8Bytes(value));
        }
        public static NBytes toNBytes(this byte[] value)
        {
            return new NBytes(value);
        }
        public static byte[] mergeToBytes(params NBytes[] nData)
        {
            int nDataCount = nData.Length;
            Span<byte> nDataLengthSpan = stackalloc byte[nDataCount];
            NInteger dataLengthNInteger = default;
            long totalLength = nDataCount;
            for (int i = 0; i < nDataCount; ++i)
            {
                dataLengthNInteger.Value = nData[i].ContentLength;
                totalLength += dataLengthNInteger.Value + (nDataLengthSpan[i] = dataLengthNInteger.estNumBytesLength());
            }
            var data = new byte[totalLength];
            int pointer = 0;
            for (int i = 0; i < nDataCount; ++i)
            {
                var item = nData[i];
                dataLengthNInteger.Value = item.ContentLength;
                dataLengthNInteger.writeTo(data, nDataLengthSpan[i], ref pointer);
                Buffer.BlockCopy(item.content, 0, data, pointer, item.ContentLength); pointer += item.ContentLength;
            }
            return data;
        }
    }
}