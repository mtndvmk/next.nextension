using System;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Nextension
{
    public static class NConverter
    {
        public static unsafe byte[] getBytes<T>(T t1) where T : unmanaged
        {
            byte[] array = new byte[NUtils.sizeOf<T>()];
            writeBytesWithoutChecks(array, t1);
            return array;
        }
        public static unsafe void writeBytes<T>(byte[] inData, T t1, int startIndex) where T : unmanaged
        {
            InternalCheck.checkValidArray(inData, startIndex, NUtils.sizeOf<T>());
            writeBytesWithoutChecks(inData, t1, startIndex);
        }
        public static unsafe void writeBytes<T>(byte[] inData, Span<T> span, int startIndex) where T : unmanaged
        {
            writeBytes(inData, span.asReadOnlySpan(), startIndex);
        }
        public static unsafe void writeBytes<T>(byte[] inData, ReadOnlySpan<T> span, int startIndex) where T : unmanaged
        {
            var srcSize = span.Length * NUtils.sizeOf<T>();
            InternalCheck.checkValidArray(inData, startIndex, srcSize);
            fixed (byte* dstPtr = &inData[startIndex])
            {
                fixed (T* srcPtr = span)
                {
                    Buffer.MemoryCopy(srcPtr, dstPtr, inData.Length, srcSize);
                }
            }
        }

        public static unsafe void writeBytesWithoutChecks<T>(byte[] inData, T t1, int startIndex = 0) where T : unmanaged
        {
            fixed (byte* ptr = &inData[startIndex])
            {
                *(T*)ptr = t1;
            }
        }

        public static unsafe T fromBytes<T>(byte[] inData, ref int startIndex) where T : unmanaged
        {
            InternalCheck.checkValidArray(inData, startIndex, NUtils.sizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, ref startIndex);
        }
        public static unsafe T fromBytes<T>(byte[] inData, int startIndex = 0) where T : unmanaged
        {
            InternalCheck.checkValidArray(inData, startIndex, NUtils.sizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, startIndex);
        }
        public static unsafe T fromBytes<T>(ReadOnlySpan<byte> inData, int startIndex = 0) where T : unmanaged
        {
            InternalCheck.checkValidArray(inData, startIndex, NUtils.sizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, startIndex);
        }
        public static unsafe T fromBytes<T>(ReadOnlySpan<byte> inData, ref int startIndex) where T : unmanaged
        {
            InternalCheck.checkValidArray(inData, startIndex, NUtils.sizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, ref startIndex);
        }

        public static T fromBytesWithoutChecks<T>(byte[] inData, ref int startIndex) where T : unmanaged
        {
            return fromBytesWithoutChecks<T>(inData.AsSpan(), ref startIndex);
        }
        public static unsafe T fromBytesWithoutChecks<T>(byte[] inData, int startIndex = 0) where T : unmanaged
        {
            return fromBytesWithoutChecks<T>(inData.AsSpan(), startIndex);
        }
        public static unsafe T fromBytesWithoutChecks<T>(ReadOnlySpan<byte> inData, ref int startIndex) where T : unmanaged
        {
            var result = fromBytesWithoutChecks<T>(inData, startIndex);
            startIndex += NUtils.sizeOf<T>();
            return result;
        }
        public static unsafe T fromBytesWithoutChecks<T>(ReadOnlySpan<byte> inData, int startIndex = 0) where T : unmanaged
        {
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
        public static byte[] getUTF8Bytes(string inData)
        {
            return Encoding.UTF8.GetBytes(inData);
        }

        public unsafe static string getUTF8String(byte[] inData, int startIndex, int bytesCount)
        {
            fixed (byte* ptr = &inData[startIndex])
            {
                return Encoding.UTF8.GetString(ptr, bytesCount);
            }
        }
        public static string getUTF8String(byte[] inData, ref int startIndex, int bytesCount)
        {
            var result = getUTF8String(inData, startIndex, bytesCount);
            startIndex += bytesCount;
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
        public unsafe static string getUTF8String(ReadOnlySpan<byte> inData)
        {
            fixed (byte* ptr = &inData[0])
            {
                return Encoding.UTF8.GetString(ptr, inData.Length);
            }
        }

        public static unsafe TOut bitConvert<TIn, TOut>(TIn inValue) where TIn : unmanaged where TOut : unmanaged
        {
            var sizeOfTin = NUtils.sizeOf<TIn>();
            var sizeOfTOut = NUtils.sizeOf<TOut>();

            if (sizeOfTin != sizeOfTOut)
            {
                throw new Exception("TIn and TOut binary must be the same size");
            }
            return *(TOut*)&inValue;
        }
        public static unsafe TOut bitConvertWithoutChecks<TIn, TOut>(TIn inValue) where TIn : unmanaged where TOut : unmanaged
        {
            return *(TOut*)&inValue;
        }
        public static unsafe TOut bitConvertDiffSize<TIn, TOut>(TIn inValue) where TIn : unmanaged where TOut : unmanaged
        {
            var sizeOfTin = NUtils.sizeOf<TIn>();
            var sizeOfTOut = NUtils.sizeOf<TOut>();

            if (sizeOfTOut > sizeOfTin)
            {
                TOut result = default;
                *(TIn*)&result = inValue;
                return result;
            }
            else
            {
                return *(TOut*)&inValue;
            }
        }
        public unsafe static T[] convert<T>(byte[] src) where T : unmanaged
        {
            return convert<T>(src.AsSpan());
        }
        public unsafe static T[] convert<T>(ReadOnlySpan<byte> src) where T : unmanaged
        {
            var tSize = NUtils.sizeOf<T>();
            var resultLength = src.Length / tSize;
            var dst = new T[resultLength];
            fixed (byte* srcPtr = src)
            {
                fixed (T* dstPtr = dst)
                {
                    Buffer.MemoryCopy(srcPtr, dstPtr, src.Length, src.Length);
                }
            }
            return dst;
        }
        public unsafe static T[] convert<T>(byte* src, int byteCount) where T : unmanaged
        {
            var tSize = NUtils.sizeOf<T>();
            var resultLength = byteCount / tSize;
            var dst = new T[resultLength];
            fixed (T* dstPtr = dst)
            {
                Buffer.MemoryCopy(src, dstPtr, byteCount, byteCount);
            }
            return dst;
        }
        public unsafe static NativeArray<T> convertToNativeArray<T>(IntPtr src, int bytesLength, Allocator allocator) where T : unmanaged
        {
            var tSize = NUtils.sizeOf<T>();
            var dstLength = bytesLength / tSize;
            NativeArray<T> arr = new(dstLength, allocator, NativeArrayOptions.UninitializedMemory);
            void* dst = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(arr);

            Buffer.MemoryCopy((void*)src, dst, bytesLength, bytesLength);
            return arr;
        }
        public unsafe static TOut[] convertArray<TIn, TOut>(TIn[] from) where TIn : unmanaged where TOut : unmanaged
        {
            return convertArray<TIn, TOut>(from.AsSpan());
        }
        public unsafe static TOut[] convertArray<TIn, TOut>(ReadOnlySpan<TIn> from) where TIn : unmanaged where TOut : unmanaged
        {
            int sizeOfTIn = NUtils.sizeOf<TIn>();
            int sizeOfTOut = NUtils.sizeOf<TOut>();
            int sizeInBytes = sizeOfTIn * from.Length;

            TOut[] result = new TOut[sizeInBytes / sizeOfTOut];
            fixed (TIn* src = from)
            {
                fixed (TOut* dst = result)
                {
                    Buffer.MemoryCopy(src, dst, sizeInBytes, sizeInBytes);

                }
            }
            return result;
        }
        /// <summary>
        /// Convert binary array to dst array
        /// </summary>
        public static bool tryConvert<T>(byte[] src, out T[] result) where T : unmanaged
        {
            try
            {
                result = convert<T>(src);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                result = null;
                return false;
            }
        }
        public static bool tryConvert<T>(ReadOnlySpan<byte> src, out T[] result) where T : unmanaged
        {
            try
            {
                result = convert<T>(src);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                result = null;
                return false;
            }
        }
        /// <summary>
        /// Convert src array to result array using binary of src array
        /// </summary>
        public static bool tryConvertArray<TIn, TOut>(TIn[] from, out TOut[] result) where TIn : unmanaged where TOut : unmanaged
        {
            try
            {
                result = convertArray<TIn, TOut>(from);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                result = null;
                return false;
            }
        }
        public static bool tryConvertArray<TIn, TOut>(ReadOnlySpan<TIn> from, out TOut[] result) where TIn : unmanaged where TOut : unmanaged
        {
            try
            {
                result = convertArray<TIn, TOut>(from);
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