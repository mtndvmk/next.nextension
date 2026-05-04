using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Nextension
{
    public static partial class NUtils
    {
        #region Number & Bit mask
        public static bool isPOT(int n)
        {
            return (n & (n - 1)) == 0 && n > 0;
        }
        public static int nextPOT(int n)
        {
            if (n <= 0) return 1;
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        public static unsafe bool checkBitMask<T>(T mask, int bitIndex) where T : unmanaged, Enum
        {
            var intOfEnum = *(int*)&mask;
            return checkBitMask(intOfEnum, bitIndex);
        }
        /// <summary>
        /// return true if (mask & filter) is not equal 0, otherwise return false
        /// </summary>
        public static unsafe bool checkMask<T>(T mask, T filter) where T : unmanaged, Enum
        {
            return ((*(int*)&mask) & (*(int*)&filter)) != 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        public static bool checkBitMask(int mask, int bitIndex)
        {
            return (mask & (1 << bitIndex)) != 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        public static bool checkBitMask(long longMask, int bitIndex)
        {
            return (longMask & (1L << bitIndex)) != 0;
        }
        public static bool checkBitMask(byte[] byteMask, int bitIndex)
        {
            int byteIndex = bitIndex >> 3;
            int maskIndex = bitIndex & 0x7;
            byte mask = byteMask[byteIndex];
            return (mask & 1 << maskIndex) != 0;
        }

        public static sbyte getBit1Index(int mask)
        {
            if (mask == 0)
            {
                return -1;
            }
            sbyte startIndex = -1;
            if ((mask & 0x0000ffff) != 0)
            {
                if ((mask & 0x000000ff) != 0)
                {
                    startIndex = 0;
                }
                else
                {
                    startIndex = 8;
                }
            }
            else if ((mask & 0xffff0000) != 0)
            {
                if ((mask & 0x00ff0000) != 0)
                {
                    startIndex = 16;
                }
                else
                {
                    startIndex = 24;
                }
            }

            if (startIndex >= 0)
            {
                var endIndex = startIndex + 8;
                for (; startIndex < endIndex; startIndex++)
                {
                    if ((mask & 1 << startIndex) != 0)
                    {
                        return startIndex;
                    }
                }
            }

            return -1;
        }
        public static sbyte getBit1Index(long longMask)
        {
            if (longMask == 0)
            {
                return -1;
            }
            sbyte startIndex = -1;
            if ((longMask & 0x00000000ffffffff) != 0)
            {
                if ((longMask & 0x0000ffff) != 0)
                {
                    if ((longMask & 0x000000ff) != 0)
                    {
                        startIndex = 0;
                    }
                    else
                    {
                        startIndex = 8;
                    }
                }
                else
                {
                    if ((longMask & 0x00ff0000) != 0)
                    {
                        startIndex = 16;
                    }
                    else
                    {
                        startIndex = 24;
                    }
                }
            }
            else if ((longMask & -4294967296) != 0)
            {
                if ((longMask & 0x0000ffff00000000) != 0)
                {
                    if ((longMask & 0x000000ff00000000) != 0)
                    {
                        startIndex = 32;
                    }
                    else
                    {
                        startIndex = 40;
                    }
                }
                else
                {
                    if ((longMask & 0x00ff000000000000) != 0)
                    {
                        startIndex = 48;
                    }
                    else
                    {
                        startIndex = 56;
                    }
                }
            }

            if (startIndex >= 0)
            {
                var endIndex = startIndex + 8;
                for (; startIndex < endIndex; startIndex++)
                {
                    if ((longMask & 1L << startIndex) != 0)
                    {
                        return startIndex;
                    }
                }
            }

            return -1;
        }
        public unsafe static int getBit1Index(byte[] byteMask)
        {
            if (byteMask == null || byteMask.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }

            fixed (byte* ptr = &byteMask[0])
            {
                return getBit1Index(ptr, byteMask.Length);
            }
        }

        public unsafe static int getBit1Index(byte* ptr, int lengthOfBytes)
        {
            int index = lengthOfBytes & 3;
            int intCount = lengthOfBytes >> 2;
            int num;
            if (index != 0)
            {
                if ((num = *(int*)ptr) != 0)
                {
                    return getBit1Index(num);
                }
            }

            int startBitIndex = index << 3;
            int* iPtr = (int*)(ptr + index);
            while (intCount-- > 0)
            {
                if ((num = *iPtr++) != 0)
                {
                    return getBit1Index(num) + startBitIndex;
                }
                startBitIndex += 32;
            }
            return -1;
        }

        public static sbyte getBit0Index(int mask)
        {
            if (mask == -1)
            {
                return -1;
            }
            sbyte startIndex = -1;
            if ((mask & 0x0000ffff) != 0x0000ffff)
            {
                if ((mask & 0x000000ff) != 0x000000ff)
                {
                    startIndex = 0;
                }
                else
                {
                    startIndex = 8;
                }
            }
            else if ((mask & 0xffff0000) != 0xffff0000)
            {
                if ((mask & 0x00ff0000) != 0x00ff0000)
                {
                    startIndex = 16;
                }
                else
                {
                    startIndex = 24;
                }
            }

            if (startIndex >= 0)
            {
                var endIndex = startIndex + 8;
                for (; startIndex < endIndex; startIndex++)
                {
                    if ((mask & 1 << startIndex) == 0)
                    {
                        return startIndex;
                    }
                }
            }

            return -1;
        }
        public static sbyte getBit0Index(long longMask)
        {
            if (longMask == -1)
            {
                return -1;
            }
            sbyte startIndex = -1;
            if ((longMask & 0x00000000ffffffff) != 0x00000000ffffffff)
            {
                if ((longMask & 0x0000ffff) != 0x0000ffff)
                {
                    if ((longMask & 0x000000ff) != 0x000000ff)
                    {
                        startIndex = 0;
                    }
                    else
                    {
                        startIndex = 8;
                    }
                }
                else
                {
                    if ((longMask & 0x00ff0000) != 0x00ff0000)
                    {
                        startIndex = 16;
                    }
                    else
                    {
                        startIndex = 24;
                    }
                }
            }
            else if ((longMask & -4294967296) != -4294967296)
            {
                if ((longMask & 0x0000ffff00000000) != 0x0000ffff00000000)
                {
                    if ((longMask & 0x000000ff00000000) != 0x000000ff00000000)
                    {
                        startIndex = 32;
                    }
                    else
                    {
                        startIndex = 40;
                    }
                }
                else
                {
                    if ((longMask & 0x00ff000000000000) != 0x00ff000000000000)
                    {
                        startIndex = 48;
                    }
                    else
                    {
                        startIndex = 56;
                    }
                }
            }

            if (startIndex >= 0)
            {
                var endIndex = startIndex + 8;
                for (; startIndex < endIndex; startIndex++)
                {
                    if ((longMask & 1L << startIndex) == 0)
                    {
                        return startIndex;
                    }
                }
            }

            return -1;
        }
        public unsafe static int getBit0Index(byte[] byteMask)
        {
            if (byteMask == null || byteMask.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }

            fixed (byte* ptr = &byteMask[0])
            {
                return getBit0Index(ptr, byteMask.Length);
            }
        }
        public unsafe static int getBit0Index(byte* ptr, int lengthOfBytes)
        {
            int index = lengthOfBytes & 3;
            int intCount = lengthOfBytes >> 2;
            int num;

            if (index != 0)
            {
                if ((num = *(int*)ptr) != -1)
                {
                    return getBit0Index(num);
                }
            }

            int startBitIndex = index << 3;
            int* iPtr = (int*)(ptr + index);
            while (intCount-- > 0)
            {
                if ((num = *iPtr++) != -1)
                {
                    return getBit0Index(num) + startBitIndex;
                }
                startBitIndex += 32;
            }
            return -1;
        }


        public static int setBit0(int mask, int bitIndex)
        {
            return mask &= ~(1 << bitIndex);
        }

        public static long setBit0(long mask, int bitIndex)
        {
            return mask &= ~(1L << bitIndex);
        }

        public static void setBit0(this byte[] bytes, int bitIndex)
        {
            var byteIndex = bitIndex >> 3;
            bitIndex &= 0x7;
            bytes[byteIndex] &= (byte)~(1 << bitIndex);
        }

        public static int setBit1(int mask, int bitIndex)
        {
            return mask |= 1 << bitIndex;
        }

        public static long setBit1(long mask, int bitIndex)
        {
            return mask |= 1L << bitIndex;
        }

        public static void setBit1(this byte[] bytes, int bitIndex)
        {
            var byteIndex = bitIndex >> 3;
            bitIndex &= 0x7;
            bytes[byteIndex] |= (byte)(1 << bitIndex);
        }

        public unsafe static bool isOnly1(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }

            fixed (byte* ptr = &bytes[0])
            {
                return isOnly1(ptr, bytes.Length);
            }
        }
        public unsafe static bool isOnly1(byte* ptr, int lengthOfBytes)
        {
            int index = lengthOfBytes & 3;
            int length = lengthOfBytes >> 2;
            if (index != 0)
            {
                if (*(int*)ptr != -1)
                {
                    return false;
                }
            }

            int* iPtr = (int*)(ptr + index);
            while (length-- > 0)
            {
                if (*iPtr++ != -1)
                {
                    return false;
                }
            }
            return true;
        }

        public static unsafe bool isOnly0(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }

            fixed (byte* ptr = &bytes[0])
            {
                return isOnly0(ptr, bytes.Length);
            }
        }
        public unsafe static bool isOnly0(byte* ptr, int lengthOfBytes)
        {
            int index = lengthOfBytes & 3;
            int length = lengthOfBytes >> 2;

            if (index != 0)
            {
                if (*(int*)ptr != 0)
                {
                    return false;
                }
            }

            int* iPtr = (int*)(ptr + index);
            while (length-- > 0)
            {
                if (*iPtr++ != 0)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region String
        public static string formatToMMSS(this long totalSeconds)
        {
            var m = totalSeconds / 60;
            var s = totalSeconds % 60;
            return NStringBuilder.get()[m.ToString("00")][':'].Append(s.ToString("00")).consume();
        }
        public static string formatToHHMMSS(this long totalSeconds)
        {
            var h = totalSeconds / 3600;
            var remainingM = totalSeconds % 3600;
            var m = remainingM / 60;
            var s = remainingM % 60;
            return NStringBuilder.get()[h.ToString("00")][':'][m.ToString("00")][':'].Append(s.ToString("00")).consume();
        }
        public static bool isHex(this string str)
        {
            int offset;
            var span = str.AsSpan();
            if (span[0] == '0' && (span[1] == 'x' || span[1] == 'X'))
            {
                offset = 2;
            }
            else
            {
                offset = 0;
            }
            bool isHex;
            int strLength = str.Length;
            for (; offset < strLength; ++offset)
            {
                var c = span[offset];
                isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!isHex)
                {
                    return false;
                }
            }
            return true;
        }
        public static unsafe string bytesToHex(this byte[] inData, bool include0xPrefix = false)
        {
            fixed (byte* ptr = inData)
            {
                return bytesToHex(ptr, inData.Length, include0xPrefix);
            }
        }
        public static unsafe string bytesToHex(this Span<byte> inData, bool include0xPrefix = false)
        {
            fixed (byte* ptr = inData)
            {
                return bytesToHex(ptr, inData.Length, include0xPrefix);
            }
        }
        public static unsafe string bytesToHex(byte* inData, int inDataLength, bool include0xPrefix = false)
        {
            int hexLength = include0xPrefix ? (inDataLength * 2 + 2) : inDataLength * 2;
            using var sb = NStringBuilder.get(hexLength);
            bytesToHex(sb, inData, inDataLength, include0xPrefix);
            return sb.ToString();
        }
        public static unsafe void bytesToHex(NStringBuilder sb, byte* inData, int inDataLength, bool include0xPrefix = false)
        {
            int hexLength = include0xPrefix ? (inDataLength * 2 + 2) : inDataLength * 2;
            if (include0xPrefix)
            {
                sb.Append('0');
                sb.Append('x');
            }
            for (int i = 0; i < inDataLength; i++)
            {
                var b = inData[i] >> 4;
                var b1 = (int)((uint)(9 - b) >> 31);
                sb.Append((char)(0x30 + b + (b1 << 3) - b1));
                b = inData[i] & 0xf;
                b1 = (int)((uint)(9 - b) >> 31);
                sb.Append((char)(0x30 + b + (b1 << 3) - b1));
            }
        }
        private static ReadOnlySpan<char> internal_getHexNoPrefix(string hex, int startIndex, int hexLength)
        {
            var c0 = hex[startIndex];
            var c1 = hex[startIndex + 1];
            if (c0 == '0' && (c1 == 'x' || c1 == 'X'))
            {
                startIndex += 2;
                hexLength -= 2;
            }
            return hex.AsSpan(startIndex, hexLength);
        }
        private static unsafe void internal_hexToBytes(ReadOnlySpan<char> hexSpan, byte* dst)
        {
            var hexLength = hexSpan.Length;
            if ((hexLength & 1) != 0) throw new Exception($"Invalid hex length: {nameof(hexSpan)}({hexSpan.ToString()})");
            for (int i = 0; i < hexLength;)
            {
                var index = i >> 1;
                var c0 = byteOfHex(hexSpan[i++]);
                var c1 = byteOfHex(hexSpan[i++]);
                dst[index] = (byte)(c0 << 4 | c1);
            }
        }
        private static readonly byte[] _hexTable = __createHexTable();

        private static byte[] __createHexTable()
        {
            byte[] table = new byte[256];
            table.fill((byte)255);

            for (int i = 0; i < 10; i++) table['0' + i] = (byte)i;
            for (int i = 0; i < 6; i++)
            {
                table['A' + i] = (byte)(10 + i);
                table['a' + i] = (byte)(10 + i);
            }
            return table;
        }

        public static byte byteOfHex(char hexChar)
        {
            if (hexChar > 255) 
                throw new ArgumentException("Invalid hex character");

            byte result = _hexTable[hexChar];
            
            if (result == 255)
                throw new ArgumentException("Invalid hex character: " + hexChar);

            return result;
        }
        /// <summary>
        /// Require hex length mod 2 == 0
        /// </summary>
        public static unsafe byte[] hexToBytes(this string hex, int startIndex = 0)
        {
            var hexLength = hex.Length;
            var hexSpan = internal_getHexNoPrefix(hex, startIndex, hexLength);

            byte[] result = new byte[hexSpan.Length >> 1];
            fixed (byte* ptr = result)
            {
                internal_hexToBytes(hexSpan, ptr);
            }
            return result;
        }
        /// <summary>
        /// Require hex length mod 2 == 0
        /// </summary>
        public static unsafe byte[] hexToBytes(this ReadOnlySpan<char> hexSpan)
        {
            var hexLength = hexSpan.Length;
            byte[] result = new byte[hexSpan.Length >> 1];
            fixed (byte* ptr = result)
            {
                internal_hexToBytes(hexSpan, ptr);
            }
            return result;
        }
        /// <summary>
        /// Require hex length mod 2 == 0
        /// </summary>
        public static unsafe void hexToBytes(string hex, int startIndex, int hexLength, byte[] dst, int dstIndex)
        {
            var hexSpan = internal_getHexNoPrefix(hex, startIndex, hexLength);

            fixed (byte* ptr = dst)
            {
                internal_hexToBytes(hexSpan, &ptr[dstIndex]);
            }
        }


        public static bool isEqualHex(this string hex0, string hex1)
        {
            ReadOnlySpan<char> hexSpan0;
            ReadOnlySpan<char> hexSpan1;
            if (hex0[1] == 'x' || hex0[1] == 'X')
            {
                hexSpan0 = hex0.AsSpan(2);
            }
            else
            {
                hexSpan0 = hex0.AsSpan();
            }

            if (hex1[1] == 'x' || hex1[1] == 'X')
            {
                hexSpan1 = hex1.AsSpan(2);
            }
            else
            {
                hexSpan1 = hex1.AsSpan();
            }
            return hexSpan0.Equals(hexSpan1, StringComparison.OrdinalIgnoreCase);
        }
        public static string computeMD5(this string s)
        {
            var strSpan = s.AsSpan();
            using var provider = System.Security.Cryptography.MD5.Create();
            Span<byte> dst = stackalloc byte[16];
            provider.TryComputeHash(strSpan.asSpan<char, byte>(), dst, out _);
            return bytesToHex(dst);
        }
        public static decimal computeMD5AsDecimal(this string s)
        {
            var strSpan = s.AsSpan();
            using var provider = System.Security.Cryptography.MD5.Create();
            Span<byte> dst = stackalloc byte[16];
            provider.TryComputeHash(strSpan.asSpan<char, byte>(), dst, out _);
            return NConverter.fromBytesWithoutChecks<decimal>(dst);
        }

        public static bool isNullOrEmpty(this string value)
        {
            if (value != null)
            {
                return value.Length == 0;
            }
            return true;
        }

        public static string compressToDeflateString(this byte[] data, int version = 0)
        {
            return compressToDeflateString(data.AsSpan(), version);
        }
        public static string compressToDeflateString(ReadOnlySpan<byte> data, int version = 0)
        {
            using var output = new MemoryStream();
            var deflate = new DeflateStream(output, CompressionMode.Compress);
            deflate.Write(data);
            deflate.Dispose();
            var outputBytes = output.ToArray();
            return NStringBuilder.get()[':'][version][':'].Append(Convert.ToBase64String(outputBytes)).consume();
        }
        public static byte[] decompressFromDeflateString(this string str)
        {
            var segments = str.Split(':');
            if (segments.Length < 3)
            {
                throw new FormatException("Invalid deflate string format");
            }

            var base64 = segments[2];
            var compressedData = Convert.FromBase64String(base64);

            using (var input = new MemoryStream(compressedData))
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                deflate.CopyTo(output);
                var outputBytes = output.ToArray();
                return outputBytes;
            }
        }
        #endregion

        #region Collection
        public static Span<T> asSpan<T>(this List<T> self)
        {
            return self.AsSpan();
        }
        public static ReadOnlySpan<T> asReadOnlySpan<T>(this Span<T> self)
        {
            return self;
        }
        public unsafe static Span<T> asSpan<T>(void* src, int lengthInBytes) where T : unmanaged
        {
            return new Span<T>(src, lengthInBytes / sizeOf<T>());
        }
        public unsafe static Span<T> asSpan<T>(this byte[] src) where T : unmanaged
        {
            fixed (byte* ptr = src)
            {
                return new Span<T>(ptr, src.Length / sizeOf<T>());
            }
        }
        public unsafe static Span<T> asSpan<TFrom, T>(this Span<TFrom> src) where T : unmanaged where TFrom : unmanaged
        {
            fixed (TFrom* ptr = src)
            {
                return new Span<T>(ptr, src.Length * sizeOf<TFrom>() / sizeOf<T>());
            }
        }
        public unsafe static ReadOnlySpan<T> asSpan<TFrom, T>(this ReadOnlySpan<TFrom> src) where T : unmanaged where TFrom : unmanaged
        {
            fixed (TFrom* ptr = src)
            {
                return new ReadOnlySpan<T>(ptr, src.Length * sizeOf<TFrom>() / sizeOf<T>());
            }
        }
        public unsafe static byte[] toBytes<T>(this ReadOnlySpan<T> self) where T : unmanaged
        {
            var dst = new byte[self.Length * NUtils.sizeOf<T>()];
            fixed (T* srcPtr = self)
            {
                fixed (byte* dstPtr = dst)
                {
                    Buffer.MemoryCopy(srcPtr, dstPtr, dst.Length, dst.Length);
                }
            }
            return dst;
        }
        public unsafe static byte[] toBytes<T>(this T[] self) where T : unmanaged
        {
            var dst = new byte[self.Length * NUtils.sizeOf<T>()];
            fixed (T* srcPtr = self)
            {
                fixed (byte* dstPtr = dst)
                {
                    Buffer.MemoryCopy(srcPtr, dstPtr, dst.Length, dst.Length);
                }
            }
            return dst;
        }
        public static bool addIfNotPresent<T>(this ICollection<T> self, T item)
        {
            if (self.Contains(item))
            {
                return false;
            }
            self.Add(item);
            return true;
        }
        public static void addAndSort<T>(this List<T> self, T item, bool ignoreIfExist = true)
        {
            if (ignoreIfExist)
            {
                if (self.Contains(item))
                {
                    return;
                }
            }
            self.Add(item);
            self.Sort();
        }
        public static void addAndSort<T>(this List<T> self, T item, Comparison<T> comparison, bool ignoreIfExist = true)
        {
            if (ignoreIfExist)
            {
                if (self.Contains(item))
                {
                    return;
                }
            }
            self.Add(item);
            self.Sort(comparison);
        }
        public static void addRange<TCollection, T>(this ICollection<T> self, ReadOnlySpan<T> values) where TCollection : ICollection<T>
        {
            foreach (T item in values)
            {
                self.Add(item);
            }
        }
        public static void addRange<TCollection, T>(this TCollection self, IReadOnlyCollection<T> values) where TCollection : ICollection<T>
        {
            foreach (T item in values)
            {
                self.Add(item);
            }
        }

        public static T first<T>(this HashSet<T> self)
        {
            var enumerator = self.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            throw new Exception("HashSet is empty");
        }
        public static bool allEquals<TCollection, T>(this TCollection self, T value) where TCollection : ICollection<T>
        {
            foreach (var item in self)
            {
                if (!item.equals(value))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isSameItem<T>(this T[] a, T[] b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].equals(b[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isSameItem<T>(this List<T> a, List<T> b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            return isSameItem(a.asSpan(), b.asSpan());
        }
        public static bool isSameItem<T>(this Span<T> a, ReadOnlySpan<T> b)
        {
            return isSameItem((ReadOnlySpan<T>)a, b);
        }
        public static bool isSameItem<T>(this ReadOnlySpan<T> a, ReadOnlySpan<T> b)
        {
            if (a == default || b == default)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].equals(b[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool Contains<T>(this T[] self, T value)
        {
            for (int i = self.Length - 1; i >= 0; i--)
            {
                if (equals(self[i], value))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool Contains<T>(this Span<T> self, T value)
        {
            return Contains((ReadOnlySpan<T>)self, value);
        }
        public static bool Contains<T>(this ReadOnlySpan<T> self, T value)
        {
            for (int i = self.Length - 1; i >= 0; i--)
            {
                if (equals(self[i], value))
                {
                    return true;
                }
            }
            return false;
        }
        public static int IndexOf<T>(this T[] self, T value)
        {
            for (int i = self.Length - 1; i >= 0; i--)
            {
                if (equals(self[i], value))
                {
                    return i;
                }
            }
            return -1;
        }
        public static int IndexOf<T>(this Span<T> self, T value)
        {
            return IndexOf((ReadOnlySpan<T>)self, value);
        }
        public static int IndexOf<T>(this ReadOnlySpan<T> self, T value)
        {
            for (int i = self.Length - 1; i >= 0; i--)
            {
                if (equals(self[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool Contains<T>(this ICollection<T> self, ICollection<T> b)
        {
            foreach (var item in b)
            {
                if (!self.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool Contains<T>(this ICollection<T> self, ReadOnlySpan<T> b)
        {
            for (int i = 0, length = b.Length; i < length; i++)
            {
                if (!self.Contains(b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// remove items in list
        /// </summary>
        /// <typeparam name="T">is a built-in type, struct or class</typeparam>
        /// <param name="self">is list contains item which you want remove</param>
        /// <param name="indexList">index of items which you want remove</param>
        public static void removeAt<T>(this List<T> self, params int[] indices)
        {
            for (int i = indices.Length - 1; i >= 0; --i)
            {
                self.RemoveAt(indices[i]);
            }
        }

        public static void removeLast<T>(this List<T> self)
        {
            self.RemoveAt(self.Count - 1);
        }

        public static T takeAndRemoveAt<T>(this List<T> self, int index)
        {
            var item = self[index];
            self.RemoveAt(index);
            return item;
        }
        public static V takeAndRemove<K, V>(this IDictionary<K, V> self, K key)
        {
            if (!self.TryGetValue(key, out var v)) return default;
            self.Remove(key);
            return v;
        }
        public static bool tryTakeAndRemove<K, V>(this IDictionary<K, V> self, K key, out V value)
        {
            if (!self.TryGetValue(key, out value)) return false;
            self.Remove(key);
            return true;
        }

        public static V getOrAddNew<K, V>(this IDictionary<K, V> self, K key) where V : class, new()
        {
            if (!self.TryGetValue(key, out var val))
            {
                val = createInstance<V>();
                self.Add(key, val);
            }
            return val;
        }

        public static T takeAndRemoveLast<T>(this List<T> self)
        {
            var item = self[^1];
            self.removeLast();
            return item;
        }

        /// <summary>
        /// swap item to back and remove it
        /// </summary>
        public static void removeAtSwapBack<T>(this List<T> self, int index)
        {
            var lastIndex = self.Count - 1;
            self[index] = self[lastIndex];
            self.RemoveAt(lastIndex);
        }

        public static bool removeSwapBack<T>(this List<T> self, T item)
        {
            var index = self.IndexOf(item);
            if (index < 0) return false;
            self.removeAtSwapBack(index);
            return true;
        }

        public static T takeAndRemoveAtSwapBack<T>(this List<T> self, int index)
        {
            var item = self[index];
            self.removeAtSwapBack(index);
            return item;
        }

        public static T takeAndRemoveFirst<T>(this HashSet<T> self)
        {
            var item = self.first();
            self.Remove(item);
            return item;
        }
        public static T[] add<T>(this T[] self, params T[] items)
        {
            var srcLength = self.Length;
            var itemsLength = items.Length;
            var result = new T[srcLength + itemsLength];
            Array.Copy(self, 0, result, 0, srcLength);
            Array.Copy(items, 0, result, srcLength, itemsLength);
            return result;
        }
        public static T[] remove<T>(this T[] self, params T[] items)
        {
            var result = new List<T>(self.Length);
            foreach (var item in self)
            {
                if (!items.Contains(item)) result.Add(item);
            }
            return result.ToArray();
        }

        public static T[] createOrAdd<T>(this T[] self, params T[] items)
        {
            if (self == null)
            {
                return items;
            }
            else
            {
                return add(self, items);
            }
        }
        public static T[] merge<T>(params T[][] arrays)
        {
            int totalLength = 0;
            int arrLength = arrays.Length;
            for (int i = 0; i < arrLength; ++i)
            {
                totalLength += arrays[i].Length;
            }
            var data = new T[totalLength];
            int pointer = 0;
            for (int i = 0; i < arrLength; ++i)
            {
                var item = arrays[i];
                Array.Copy(item, 0, data, pointer, item.Length);
                pointer += item.Length;
            }
            return data;
        }
        public static T[] mergeNullableArrays<T>(params T[][] arrays)
        {
            int totalLength = 0;
            int arrLength = arrays.Length;
            for (int i = 0; i < arrLength; ++i)
            {
                if (arrays[i] != null)
                {
                    totalLength += arrays[i].Length;
                }
            }
            var result = new T[totalLength];
            int pointer = 0;
            for (int i = 0; i < arrLength; ++i)
            {
                var item = arrays[i];
                if (item != null)
                {
                    Array.Copy(item, 0, result, pointer, item.Length);
                    pointer += item.Length;
                }
            }
            return result;
        }
        public static T[] mergeWith<T>(this T[] left, T[] right)
        {
            var aLength = left.Length;
            var bLength = right.Length;
            var result = new T[aLength + bLength];
            Array.Copy(left, 0, result, 0, aLength);
            Array.Copy(right, 0, result, aLength, bLength);
            return result;
        }
        public static T[] getBlock<T>(T[] src, int startIndex, int count)
        {
            T[] result = new T[count];
            Array.Copy(src, startIndex, result, 0, count);
            return result;
        }
        public static T[] getBlockToEnd<T>(T[] src, int startIndex)
        {
            return getBlock(src, startIndex, src.Length - startIndex);
        }

        public static void clear(this Array self)
        {
            Array.Clear(self, 0, self.Length);
        }

        public static T[] clone<T>(this T[] self)
        {
            return clone(self, 0, self.Length);
        }
        public static T[] clone<T>(this T[] self, int startIndex, int length)
        {
            T[] result = new T[length];
            Array.Copy(self, startIndex, result, 0, length);
            return result;
        }

        public static IntPtr getIntPtr<T>(this T[] self) where T : unmanaged
        {
            return getIntPtr(self.AsSpan().asReadOnlySpan());
        }
        public static unsafe IntPtr getIntPtr<T>(this ReadOnlySpan<T> self) where T : unmanaged
        {
            fixed (T* ptr = self)
            {
                return (IntPtr)ptr;
            }
        }

        public static void quickSort<T>(Span<T> span)
        {
            if (span.Length <= 1) return;
            quickSort(span, 0, span.Length - 1);

        }

        public static void quickSort<T>(Span<T> span, int left, int right)
        {
            if (left >= right) return;

            T pivot = span[(left + right) / 2];
            int i = left;
            int j = right;

            while (i <= j)
            {
                while (span[i].compareTo(pivot) < 0) i++;
                while (span[j].compareTo(pivot) > 0) j--;

                if (i <= j)
                {
                    // Swap elements
                    (span[j], span[i]) = (span[i], span[j]);
                    i++;
                    j--;
                }
            }

            if (left < j) quickSort(span, left, j);
            if (i < right) quickSort(span, i, right);
        }

        #endregion

        #region C# Type

        public unsafe static int sizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }

        public static BindingFlags getStaticBindingFlags()
        {
            return BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        }

        public static BindingFlags getAllBindingFlags()
        {
            return (BindingFlags)(-1);
        }
        public static bool isInherited(this Type type, Type parent)
        {
            return parent.IsAssignableFrom(type);
        }
        public static object createInstance(this Type type)
        {
            return ObjectFactory.createInstance(type);
        }
        internal static class ObjectFactory
        {
            private static ConcurrentDictionary<Type, Func<object>> _ctorCache = new ConcurrentDictionary<Type, Func<object>>();
            private static class StaticFactory<T>
            {
                private static Func<T> _ctor;
                static StaticFactory()
                {
                    initialize();
                }
                public static void initialize()
                {
                    var type = typeof(T);
                    Func<object> func;
                    if (!_ctorCache.ContainsKey(type))
                    {
                        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        var ctor = type.GetConstructor(flags, null, Type.EmptyTypes, null);
                        if (ctor == null)
                        {
                            NDebug.LogWarning($"Type: {type} does not have a default constructor. Use System.Runtime.Serialization.FormatterServices.GetUninitializedObject instead.");
                            func = () => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
                            _ctorCache.TryAdd(type, func);
                            _ctor = () => (T)func();
                        }
                        else if (ctor.IsPublic)
                        {
                            func = () => Activator.CreateInstance<T>();
                            _ctorCache.TryAdd(type, func);
                            _ctor = Activator.CreateInstance<T>;
                        }
                        else
                        {
                            _ctor = () => (T)ctor.Invoke(null);
                            func = () => _ctor.Invoke();
                            _ctorCache.TryAdd(type, func);
                        }
                    }
                }
                public static T createInstance()
                {
                    return _ctor();
                }
            }
            public static T createInstance<T>()
            {
                return StaticFactory<T>.createInstance();
            }
            private static Func<object> __tryGetOrInitialize(Type type)
            {
                if (!_ctorCache.TryGetValue(type, out var func))
                {
                    lock (_ctorCache)
                    {
                        if (!_ctorCache.TryGetValue(type, out func))
                        {
                            var factoryType = typeof(StaticFactory<>).MakeGenericType(type);
                            var bindingFlags = NUtils.getStaticBindingFlags();
                            var initializeMethod = factoryType.GetMethod("initialize", bindingFlags);
                            initializeMethod.Invoke(null, null);
                            if (!_ctorCache.TryGetValue(type, out func))
                            {
                                throw new Exception($"Can't initialize ObjectFactory for {type}");
                            }
                        }
                    }
                }
                return func;
            }
            public static object createInstance(Type type)
            {
                return __tryGetOrInitialize(type).Invoke();
            }
        }
        public static T createInstance<T>()
        {
            return ObjectFactory.createInstance<T>();
        }

        public static object getDefault(this Type t)
        {
            var method = typeof(NUtils).getMethod(nameof(getDefaultGeneric));
            return method.MakeGenericMethod(t).Invoke(null, null);
        }

        public static T getDefaultGeneric<T>()
        {
            return default;
        }

        public static FieldInfo getField(this Type type, string name)
        {
            return getField(type, name, getAllBindingFlags(), true);
        }
        public static FieldInfo getField(this Type type, string name, BindingFlags bindingFlags, bool recursiveInBaseType = true)
        {
            FieldInfo fieldInfo = type.GetField(name, bindingFlags);
            if (fieldInfo == null && recursiveInBaseType)
            {
                type = type.BaseType;
                while (type != null)
                {
                    fieldInfo = type.GetField(name, bindingFlags);
                    if (fieldInfo != null)
                    {
                        break;
                    }
                    type = type.BaseType;
                }
            }
            return fieldInfo;
        }
        public static List<FieldInfo> getFields(this Type type, BindingFlags bindingFlags, bool recursiveInBaseType = true)
        {
            var results = new List<FieldInfo>();
            getFields(type, ref results, bindingFlags, recursiveInBaseType);
            return results;
        }
        public static void getFields(this Type type, ref List<FieldInfo> results, BindingFlags bindingFlags, bool recursiveInBaseType = true)
        {
            results ??= new List<FieldInfo>();
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);
            results.InsertRange(0, fieldInfos);
            if (recursiveInBaseType)
            {
                var baseType = type.BaseType;
                if (baseType != null) getFields(baseType, ref results, bindingFlags, recursiveInBaseType);
            }
        }
        public static void getMembers(this Type type, ref List<MemberInfo> results, BindingFlags bindingFlags, bool recursiveInBaseType = false)
        {
            if (recursiveInBaseType)
            {
                getMembers(type, ref results, bindingFlags, null, false);
            }
            else
            {
                getMembers(type, ref results, bindingFlags, type, false);
            }
        }
        public static void getMembers(this Type type, ref List<MemberInfo> results, BindingFlags bindingFlags, Type recursiveInBaseType, bool includeBaseType)
        {
            results ??= new List<MemberInfo>();
            var members = type.GetMembers(bindingFlags);
            results.InsertRange(0, members);

            if (type != recursiveInBaseType)
            {
                var baseType = type.BaseType;
                if (baseType != null)
                {
                    if (includeBaseType || baseType != recursiveInBaseType)
                    {
                        getMembers(baseType, ref results, bindingFlags, recursiveInBaseType, includeBaseType);
                    }
                }
            }
        }
        public static MethodInfo getMethod(this Type type, string name)
        {
            return getMethod(type, name, getAllBindingFlags(), true);
        }
        public static MethodInfo getMethod(Type type, string name, BindingFlags bindingFlags, bool recursiveInBaseType)
        {
            var fieldInfo = type.GetMethod(name, bindingFlags);
            if (fieldInfo != null) return fieldInfo;
            if (recursiveInBaseType)
            {
                var baseType = type.BaseType;
                if (baseType != null) return getMethod(baseType, name, bindingFlags, recursiveInBaseType);
            }
            return null;
        }

        public static PropertyInfo getProperty(this Type type, string name, BindingFlags bindingFlags, bool recursiveInBaseType = true)
        {
            PropertyInfo propertyInfo = type.GetProperty(name, bindingFlags);
            if (propertyInfo == null && recursiveInBaseType)
            {
                type = type.BaseType;
                while (type != null)
                {
                    propertyInfo = type.GetProperty(name, bindingFlags);
                    if (propertyInfo != null)
                    {
                        break;
                    }
                    type = type.BaseType;
                }
            }
            return propertyInfo;
        }

        /// <summary>
        /// Get value of object from name of method, field or property 
        /// </summary>
        public static object getValue(object obj, string name, bool recursiveInBaseType = true)
        {
            var objType = obj.GetType();
            var allBindingFlag = getAllBindingFlags();

            var methodInfo = getMethod(objType, name, allBindingFlag, recursiveInBaseType);
            if (methodInfo != null)
            {
                if (methodInfo.IsStatic)
                {
                    return methodInfo.Invoke(null, null);
                }
                else
                {
                    return methodInfo.Invoke(obj, null);
                }
            }
            else
            {
                var fieldInfo = getField(objType, name, allBindingFlag, recursiveInBaseType);
                if (fieldInfo != null)
                {
                    return fieldInfo.GetValue(obj);
                }
                var property = objType.getProperty(name, allBindingFlag);
                if (property != null)
                {
                    return property.GetValue(obj);
                }
                return null;
            }
        }
        public static T2 safeAs<T1, T2>(this T1 self) where T1 : unmanaged where T2 : unmanaged
        {
            return NConverter.bitConvertDiffSize<T1, T2>(self);
        }
        public static T2 fastAs<T1, T2>(this T1 self) where T1 : unmanaged where T2 : unmanaged
        {
            return NConverter.bitConvertWithoutChecks<T1, T2>(self);
        }
        #endregion

        #region Others
        public static bool equals<T>(this T self, T other)
        {
            return EqualityComparer<T>.Default.Equals(self, other);
        }
        public static int compareTo<TType>(this TType a, TType b)
        {
            return NGenericComparer<TType>.compare(a, b);
        }

        public unsafe static int unsafeCompareAsNumber<TType, TNumberType>(TType a, TType b)
            where TType : unmanaged
            where TNumberType : unmanaged, IComparable<TNumberType>
        {
            return (*(TNumberType*)&a).CompareTo(*(TNumberType*)&b);
        }

        public static void dispose(params IDisposable[] disposables)
        {
            for (int i = 0; i < disposables.Length; ++i)
            {
                disposables[i].Dispose();
            }
        }
        public static string removeExtension(this string path)
        {
            var indexOfDot = path.LastIndexOf('.');
            if (indexOfDot >= 0)
            {
                return path[..indexOfDot];
            }
            return path;
        }
        #endregion
    }
}
