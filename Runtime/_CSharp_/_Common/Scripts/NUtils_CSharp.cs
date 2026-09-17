using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nextension
{
    public static partial class NUtils
    {
        #region Number & Bit mask

        public static long getNum<TEnum>(TEnum value) where TEnum : unmanaged, Enum
        {
            return Unsafe.SizeOf<TEnum>() switch
            {
                1 => Unsafe.As<TEnum, byte>(ref value),
                2 => Unsafe.As<TEnum, short>(ref value),
                4 => Unsafe.As<TEnum, int>(ref value),
                8 => Unsafe.As<TEnum, long>(ref value),
                _ => throw new NotSupportedException("Invalid enum size")
            };
        }

        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        public static bool checkBitMask<TEnum>(TEnum value, int bitIndex) where TEnum : unmanaged, Enum
        {
            return NMath.checkBit1Index(getNum(value), bitIndex);
        }
        /// <summary>
        /// return true if (value & filter) is not equal 0, otherwise return false
        /// </summary>
        public static bool hasAnyFlag<T>(this T value, T filter) where T : unmanaged, Enum
        {
            var v = getNum(value);
            var f = getNum(filter);
            return (v & f) != 0;
        }

        public static int setBit0(int value, int bitIndex)
        {
            return value &= ~(1 << bitIndex);
        }

        public static long setBit0(long value, int bitIndex)
        {
            return value &= ~(1L << bitIndex);
        }

        public static void setBit0(this byte[] bytes, int bitIndex)
        {
            var byteIndex = bitIndex >> 3;
            bitIndex &= 0x7;
            bytes[byteIndex] &= (byte)~(1 << bitIndex);
        }

        public static int setBit1(int value, int bitIndex)
        {
            return value |= 1 << bitIndex;
        }

        public static long setBit1(long value, int bitIndex)
        {
            return value |= 1L << bitIndex;
        }

        public static void setBit1(this byte[] bytes, int bitIndex)
        {
            var byteIndex = bitIndex >> 3;
            bitIndex &= 0x7;
            bytes[byteIndex] |= (byte)(1 << bitIndex);
        }

        public static bool isOnly1(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }
            return isOnly1(bytes.AsSpan());
        }

        public unsafe static bool isOnly1(ReadOnlySpan<byte> bytes)
        {
            if (bytes.IsEmpty)
            {
                throw new Exception("bytes is null or empty");
            }
            return isOnly1((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in bytes[0])), bytes.Length);
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

        public static bool isOnly0(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }
            return isOnly0(bytes.AsSpan());
        }

        public unsafe static bool isOnly0(ReadOnlySpan<byte> bytes)
        {
            if (bytes.IsEmpty)
            {
                throw new Exception("bytes is null or empty");
            }
            return isOnly0((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in bytes[0])), bytes.Length);
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
#if UNITY_5_3_OR_NEWER
#else
        public static string formatToMMSS(this long totalSeconds)
        {
            var m = Math.DivRem(totalSeconds, 60, out long s);
            return $"{m:00}:{s:00}";
        }
        public static string formatToHHMMSS(this long totalSeconds)
        {
            var h = Math.DivRem(totalSeconds, 3600, out long remainingM);
            var m = Math.DivRem(remainingM, 60, out long s);
            return $"{h:00}:{m:00}:{s:00}";
        }
