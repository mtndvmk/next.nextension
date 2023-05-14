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
        public NData(string nStr)
        {
            data = Encoding.UTF8.GetBytes(nStr);
        }

        public int nextOffset;
        public byte[] data;
        public byte[] toBytes()
        {
            return NUtils.merge(BitConverter.GetBytes(data.Length), data);
        }
        public int Length => data.Length;
    }

    public interface IToNData
    {
        public NData toNData();
        public void fromNData(NData inNData);
    }

    public static class NDataUtils
    {
        public static NData readNext(this byte[] inData, ref int startIndex)
        {
            int offset = startIndex;
            var length = BitConverter.ToInt32(inData, offset); offset += 4;
            var data = NUtils.getBlock(inData, offset, length); offset += length;
            var nextOffset = offset;

            NData nData = new NData(data);
            nData.nextOffset = nextOffset;
            startIndex = offset;
            return nData;
        }
        public static bool tryReadNext(this byte[] inData, int startIndex, out NData outNData, out int nextIndex)
        {
            try
            {
                int offset = startIndex;
                outNData = readNext(inData, ref offset);
                nextIndex = offset;
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
            return new NData(value);
        }
        public static NData toNData(this byte[] value)
        {
            NData nData = new NData(value);
            return nData;
        }
        public static byte[] merge(params NData[] nData)
        {
            var data = new byte[0];
            foreach (var n in nData)
            {
                data = data.mergeTo(n.toBytes());
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
            return Encoding.UTF8.GetString(nData.data);
        }
    }
}