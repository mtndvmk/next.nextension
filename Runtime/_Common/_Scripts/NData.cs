using System;
using System.Collections.Generic;
using System.Text;

namespace Nextension
{
    public class NData
    {
        public NData(byte[] nValue)
        {
            data = nValue;
        }
        public readonly byte[] data;
        public byte[] toBytes()
        {
            return NUtils.merge(BitConverter.GetBytes(data.Length), data);
        }
        public int Length => data.Length;
    }

    public static class NDataUtils
    {
        public static NData readNext(this byte[] inData, ref int startIndex)
        {
            var length = NConverter.fromBytes<int>(inData, ref startIndex);
            var data = NUtils.getBlock(inData, startIndex, length); startIndex += length;

            NData nData = new NData(data);
            return nData;
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
            return new NData(NConverter.getBytes(value));
        }
        public static NData toNData(this byte[] value)
        {
            NData nData = new NData(value);
            return nData;
        }
        public static byte[] merge(params NData[] nData)
        {
            int totalLength = 0;
            for (int i = 0; i < nData.Length; ++i)
            {
                totalLength += nData[i].Length;
            }
            var data = new byte[totalLength];
            int pointer = 0;
            for (int i = 0; i < nData.Length; ++i)
            {
                var bytes = nData[i].toBytes();
                Buffer.BlockCopy(bytes, 0, data, pointer, bytes.Length);
                pointer += bytes.Length;
            }
            return data;
        }
        public static NData mergeToNData(params NData[] nData)
        {
            return new NData(merge(nData));
        }
        public static NData[] splitNData(NData inNData)
        {
            List<NData> nDatas = new List<NData>();
            int startIndex = 0;
            var inData = inNData.data;
            while (startIndex < inData.Length)
            {
                var tempNData = readNext(inData, ref startIndex);
                nDatas.Add(tempNData);
            }
            return nDatas.ToArray();
        }
        public static string getString(this NData nData)
        {
            return NConverter.getUTF8String(nData.data);
        }
    }
}