#endif
        public static bool isHex(this string str)
        {
            return isHex(str.AsSpan());
        }
        public static bool isHex(this ReadOnlySpan<char> span)
        {
            int offset;
            if (span[0] == '0' && (span[1] == 'x' || span[1] == 'X'))
            {
                offset = 2;
            }
            else
            {
                offset = 0;
            }
            bool isHex;
            int strLength = span.Length;
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
        public static string bytesToHex(this byte[] inData, bool include0xPrefix = false)
        {
            return bytesToHex(inData.AsSpan(), include0xPrefix);
        }

        public static string bytesToHex(this Span<byte> inData, bool include0xPrefix = false)
        {
            return bytesToHex((ReadOnlySpan<byte>)inData, include0xPrefix);
        }

        public static string bytesToHex(this ReadOnlySpan<byte> inData, bool include0xPrefix = false)
        {
            int inDataLength = inData.Length;
            int hexLength = include0xPrefix ? (inDataLength * 2 + 2) : inDataLength * 2;
            if (hexLength == 0) return "";
            char[] arr = new char[hexLength];
            int idx = 0;
            if (include0xPrefix)
            {
                arr[idx++] = '0';
                arr[idx++] = 'x';
            }
            for (int i = 0; i < inDataLength; i++)
            {
                var b = inData[i] >> 4;
                var b1 = (int)((uint)(9 - b) >> 31);
                arr[idx++] = (char)(0x30 + b + (b1 << 3) - b1);
                b = inData[i] & 0xf;
                b1 = (int)((uint)(9 - b) >> 31);
                arr[idx++] = (char)(0x30 + b + (b1 << 3) - b1);
            }
            return new string(arr);
        }
#if UNITY_5_3_OR_NEWER
#else
        public static unsafe string bytesToHex(byte* inData, int inDataLength, bool include0xPrefix = false)
        {
            int hexLength = include0xPrefix ? (inDataLength * 2 + 2) : inDataLength * 2;
            if (hexLength == 0) return "";
            char[] arr = new char[hexLength];
            int idx = 0;
            if (include0xPrefix)
            {
                arr[idx++] = '0';
                arr[idx++] = 'x';
            }
            for (int i = 0; i < inDataLength; i++)
            {
                var b = inData[i] >> 4;
                var b1 = (int)((uint)(9 - b) >> 31);
                arr[idx++] = (char)(0x30 + b + (b1 << 3) - b1);
                b = inData[i] & 0xf;
                b1 = (int)((uint)(9 - b) >> 31);
                arr[idx++] = (char)(0x30 + b + (b1 << 3) - b1);
            }
            return new string(arr);
        }
#endif
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
        private static void internal_hexToBytes(ReadOnlySpan<char> hexSpan, Span<byte> dst)
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

        private static byte[] _hexTable;
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
            if (hexChar > 255) throw new ArgumentException("Invalid hex character");

            _hexTable ??= __createHexTable();
            byte result = _hexTable[hexChar];

            if (result == 255)
                throw new ArgumentException("Invalid hex character: " + hexChar);

            return result;
        }
        /// <summary>
        /// Require hex length mod 2 == 0
        /// </summary>
        public static byte[] hexToBytes(this string hex, int startIndex = 0)
        {
            var hexLength = hex.Length;
            var hexSpan = internal_getHexNoPrefix(hex, startIndex, hexLength);

            byte[] result = new byte[hexSpan.Length >> 1];
            internal_hexToBytes(hexSpan, result.AsSpan());
            return result;
        }
        /// <summary>
        /// Require hex length mod 2 == 0
        /// </summary>
        public static byte[] hexToBytes(this ReadOnlySpan<char> hexSpan)
        {
            byte[] result = new byte[hexSpan.Length >> 1];
            internal_hexToBytes(hexSpan, result.AsSpan());
            return result;
        }
        /// <summary>
        /// Require hex length mod 2 == 0
        /// </summary>
        public static void hexToBytes(string hex, int startIndex, int hexLength, Span<byte> dst)
        {
            var hexSpan = internal_getHexNoPrefix(hex, startIndex, hexLength);
            internal_hexToBytes(hexSpan, dst);
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
        #endregion

        #region Collection

        public static Span<T> asSpan<T>(ref T self)
        {
            return MemoryMarshal.CreateSpan(ref self, 1);
        }

        public static Span<T> asSpan<T>(this List<T> self)
        {
            return self.AsSpan();
        }
        public static Span<T> asRoSpan<T>(in T self)
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in self), 1);
        }
        public static ReadOnlySpan<T> asRoSpan<T>(this Span<T> self)
        {
            return self;
        }

        public unsafe static Span<T> asSpan<T>(void* src, int lengthInBytes) where T : unmanaged
        {
            return new Span<T>(src, lengthInBytes / Unsafe.SizeOf<T>());
        }

        public static Span<T> asSpan<T>(this byte[] src) where T : unmanaged
        {
            if (src == null || src.Length == 0) return default;
            return MemoryMarshal.Cast<byte, T>(src.AsSpan());
        }

        public static Span<T> asSpan<TFrom, T>(this Span<TFrom> src) where T : unmanaged where TFrom : unmanaged
        {
            if (src.IsEmpty) return default;
            return MemoryMarshal.Cast<TFrom, T>(src);
        }

        public unsafe static ReadOnlySpan<T> asSpan<TFrom, T>(this ReadOnlySpan<TFrom> src) where T : unmanaged where TFrom : unmanaged
        {
            if (src.IsEmpty) return default;
            return new ReadOnlySpan<T>(Unsafe.AsPointer(ref Unsafe.AsRef(in src[0])), src.Length * Unsafe.SizeOf<TFrom>() / Unsafe.SizeOf<T>());
        }

        public unsafe static byte[] toBytes<T>(this ReadOnlySpan<T> self) where T : unmanaged
        {
            var dst = new byte[self.Length * Unsafe.SizeOf<T>()];
            if (dst.Length > 0)
            {
                var srcPtr = Unsafe.AsPointer(ref Unsafe.AsRef(in self[0]));
                var dstPtr = Unsafe.AsPointer(ref dst[0]);
                Buffer.MemoryCopy(srcPtr, dstPtr, dst.Length, dst.Length);
            }
            return dst;
        }

        public static byte[] toBytes<T>(this T[] self) where T : unmanaged
        {
            if (self == null || self.Length == 0) return Array.Empty<byte>();
            return toBytes<T>(self.AsSpan());
        }

        public static bool addIfNotPresent<TCollection, T>(this TCollection self, T item) where TCollection : ICollection<T>
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
            for (int i = 0, length = values.Length; i < length; i++)
            {
                self.Add(values[i]);
            }
        }

        public static void addRange<TCollection, T>(this TCollection self, IEnumerable<T> values) where TCollection : ICollection<T>
        {
            if (values is IList<T> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    self.Add(list[i]);
                }
                return;
            }
            if (values is IReadOnlyList<T> roList)
            {
                for (int i = 0, count = roList.Count; i < count; i++)
                {
                    self.Add(roList[i]);
                }
                return;
            }
            foreach (T item in values)
            {
                self.Add(item);
            }
        }

        public static T[] add<T>(this T[] self, T item)
        {
            int len = self?.Length ?? 0;
            Array.Resize(ref self, len + 1);
            self[^1] = item;
            return self;
        }

        public static T[] remove<T>(this T[] self, T item)
        {
            if (self == null || self.Length == 0)
            {
                return self;
            }
            int index = Array.IndexOf(self, item);
            if (index < 0)
            {
                return self;
            }
            T[] newArray = new T[self.Length - 1];
            if (index > 0)
            {
                Array.Copy(self, 0, newArray, 0, index);
            }
            if (index < self.Length - 1)
            {
                Array.Copy(self, index + 1, newArray, index, self.Length - index - 1);
            }
            return newArray;
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
            if (self is IList<T> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    if (!list[i].equals(value))
                    {
                        return false;
                    }
                }
                return true;
            }
            if (self is IReadOnlyList<T> roList)
            {
                for (int i = 0, count = roList.Count; i < count; i++)
                {
                    if (!roList[i].equals(value))
                    {
                        return false;
                    }
                }
                return true;
            }
            foreach (var item in self)
            {
                if (!item.equals(value))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool sequenceEqual<T>(this T[] a, T[] b) where T : IEquatable<T>
        {
            if (a == null || b == null)
            {
                return false;
            }
            return a.AsSpan().SequenceEqual(b.AsSpan());
        }

        public static bool sequenceEqual<T>(this List<T> a, List<T> b) where T : IEquatable<T>
        {
            if (a == null || b == null)
            {
                return false;
            }
            return a.asSpan().SequenceEqual(b.asSpan());
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

        public static V takeAndRemove<K, V>(this Dictionary<K, V> self, K key)
        {
            if (!self.TryGetValue(key, out var v)) return default;
            self.Remove(key);
            return v;
        }

        public static bool tryTakeAndRemove<K, V>(this Dictionary<K, V> self, K key, out V value)
        {
            if (!self.TryGetValue(key, out value)) return false;
            self.Remove(key);
            return true;
        }

        public static V getOrAddNew<K, V>(this Dictionary<K, V> self, K key) where V : class, new()
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
            return getIntPtr(self.AsSpan().asRoSpan());
        }

        public static unsafe IntPtr getIntPtr<T>(this ReadOnlySpan<T> self) where T : unmanaged
        {
            if (self.IsEmpty) return IntPtr.Zero;
            return (IntPtr)Unsafe.AsPointer(ref Unsafe.AsRef(in self[0]));
        }

        public static void setSize<T>(this List<T> list, int count)
        {
            int current = list.Count;
            if (current == count) return;
            if (current < count)
            {
                if (list.Capacity < count) list.Capacity = count;
                while (current < count)
                {
                    list.Add(default);
                    current++;
                }
            }
            else
            {
                list.RemoveRange(count, current - count);
            }
        }

        public static void setSize<T, TList>(this TList list, int count) where TList : IList<T>
        {
            int current = list.Count;
            if (current == count) return;

            if (list is List<T> sysList)
            {
                setSize(sysList, count);
                return;
            }
            if (list is NList<T> nList)
            {
                nList.SetCount(count);
                return;
            }

            if (current < count)
            {
                while (current < count)
                {
                    list.Add(default);
                    current++;
                }
            }
            else
            {
                while (current > count)
                {
                    list.RemoveAt(current - 1);
                    current--;
                }
            }
        }

        #endregion

        #region C# Type

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
            return NConverter.bitConvertSizeChecks<T1, T2>(self);
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
            return Comparer<TType>.Default.Compare(a, b);
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
