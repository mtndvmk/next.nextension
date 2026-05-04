using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    internal static class NGenericComparer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int compare(T a, T b)
        {
            return _compareFunc(a, b);
        }
        private static readonly Func<T, T, int> _compareFunc = createCompareFunc();
        private static Func<T, T, int> createCompareFunc()
        {
            var typeOfT = typeof(T);
            var bindingFlags = NUtils.getStaticBindingFlags();
            if (typeof(IComparable<T>).IsAssignableFrom(typeOfT))
            {
                var method = typeof(ComparableGenericComparer<>).MakeGenericType(typeOfT).GetMethod("compare", bindingFlags);
                return (Func<T, T, int>)method.CreateDelegate(typeof(Func<T, T, int>));
            }
            if (typeOfT.IsEnum)
            {
                var method = typeof(EnumComparer<>).MakeGenericType(typeOfT).GetMethod("getCompareFunc", bindingFlags);
                return (Func<T, T, int>)method.Invoke(null, null);
            }
            if (typeof(IComparable).IsAssignableFrom(typeOfT))
            {
                var method = typeof(ComparableComparer<>).MakeGenericType(typeOfT).GetMethod("compare", bindingFlags);
                return (Func<T, T, int>)method.CreateDelegate(typeof(Func<T, T, int>));
            }
            return Comparer<T>.Default.Compare;
        }
    }

    internal static class ComparableGenericComparer<T> where T : IComparable<T>
    {
        public static int compare(T a, T b)
        {
            return a.CompareTo(b);
        }
    }
    internal static class ComparableComparer<T> where T : IComparable
    {
        public static int compare(T a, T b)
        {
            return a.CompareTo(b);
        }
    }
    internal static class EnumComparer<T> where T : unmanaged, Enum
    {
        public static int compare(T a, T b)
        {
            return NGenericComparer<T>.compare(a, b);
        }
        public static Func<T, T, int> getCompareFunc()
        {
            var typeCode = Type.GetTypeCode(Enum.GetUnderlyingType(typeof(T)));
            return typeCode switch
            {
                TypeCode.SByte => NUtils.unsafeCompareAsNumber<T, sbyte>,
                TypeCode.Byte => NUtils.unsafeCompareAsNumber<T, byte>,
                TypeCode.Int16 => NUtils.unsafeCompareAsNumber<T, short>,
                TypeCode.UInt16 => NUtils.unsafeCompareAsNumber<T, ushort>,
                TypeCode.Int32 => NUtils.unsafeCompareAsNumber<T, int>,
                TypeCode.UInt32 => NUtils.unsafeCompareAsNumber<T, uint>,
                TypeCode.Int64 => NUtils.unsafeCompareAsNumber<T, long>,
                TypeCode.UInt64 => NUtils.unsafeCompareAsNumber<T, ulong>,
                _ => throw new NotSupportedException(typeCode.ToString())
            };
        }
    }
}