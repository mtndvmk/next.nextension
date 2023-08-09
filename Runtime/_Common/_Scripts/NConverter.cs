using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Nextension
{
    public static class NConverter
    {
        public static unsafe byte[] getBytes<T>(T t1) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            byte[] array = new byte[sizeOfT];
            fixed (byte* ptr = array)
            {
                *(T*)ptr = t1;
            }
            return array;
        }
        public static unsafe byte[] getBytes<T>(T t1, T t2) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            byte[] array = new byte[2 * sizeOfT];
            fixed (byte* ptr = array)
            {
                T* ptrOfT = (T*)ptr;
                *ptrOfT++ = t1;
                *ptrOfT = t2;
            }
            return array;
        }
        public static unsafe byte[] getBytes<T>(T t1, T t2, T t3) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            byte[] array = new byte[3 * sizeOfT];
            fixed (byte* ptr = array)
            {
                T* ptrOfT = (T*)ptr;
                *ptrOfT++ = t1;
                *ptrOfT++ = t2;
                *ptrOfT = t3;
            }
            return array;
        }
        public static unsafe byte[] getBytes<T>(T t1, T t2, T t3, T t4) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            byte[] array = new byte[4 * sizeOfT];
            fixed (byte* ptr = array)
            {
                T* ptrOfT = (T*)ptr;
                *ptrOfT++ = t1;
                *ptrOfT++ = t2;
                *ptrOfT++ = t3;
                *ptrOfT = t4;
            }
            return array;
        }
        public static unsafe byte[] getBytes<T>(params T[] inData) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            byte[] array = new byte[inData.Length * sizeOfT];
            fixed (byte* ptr = array)
            {
                T* ptrOfT = (T*)ptr;
                for (int i = 0; i < inData.Length; ++i)
                {
                    *ptrOfT++ = inData[i];
                }
            }
            return array;
        }

        public static unsafe void writeBytes<T>(byte[] inData, T t1, int startIndex) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            InternalCheck.checkValidArray(inData, startIndex, sizeOfT);

            fixed (byte* ptr = &inData[startIndex])
            {
                *(T*)ptr = t1;
            }
        }

        public static unsafe T fromBytes<T>(byte[] inData, ref int startIndex) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            InternalCheck.checkValidArray(inData, startIndex, sizeOfT);

            fixed (byte* ptr = &inData[startIndex])
            {
                startIndex += sizeOfT;
                return *(T*)ptr;
            }
        }
        public static unsafe T fromBytes<T>(byte[] inData, int startIndex) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            if (inData == null)
            {
                NThrowHelper.throwArgNullException("inData");
            }

            if ((uint)startIndex >= inData.Length)
            {
                NThrowHelper.throwArgOutOfRangeException("startIndex", startIndex, inData.Length);
            }

            if (startIndex > inData.Length - sizeOfT)
            {
                NThrowHelper.throwArrayLengthToSmallException("inData", startIndex, inData.Length, sizeOfT);
            }

            fixed (byte* ptr = &inData[startIndex])
            {
                return *(T*)ptr;
            }
        }
        public static unsafe T fromBytes<T>(Span<byte> inData, int startIndex) where T : unmanaged
        {
            var sizeOfT = Marshal.SizeOf<T>();
            if (inData == null)
            {
                NThrowHelper.throwArgNullException("inData");
            }

            if ((uint)startIndex >= inData.Length)
            {
                NThrowHelper.throwArgOutOfRangeException("startIndex", startIndex, inData.Length);
            }

            if (startIndex > inData.Length - sizeOfT)
            {
                NThrowHelper.throwArrayLengthToSmallException("inData", startIndex, inData.Length, sizeOfT);
            }

            fixed (byte* ptr = &inData[startIndex])
            {
                return *(T*)ptr;
            }
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
        public static unsafe TOut bitConvert<TIn, TOut>(TIn inValue) where TIn : unmanaged where TOut : unmanaged
        {
            var sizeOfTin = Marshal.SizeOf<TIn>();
            var sizeOfTOut = Marshal.SizeOf<TOut>();

            if (sizeOfTin != sizeOfTOut)
            {
                throw new Exception("TIn and TOut binary must be the same size");
            }
            return *(TOut*)&inValue;
        }
    }
}