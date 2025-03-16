using System;
using System.Collections.Generic;

namespace Nextension
{
    public class NData
    {
        public NData(byte[] nValue)
        {
            data = nValue;
        }
        public readonly byte[] data;
        public int Length => data.Length;

        public byte[] toBytes()
        {
            var nLength = new NInteger(data.Length);
            var numBytesLength = nLength.estNumBytesLength();
            byte[] result = new byte[numBytesLength + 1 + data.Length];
            int startIndex = 0;
            nLength.writeTo(result, numBytesLength, ref startIndex);
            Buffer.BlockCopy(data, 0, result, startIndex, data.Length);
            return result;
        }
    }

    public static class NDataUtils
    {
        public static NData readNext(this byte[] inData, ref int startIndex)
        {
            var length = (int)NInteger.fromBytes(inData, ref startIndex).Value;
            var data = NUtils.getBlock(inData, startIndex, length); startIndex += length;
            return new NData(data);
        }
        public static bool tryReadNext(this byte[] inData, int startIndex, out NData outNData, out int nextIndex)
        {
            try
            {
                outNData = readNext(inData, ref startIndex);
                nextIndex = startIndex;
                return true;
            }
            catch
            {
                outNData = null;
                nextIndex = startIndex;
                return false;
            }
        }

        public static NData toNData(this string value)
        {
            return new NData(NConverter.getUTF8Bytes(value));
        }
        public static NData toNData(this byte[] value)
        {
            NData nData = new NData(value);
            return nData;
        }
        public static byte[] merge(params NData[] nData)
        {
            int nDataCount = nData.Length;
            Span<byte> nDataLengthSpan = stackalloc byte[nDataCount];
            NInteger dataLengthNInteger = default;
            long totalLength = nDataCount;
            for (int i = 0; i < nDataCount; ++i)
            {
                dataLengthNInteger.Value = nData[i].Length;
                totalLength += dataLengthNInteger.Value + (nDataLengthSpan[i] = dataLengthNInteger.estNumBytesLength());
            }
            var data = new byte[totalLength];
            int pointer = 0;
            for (int i = 0; i < nDataCount; ++i)
            {
                var item = nData[i];
                dataLengthNInteger.Value = item.Length;
                dataLengthNInteger.writeTo(data, nDataLengthSpan[i], ref pointer);
                Buffer.BlockCopy(item.data, 0, data, pointer, item.Length); pointer += item.Length;
            }
            return data;
        }
        public static NData mergeToNData(params NData[] nData)
        {
            return new NData(merge(nData));
        }
        public static IEnumerable<NData> splitNData(NData inNData)
        {
            return splitNData(inNData.data);
        }
        public static IEnumerable<NData> splitNData(byte[] inData)
        {
            int startIndex = 0;
            while (startIndex < inData.Length)
            {
                yield return readNext(inData, ref startIndex);
            }
        }
        public static string getString(this NData nData)
        {
            return NConverter.getUTF8String(nData.data);
        }
    }
}