using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Nextension
{
    public static class NConverter
    {
        public static byte[] getBytes<T>(T t1)
        {
            byte[] array = new byte[Unsafe.SizeOf<T>()];
            writeBytesWithoutChecks(array, t1);
            return array;
        }

        public static void writeBytes<T>(byte[] inData, T t1, int startIndex)
        {
            InternalCheck.checkValidArray(inData, startIndex, Unsafe.SizeOf<T>());
            writeBytesWithoutChecks(inData, t1, startIndex);
        }

        public static void writeBytes<T>(byte[] inData, Span<T> span, int startIndex) where T : unmanaged
        {
            writeBytes(inData, span.asRoSpan(), startIndex);
        }

        public unsafe static void writeBytes<T>(byte[] inData, ReadOnlySpan<T> span, int startIndex) where T : unmanaged
        {
            int bytesLength = span.Length * Unsafe.SizeOf<T>();
            InternalCheck.checkValidArray(inData, startIndex, bytesLength);
            if (bytesLength > 0)
            {
                var srcPtr = Unsafe.AsPointer(ref Unsafe.AsRef(in span[0]));
                var dstPtr = Unsafe.AsPointer(ref inData[startIndex]);
                Buffer.MemoryCopy(srcPtr, dstPtr, bytesLength, bytesLength);
            }
        }

        public static void writeBytesWithoutChecks<T>(byte[] inData, T t1, int startIndex = 0)
        {
            Unsafe.WriteUnaligned(ref inData[startIndex], t1);
        }

        public static void writeBytesWithoutChecks<T>(Span<byte> inData, T t1, int startIndex = 0)
        {
            Unsafe.WriteUnaligned(ref inData[startIndex], t1);
        }

        public static T fromBytes<T>(byte[] inData, ref int startIndex)
        {
            InternalCheck.checkValidArray(inData, startIndex, Unsafe.SizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, ref startIndex);
        }

        public static T fromBytes<T>(byte[] inData, int startIndex = 0)
        {
            InternalCheck.checkValidArray(inData, startIndex, Unsafe.SizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, startIndex);
        }

        public static T fromBytes<T>(ReadOnlySpan<byte> inData, int startIndex = 0)
        {
            InternalCheck.checkValidArray(inData, startIndex, Unsafe.SizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, startIndex);
        }

        public static T fromBytes<T>(ReadOnlySpan<byte> inData, ref int startIndex)
        {
            InternalCheck.checkValidArray(inData, startIndex, Unsafe.SizeOf<T>());
            return fromBytesWithoutChecks<T>(inData, ref startIndex);
        }

        public static T fromBytesWithoutChecks<T>(byte[] inData, ref int startIndex)
        {
            return fromBytesWithoutChecks<T>(inData.AsSpan(), ref startIndex);
        }

        public static T fromBytesWithoutChecks<T>(byte[] inData, int startIndex = 0)
        {
            return Unsafe.ReadUnaligned<T>(ref inData[startIndex]);
        }

        public static T fromBytesWithoutChecks<T>(ReadOnlySpan<byte> inData, ref int startIndex)
        {
            var result = fromBytesWithoutChecks<T>(inData, startIndex);
            startIndex += Unsafe.SizeOf<T>();
            return result;
        }

        public static T fromBytesWithoutChecks<T>(ReadOnlySpan<byte> inData, int startIndex = 0)
        {
            return Unsafe.ReadUnaligned<T>(ref Unsafe.AsRef(in inData[startIndex]));
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

        public static string getUTF8String(byte[] inData, int startIndex, int bytesCount)
        {
            return Encoding.UTF8.GetString(inData.AsSpan(startIndex, bytesCount));
        }

        public static string getUTF8String(byte[] inData, ref int startIndex, int bytesCount)
        {
            var result = getUTF8String(inData, startIndex, bytesCount);
            startIndex += bytesCount;
            return result;
        }

        public static string getUTF8StringToEnd(byte[] inData, int startIndex)
        {
            return Encoding.UTF8.GetString(inData.AsSpan(startIndex));
        }

        public static string getUTF8String(byte[] inData)
        {
            return Encoding.UTF8.GetString(inData);
        }

        public static string getUTF8String(ReadOnlySpan<byte> inData)
        {
            return Encoding.UTF8.GetString(inData);
        }

        public static TOut bitConvert<TIn, TOut>(TIn inValue) where TIn : unmanaged where TOut : unmanaged
        {
            if (Unsafe.SizeOf<TIn>() != Unsafe.SizeOf<TOut>())
            {
                throw new ArgumentException("TIn and TOut binary must be the same size");
            }
            return Unsafe.As<TIn, TOut>(ref inValue);
        }

        public static TOut bitConvertWithoutChecks<TIn, TOut>(TIn inValue) where TIn : unmanaged where TOut : unmanaged
        {
            return Unsafe.As<TIn, TOut>(ref inValue);
        }

        public static TOut bitConvertSizeChecks<TIn, TOut>(TIn inValue) where TIn : unmanaged where TOut : unmanaged
        {
            if (Unsafe.SizeOf<TOut>() > Unsafe.SizeOf<TIn>())
            {
                TOut result = default;
                Unsafe.As<TOut, TIn>(ref result) = inValue;
                return result;
            }
            else
            {
                return Unsafe.As<TIn, TOut>(ref inValue);
            }
        }

        public static T[] convert<T>(byte[] src) where T : unmanaged
        {
            return convert<T>(src.AsSpan());
        }

        public unsafe static T[] convert<T>(ReadOnlySpan<byte> src) where T : unmanaged
        {
            var tSize = Unsafe.SizeOf<T>();
            var resultLength = src.Length / tSize;
            var dst = new T[resultLength];
            int copyBytes = resultLength * tSize;
            if (copyBytes > 0)
            {
                var srcPtr = Unsafe.AsPointer(ref Unsafe.AsRef(in src[0]));
                var dstPtr = Unsafe.AsPointer(ref dst[0]);
                Buffer.MemoryCopy(srcPtr, dstPtr, copyBytes, copyBytes);
            }
            return dst;
        }

        public static TOut[] convertArray<TIn, TOut>(TIn[] from) where TIn : unmanaged where TOut : unmanaged
        {
            return convertArray<TIn, TOut>(from.AsSpan());
        }

        public unsafe static TOut[] convertArray<TIn, TOut>(ReadOnlySpan<TIn> from) where TIn : unmanaged where TOut : unmanaged
        {
            int totalBytes = from.Length * Unsafe.SizeOf<TIn>();
            int sizeOfTOut = Unsafe.SizeOf<TOut>();
            int resultLength = totalBytes / sizeOfTOut;
            TOut[] result = new TOut[resultLength];
            int copyBytes = resultLength * sizeOfTOut;
            if (copyBytes > 0)
            {
                var srcPtr = Unsafe.AsPointer(ref Unsafe.AsRef(in from[0]));
                var dstPtr = Unsafe.AsPointer(ref result[0]);
                Buffer.MemoryCopy(srcPtr, dstPtr, copyBytes, copyBytes);
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
                NDebug.LogWarning(e);
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
                NDebug.LogWarning(e);
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
                NDebug.LogWarning(e);
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
                NDebug.LogWarning(e);
                result = null;
                return false;
            }
        }
    }
}

