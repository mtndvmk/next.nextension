using System;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public static class UnsafeUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo reinterpret<TFrom, TTo>(TFrom self)
        {
            return Unsafe.As<TFrom, TTo>(ref self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isNullRef<T>(ref this T source) where T : struct
        {
            return Unsafe.IsNullRef(ref source);
        }
    }
}
