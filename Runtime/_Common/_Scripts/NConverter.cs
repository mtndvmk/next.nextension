using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Nextension
{
    public static class NConverter
    {
        public static byte[] getBytesOf(object inData)
        {
            var valueType = inData.GetType();
            if (valueType == typeof(short))
            {
                return getBytes((short)inData);
            }
            if (valueType == typeof(ushort))
            {
                return getBytes((ushort)inData);
            }
            if (valueType == typeof(int))
            {
                return getBytes((int)inData);
            }
            if (valueType == typeof(uint))
            {
                return getBytes((uint)inData);
            }
            if (valueType == typeof(float))
            {
                return getBytes((float)inData);
            }
            if (valueType == typeof(long))
            {
                return getBytes((long)inData);
            }
            if (valueType == typeof(ulong))
            {
                return getBytes((ulong)inData);
            }
            if (valueType == typeof(string))
            {
                return getBytes((string)inData);
            }
            if (valueType == typeof(Vector3))
            {
                return getBytes((Vector3)inData);
            }

            Debug.LogWarning($"Now not support getBytes [{valueType}]");
            return null;
        }
        public static T fromBytes<T>(byte[] inData, ref int startIndex)
        {
            var valueType = typeof(T);
            if (valueType == typeof(short))
            {
                return (T)(object)toInt16(inData, ref startIndex);
            }
            if (valueType == typeof(ushort))
            {
                return (T)(object)toUInt16(inData, ref startIndex);
            }
            if (valueType == typeof(int))
            {
                return (T)(object)toInt32(inData, ref startIndex);
            }
            if (valueType == typeof(uint))
            {
                return (T)(object)toUInt32(inData, ref startIndex);
            }
            if (valueType == typeof(float))
            {
                return (T)(object)toFloat(inData, ref startIndex);
            }
            if (valueType == typeof(long))
            {
                return (T)(object)toInt64(inData, ref startIndex);
            }
            if (valueType == typeof(ulong))
            {
                return (T)(object)toUInt64(inData, ref startIndex);
            }
            if (valueType == typeof(string))
            {
                var outValue = (T)(object)getUTF8StringToEnd(inData, startIndex);
                startIndex = inData.Length;
                return outValue;
            }

            Debug.LogWarning($"Now not support fromBytes [{valueType}]");
            return default;
        }
        public static T fromBytes<T>(byte[] inData, int startIndex)
        {
            var tempIndex = startIndex;
            return fromBytes<T>(inData, ref tempIndex);
        }


        public static byte[] getBytes(short inData)
        {
            return BitConverter.GetBytes(inData);
        }
        public static byte[] getBytes(ushort inData)
        {
            return BitConverter.GetBytes(inData);
        }
        public static byte[] getBytes(int inData)
        {
            return BitConverter.GetBytes(inData);
        }
        public static byte[] getBytes(uint inData)
        {
            return BitConverter.GetBytes(inData);
        }
        public static byte[] getBytes(float inData)
        {
            return BitConverter.GetBytes(inData);
        }
        public static byte[] getBytes(long inData)
        {
            return BitConverter.GetBytes(inData);
        }
        public static byte[] getBytes(ulong inData)
        {
            return BitConverter.GetBytes(inData);
        }
        /// <summary>
        /// UTF8 Encoding
        /// </summary>
        /// <param name="inData"></param>
        /// <returns></returns>
        public static byte[] getBytes(string inData)
        {
            return Encoding.UTF8.GetBytes(inData);
        }
        public static byte[] getBytes(Vector2 inData)
        {
            var xBuffer = NConverter.getBytes(inData.x);
            var yBuffer = NConverter.getBytes(inData.y);
            return NUtils.mergeTo(xBuffer, yBuffer);
        }
        public static byte[] getBytes(Vector3 inData)
        {
            var xBuffer = NConverter.getBytes(inData.x);
            var yBuffer = NConverter.getBytes(inData.y);
            var zBuffer = NConverter.getBytes(inData.z);
            return NUtils.merge(xBuffer, yBuffer, zBuffer);
        }

        public static short toInt16(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toInt16(inData, startIndex);
            startIndex += 2;
            return result;
        }
        public static ushort toUInt16(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toUInt16(inData, startIndex);
            startIndex += 2;
            return result;
        }
        public static int toInt32(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toInt32(inData, startIndex);
            startIndex += 4;
            return result;
        }
        public static uint toUInt32(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toUInt32(inData, startIndex);
            startIndex += 4;
            return result;
        }
        public static float toFloat(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toFloat(inData, startIndex);
            startIndex += 4;
            return result;
        }
        public static long toInt64(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toInt64(inData, startIndex);
            startIndex += 8;
            return result;
        }
        public static ulong toUInt64(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toUInt64(inData, startIndex);
            startIndex += 8;
            return result;
        }
        public static Vector2 toVector2(byte[] inData, ref int startIndex)
        {
            var result = NConverter.toVector2(inData, startIndex);
            startIndex += 8;
            return result;
        }
        public static Vector3 toVector3(byte[] inData, ref int startIndex)
        {
            var result = toVector3(inData, startIndex);
            startIndex += 12;
            return result;
        }

        public static short toInt16(byte[] inData, int startIndex)
        {
            var result = BitConverter.ToInt16(inData, startIndex);
            return result;
        }
        public static ushort toUInt16(byte[] inData, int startIndex)
        {
            var result = BitConverter.ToUInt16(inData, startIndex);
            return result;
        }
        public static int toInt32(byte[] inData, int startIndex)
        {
            var result = BitConverter.ToInt32(inData, startIndex);
            return result;
        }
        public static uint toUInt32(byte[] inData, int startIndex)
        {
            var result = BitConverter.ToUInt32(inData, startIndex);
            return result;
        }
        public static float toFloat(byte[] inData, int startIndex)
        {
            var result = BitConverter.ToSingle(inData, startIndex);
            return result;
        }
        public static long toInt64(byte[] inData, int startIndex)
        {
            var result = BitConverter.ToInt64(inData, startIndex);
            return result;
        }
        public static ulong toUInt64(byte[] inData, int startIndex)
        {
            var result = BitConverter.ToUInt64(inData, startIndex);
            return result;
        }
        public static Vector2 toVector2(byte[] inData, int startIndex)
        {
            float x = BitConverter.ToSingle(inData, startIndex);
            float y = BitConverter.ToSingle(inData, startIndex + 4);
            return new Vector2(x, y);
        }
        public static Vector3 toVector3(byte[] inData, int startIndex)
        {
            float x = BitConverter.ToSingle(inData, startIndex);
            float y = BitConverter.ToSingle(inData, startIndex + 4);
            float z = BitConverter.ToSingle(inData, startIndex + 8);
            return new Vector3(x, y, z);
        }

        public static string getUTF8String(byte[] inData, int startIndex, int count)
        {
            return Encoding.UTF8.GetString(inData, startIndex, count);
        }
        public static string getUTF8String(byte[] inData, ref int startIndex, int count)
        {
            var result = getUTF8String(inData, startIndex, count);
            startIndex += count;
            return result;
        }
        public static string getUTF8StringToEnd(byte[] inData, int startIndex)
        {
            int count = inData.Length - startIndex;
            return getUTF8String(inData, startIndex, count);
        }
        public static string getUTF8String(byte[] inData)
        {
            return getUTF8StringToEnd(inData, 0);
        }

        /// <summary>
        /// Convert src array to result array using binary of src array
        /// </summary>
        public static bool tryConvertArray<T1, T2>(T1[] from, out T2[] result)
        {
            try
            {
                if (typeof(T1) == typeof(byte))
                {
                    return tryConvert(from as byte[], out result);
                }
                var t1Size = Marshal.SizeOf(typeof(T1));
                var bytes = new byte[from.Length * t1Size];
                var fromPtr = Marshal.UnsafeAddrOfPinnedArrayElement(from, 0);

                Marshal.Copy(fromPtr, bytes, 0, bytes.Length);

                if (typeof(T2) == typeof(byte))
                {
                    result = bytes as T2[];
                    return true;
                }

                return tryConvert(bytes, out result);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                result = null;
                return false;
            }
        }
        /// <summary>
        /// Convert binary array to dst array
        /// </summary>
        public static bool tryConvert<T>(byte[] src, out T[] result)
        {
            try
            {
                var tSize = Marshal.SizeOf(typeof(T));
                var resultLength = src.Length / tSize;
                var t = new T[resultLength];
                var tPtr = Marshal.UnsafeAddrOfPinnedArrayElement(t, 0);
                Marshal.Copy(src, 0, tPtr, src.Length);
                result = t;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                result = null;
                return false;
            }
        }
    }
}