using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.LowLevel;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

namespace Nextension
{
    public static class NUtils
    {
        #region Number & Bit mask
        [BurstCompile]
        public static bool isPOT(float n)
        {
            int int_n = (int)n;
            if (n - int_n > Mathf.Epsilon)
            {
                return false;
            }
            return isPOT(int_n);
        }
        [BurstCompile]
        public static bool isPOT(int n)
        {
            return (n & (n - 1)) == 0 && n > 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [BurstCompile]
        public static unsafe bool checkBitMask<T>(T mask, int bitIndex) where T : unmanaged, Enum
        {
            var intOfEnum = *(int*)&mask;
            return checkBitMask(intOfEnum, bitIndex);
        }
        /// <summary>
        /// return true if (mask & filter) is not equal 0, otherwise return false
        /// </summary>
        [BurstCompile]
        public static unsafe bool checkMask<T>(T mask, T filter) where T : unmanaged, Enum
        {
            return ((*(int*)&mask) & (*(int*)&filter)) != 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [BurstCompile]
        public static bool checkBitMask(int mask, int bitIndex)
        {
            return (mask & (1 << bitIndex)) != 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [BurstCompile]
        public static bool checkBitMask(long longMask, int bitIndex)
        {
            return (longMask & (1L << bitIndex)) != 0;
        }
        [BurstCompile]
        public static bool checkBitMask(byte[] byteMask, int bitIndex)
        {
            int byteIndex = bitIndex >> 3;
            int maskIndex = bitIndex & 0x7;
            byte mask = byteMask[byteIndex];
            return (mask & 1 << maskIndex) != 0;
        }
        [BurstCompile]
        public static bool checkBitMask(NativeArray<byte> byteMask, int bitIndex)
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
        public unsafe static int getBit1Index(NativeArray<byte> byteMask)
        {
            if (byteMask == null || byteMask.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }
            var ptr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(byteMask);
            return getBit1Index(ptr, byteMask.Length);
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
        public unsafe static int getBit0Index(NativeArray<byte> byteMask)
        {
            if (byteMask == null || byteMask.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }
            var ptr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(byteMask);
            return getBit0Index(ptr, byteMask.Length);
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
        [BurstCompile]

        public static void setBit0(this NativeArray<byte> bytes, int bitIndex)
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
        [BurstCompile]

        public static void setBit1(this NativeArray<byte> bytes, int bitIndex)
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
        public unsafe static bool isOnly1(this NativeArray<byte> bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }
            var ptr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(bytes);
            return isOnly1(ptr, bytes.Length);
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
        public unsafe static bool isOnly0(this NativeArray<byte> bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("bytes is null or empty");
            }

            var ptr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(bytes);
            return isOnly0(ptr, bytes.Length);
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

        #region Unity Transform and Point

        #region Vector2

        public static bool hasZeroAxis(this Vector2 vector2)
        {
            return vector2.x == 0 || vector2.y == 0;
        }
        public static Vector2 setX(this Vector2 vector2, float x)
        {
            vector2.x = x;
            return vector2;
        }
        public static Vector2 setY(this Vector2 vector2, float y)
        {
            vector2.y = y;
            return vector2;
        }
        public static Vector2 plusX(this Vector2 vector2, float x)
        {
            vector2.x += x;
            return vector2;
        }
        public static Vector2 plusY(this Vector2 vector2, float y)
        {
            vector2.y += y;
            return vector2;
        }
        public static Vector2 plusXY(this Vector2 vector2, float x, float y)
        {
            vector2.x += x;
            vector2.y += y;
            return vector2;
        }
        public static Vector2 plusXY(this Vector2 vector2, float xy)
        {
            vector2.x += xy;
            vector2.y += xy;
            return vector2;
        }

        public static Vector2 mul(this Vector2 vector2, Vector2 factor)
        {
            return new Vector2(vector2.x * factor.x, vector2.y * factor.y);
        }
        public static Vector2 mul(this Vector2 vector2, float x, float y)
        {
            return new Vector2(vector2.x * x, vector2.y * y);
        }
        public static Vector2 mulX(this Vector2 vector2, float x)
        {
            return new Vector2(vector2.x * x, vector2.y);
        }
        public static Vector2 mulY(this Vector2 vector2, float y)
        {
            return new Vector2(vector2.x, vector2.y * y);
        }
        public static Vector2 div(this Vector2 vector2, Vector2 factor)
        {
            return new Vector2(vector2.x / factor.x, vector2.y / factor.y);
        }
        public static Vector2 div(this Vector2 vector2, float x, float y)
        {
            return new Vector2(vector2.x / x, vector2.y / y);
        }
        public static Vector2 divX(this Vector2 vector2, float x)
        {
            return new Vector2(vector2.x / x, vector2.y);
        }
        public static Vector2 divY(this Vector2 vector2, float y)
        {
            return new Vector2(vector2.x, vector2.y / y);
        }
        public static Vector2 findNearest(ReadOnlySpan<Vector2> fromSpan, Vector2 dst)
        {
            int fromCount = fromSpan.Length;
            if (fromCount == 0) throw new Exception("fromSpan is empty");
            Vector2 result = fromSpan[0];
            float min = (result - dst).sqrMagnitude;
            for (int i = 1; i < fromCount; i++)
            {
                var from = fromSpan[i];
                var dt = (from - dst).sqrMagnitude;
                if (dt < min)
                {
                    result = from;
                    min = dt;
                }
            }
            return result;
        }
        public static Vector2 findNearest(ICollection<Vector2> fromCollection, Vector2 dst)
        {
            float min = float.MaxValue;
            Vector2 result = default;
            foreach (var from in fromCollection)
            {
                var dt = (from - dst).sqrMagnitude;
                if (dt < min)
                {
                    min = dt;
                    result = from;
                }
            }
            return result;
        }

        [BurstCompile]
        public static float maxAxis(this Vector2 vector2)
        {
            return Mathf.Max(vector2.x, vector2.y);
        }
        #endregion

        #region Vector3

        public static bool hasZeroAxis(this Vector3 vector3)
        {
            return vector3.x == 0 || vector3.y == 0 || vector3.z == 0;
        }

        public static Vector3 setX(this Vector3 vector3, float x)
        {
            vector3.x = x;
            return vector3;
        }

        public static Vector3 setY(this Vector3 vector3, float y)
        {
            vector3.y = y;
            return vector3;
        }

        public static Vector3 setZ(this Vector3 vector3, float z)
        {
            vector3.z = z;
            return vector3;
        }
        public static Vector3 setXYZ(this Vector3 vector3, float x = float.NaN, float y = float.NaN, float z = float.NaN)
        {
            if (!x.Equals(float.NaN))
            {
                vector3.x = x;
            }
            if (!y.Equals(float.NaN))
            {
                vector3.y = y;
            }
            if (!z.Equals(float.NaN))
            {
                vector3.z = z;
            }
            return vector3;
        }

        public static Vector3 plusX(this Vector3 vector3, float x)
        {
            vector3.x += x;
            return vector3;
        }

        public static Vector3 plusY(this Vector3 vector3, float y)
        {
            vector3.y += y;
            return vector3;
        }

        public static Vector3 plusZ(this Vector3 vector3, float z)
        {
            vector3.z += z;
            return vector3;
        }
        public static Vector3 plusXYZ(this Vector3 vector3, float xyz)
        {
            vector3.x += xyz;
            vector3.y += xyz;
            vector3.z += xyz;
            return vector3;
        }
        public static Vector3 plusXYZ(this Vector3 vector3, float x, float y, float z)
        {
            vector3.x += x;
            vector3.y += y;
            vector3.z += z;
            return vector3;
        }

        public static Vector3 mulX(this Vector3 vector3, float x)
        {
            vector3.x *= x;
            return vector3;
        }

        public static Vector3 mulY(this Vector3 vector3, float y)
        {
            vector3.y *= y;
            return vector3;
        }

        public static Vector3 mulZ(this Vector3 vector3, float z)
        {
            vector3.z *= z;
            return vector3;
        }
        public static Vector3 mulXYZ(this Vector3 vector3, float x = float.NaN, float y = float.NaN, float z = float.NaN)
        {
            if (!x.Equals(float.NaN))
            {
                vector3.x *= x;
            }
            if (!y.Equals(float.NaN))
            {
                vector3.y *= y;
            }
            if (!z.Equals(float.NaN))
            {
                vector3.z *= z;
            }
            return vector3;
        }
        /// <summary>
        /// return (vector3.x * x, vector3.y * y, vector3.z * z)
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 mul(this Vector3 vector3, float x, float y, float z)
        {
            return new Vector3(vector3.x * x, vector3.y * y, vector3.z * z);
        }
        /// <summary>
        /// return (vector3.x * factor.x, vector3.y * factor.y, vector3.z * factor.z)
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Vector3 mul(this Vector3 vector3, Vector3 factor)
        {
            return new Vector3(vector3.x * factor.x, vector3.y * factor.y, vector3.z * factor.z);
        }
        /// <summary>
        /// return (vector3.x / x, vector3.y / y, vector3.z / z)
        /// </summary>
        public static Vector3 div(this Vector3 vector3, float x, float y, float z)
        {
            return new Vector3(vector3.x / x, vector3.y / y, vector3.z / z);
        }
        /// <summary>
        /// return (vector3.x / factor.x, vector3.y / factor.y, vector3.z / factor.z)
        /// </summary>
        public static Vector3 div(this Vector3 vector3, Vector3 dividedFactor)
        {
            return new Vector3(vector3.x / dividedFactor.x, vector3.y / dividedFactor.y, vector3.z / dividedFactor.z);
        }
        public static Vector3 findNearest(ReadOnlySpan<Vector3> fromSpan, Vector3 dst)
        {
            int fromCount = fromSpan.Length;
            if (fromCount == 0) throw new Exception("fromSpan is empty");
            Vector3 result = fromSpan[0];
            float min = (result - dst).sqrMagnitude;
            for (int i = 1; i < fromCount; i++)
            {
                var from = fromSpan[i];
                var dt = (from - dst).sqrMagnitude;
                if (dt < min)
                {
                    result = from;
                    min = dt;
                }
            }
            return result;
        }
        public static Vector3 findNearest(ICollection<Vector3> fromCollection, Vector3 dst)
        {
            float min = float.MaxValue;
            Vector2 result = default;
            foreach (var from in fromCollection)
            {
                var dt = (from - dst).sqrMagnitude;
                if (dt < min)
                {
                    min = dt;
                    result = from;
                }
            }
            return result;
        }

        [BurstCompile]
        public static float maxAxis(this Vector3 vector3)
        {
            return Mathf.Max(vector3.x, vector3.y, vector3.z);
        }
        #endregion

        #region Vector4 & Quaternion

        [BurstCompile]
        public static Vector4 toVector4(this Quaternion quaternion)
        {
            return NConverter.bitConvertWithoutChecks<Quaternion, Vector4>(quaternion);
        }

        [BurstCompile]
        public static Quaternion toQuaternion(this Vector4 vector4)
        {
            return NConverter.bitConvertWithoutChecks<Vector4, Quaternion>(vector4);
        }

        [BurstCompile]
        public static float4 toFloat4(this Quaternion quaternion)
        {
            return NConverter.bitConvertWithoutChecks<Quaternion, float4>(quaternion);
        }

        [BurstCompile]
        public static Quaternion toQuaternion(this float4 f4)
        {
            return NConverter.bitConvertWithoutChecks<float4, Quaternion>(f4);
        }

        [BurstCompile]
        public static float maxAxis(this Vector4 vector4)
        {
            return Mathf.Max(vector4.x, vector4.y, vector4.z, vector4.w);
        }

        [BurstCompile]
        public static Vector3 up(this Quaternion quaternion)
        {
            return quaternion * Vector3.up;
        }

        [BurstCompile]
        public static Vector3 right(this Quaternion quaternion)
        {
            return quaternion * Vector3.right;
        }

        [BurstCompile]
        public static Vector3 forward(this Quaternion quaternion)
        {
            return quaternion * Vector3.forward;
        }
        #endregion

        #region Transform & Rect Transform
        public static void setPositionX(this Transform self, float x, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = self.localPosition.setX(x);
            }
            else
            {
                self.position = self.position.setX(x);
            }
        }
        public static void setPositionY(this Transform self, float y, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = self.localPosition.setY(y);
            }
            else
            {
                self.position = self.position.setY(y);
            }
        }
        public static void setPositionZ(this Transform self, float z, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = self.localPosition.setZ(z);
            }
            else
            {
                self.position = self.position.setZ(z);
            }
        }
        public static void setAnchorMinX(this RectTransform self, float x)
        {
            self.anchorMin = self.anchorMin.setX(x);
        }
        public static void setAnchorMinY(this RectTransform self, float y)
        {
            self.anchorMin = self.anchorMin.setY(y);
        }
        public static void setAnchorMaxX(this RectTransform self, float x)
        {
            self.anchorMax = self.anchorMax.setX(x);
        }
        public static void setAnchorMaxY(this RectTransform self, float y)
        {
            self.anchorMax = self.anchorMax.setY(y);
        }
        public static void setAnchorX(this RectTransform self, float x)
        {
            self.anchorMin = self.anchorMin.setX(x);
            self.anchorMax = self.anchorMax.setX(x);
        }
        public static void setAnchorY(this RectTransform self, float y)
        {
            self.anchorMin = self.anchorMin.setY(y);
            self.anchorMax = self.anchorMax.setY(y);
        }
        public static void setAnchorPositionX(this RectTransform self, float x)
        {
            self.anchoredPosition = self.anchoredPosition.setX(x);
        }
        public static void setAnchorPositionY(this RectTransform self, float y)
        {
            self.anchoredPosition = self.anchoredPosition.setY(y);
        }
        public static void plusPositionX(this Transform self, float x, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = self.localPosition.plusX(x);
            }
            else
            {
                self.position = self.position.plusX(x);
            }
        }
        public static void plusPositionY(this Transform self, float y, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = self.localPosition.plusY(y);
            }
            else
            {
                self.position = self.position.plusY(y);
            }
        }
        public static void plusPositionZ(this Transform self, float z, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = self.localPosition.plusZ(z);
            }
            else
            {
                self.position = self.position.plusZ(z);
            }
        }
        public static void plusPositionXYZ(this Transform self, float x, float y, float z, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = self.localPosition.plusXYZ(x, y, z);
            }
            else
            {
                self.position = self.position.plusXYZ(x, y, z);
            }
        }
        public static void setEulerAnglesX(this Transform self, float x, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localEulerAngles = self.localEulerAngles.setX(x);
            }
            else
            {
                self.eulerAngles = self.eulerAngles.setX(x);
            }
        }
        public static void setEulerAnglesY(this Transform self, float y, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localEulerAngles = self.localEulerAngles.setY(y);
            }
            else
            {
                self.eulerAngles = self.eulerAngles.setY(y);
            }
        }
        public static void setEulerAnglesZ(this Transform self, float z, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localEulerAngles = self.localEulerAngles.setZ(z);
            }
            else
            {
                self.eulerAngles = self.eulerAngles.setZ(z);
            }
        }
        public static void setScaleX(this Transform self, float x)
        {
            self.localScale = self.localScale.setX(x);
        }
        public static void setScaleY(this Transform self, float y)
        {
            self.localScale = self.localScale.setY(y);
        }
        public static void setScaleZ(this Transform self, float z)
        {
            self.localScale = self.localScale.setZ(z);
        }
        public static void setScale(this Transform self, float uniformScale)
        {
            self.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
        }

        public static RectTransform asRectTransform(this Transform self)
        {
            return self as RectTransform;
        }
        public static RectTransform rectTransform(this GameObject self)
        {
            return self.transform as RectTransform;
        }
        public static RectTransform rectTransform(this Component self)
        {
            return self.transform as RectTransform;
        }
        public static void markLayoutForRebuild(this GameObject self)
        {
            if (self.transform is RectTransform rectTf)
            {
                markLayoutForRebuild(rectTf);
            }
        }
        public static void markLayoutForRebuild(this Component self)
        {
            if (self.transform is RectTransform rectTf)
            {
                markLayoutForRebuild(rectTf);
            }
        }
        public static void markLayoutForRebuild(this Component self, bool isImmediate)
        {
            if (self.transform is RectTransform rectTf)
            {
                markLayoutForRebuild(rectTf, isImmediate);
            }
        }
        public static void markLayoutForRebuild(this RectTransform self)
        {
            RectTransformRebuildList.add(self);
        }
        public static void markLayoutForRebuild(this RectTransform self, bool isImmediate)
        {
            if (isImmediate)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(self);
            }
            else
            {
                RectTransformRebuildList.add(self);
            }
        }
        public static void resetPosition(this Transform self, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = Vector3.zero;
            }
            else
            {
                self.position = Vector3.zero;
            }
        }
        public static void resetAnchorPosition(this RectTransform self)
        {
            self.anchoredPosition = Vector2.zero;
        }
        public static void resetRotation(this Transform self, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localRotation = Quaternion.identity;
            }
            else
            {
                self.rotation = Quaternion.identity;
            }
        }
        public static void resetScale(this Transform self)
        {
            self.localScale = Vector3.one;
        }
        public static void setLossyScale(this Transform self, Vector3 lossyScale)
        {
            var parentLossyScale = self.parent != null ? self.parent.lossyScale : Vector3.one;
            var x = parentLossyScale.x == 0 ? 0 : lossyScale.x / parentLossyScale.x;
            var y = parentLossyScale.y == 0 ? 0 : lossyScale.y / parentLossyScale.y;
            var z = parentLossyScale.z == 0 ? 0 : lossyScale.z / parentLossyScale.z;
            self.localScale = new Vector3(x, y, z);
        }
        public static void resetPosAndRot(this Transform self, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = Vector3.zero;
                self.localRotation = Quaternion.identity;
            }
            else
            {
                self.position = Vector3.zero;
                self.rotation = Quaternion.identity;
            }
        }
        public static void resetPosAndScale(this Transform self, bool isLocal = true)
        {
            if (isLocal)
            {
                self.localPosition = Vector3.zero;
            }
            else
            {
                self.position = Vector3.zero;
            }
            self.localScale = Vector3.one;
        }
        public static void resetTransform(this Transform self, bool isLocal = true)
        {
            if (!isLocal)
            {
                self.position = Vector3.zero;
                self.eulerAngles = Vector3.zero;
                self.localScale = Vector3.one;
            }
            else
            {
                self.localPosition = Vector3.zero;
                self.localEulerAngles = Vector3.zero;
                self.localScale = Vector3.one;
            }
        }
        public static void resetTransform(this Transform self, Transform parent)
        {
            self.SetParent(parent);
            self.localPosition = Vector3.zero;
            self.localEulerAngles = Vector3.zero;
            self.localScale = Vector3.one;
        }


        [BurstCompile]
        public static Vector2 getBottomLeft(this Rect rect)
        {
            float x = rect.x;
            float y = rect.y;
            return new Vector2(x, y);
        }

        [BurstCompile]
        public static Vector2 getTopLeft(this Rect rect)
        {
            float x = rect.x;
            float yMax = rect.yMax;
            return new Vector2(x, yMax);
        }

        [BurstCompile]
        public static Vector2 getTopRight(this Rect rect)
        {
            float xMax = rect.xMax;
            float yMax = rect.yMax;
            return new Vector2(xMax, yMax);
        }

        [BurstCompile]
        public static Vector2 getBottomRight(this Rect rect)
        {
            float y = rect.y;
            float xMax = rect.xMax;
            return new Vector2(xMax, y);
        }

        public static Vector3 getBottomLeft(this RectTransform self, bool isWorldSpace = true)
        {
            Rect rect = self.rect;
            float x = rect.x;
            float y = rect.y;
            var point = new Vector3(x, y, 0f);
            if (isWorldSpace)
            {
                return self.TransformPoint(point);
            }
            else
            {
                return point;
            }
        }
        public static Vector3 getTopLeft(this RectTransform self, bool isWorldSpace = true)
        {
            Rect rect = self.rect;
            float x = rect.x;
            float yMax = rect.yMax;
            var point = new Vector3(x, yMax, 0f);
            if (isWorldSpace)
            {
                return self.TransformPoint(point);
            }
            else
            {
                return point;
            }
        }
        public static Vector3 getTopRight(this RectTransform self, bool isWorldSpace = true)
        {
            Rect rect = self.rect;
            float xMax = rect.xMax;
            float yMax = rect.yMax;
            var point = new Vector3(xMax, yMax, 0f);
            if (isWorldSpace)
            {
                return self.TransformPoint(point);
            }
            else
            {
                return point;
            }
        }
        public static Vector3 getBottomRight(this RectTransform self, bool isWorldSpace = true)
        {
            Rect rect = self.rect;
            float y = rect.y;
            float xMax = rect.xMax;
            var point = new Vector3(xMax, y, 0f);
            if (isWorldSpace)
            {
                return self.TransformPoint(point);
            }
            else
            {
                return point;
            }
        }
        public static Rect getRectInParentSpace(this RectTransform self)
        {
            Rect result = self.rect;
            Vector2 vector = self.offsetMin + Vector2.Scale(self.pivot, result.size);
            if (self.parent is RectTransform component)
            {
                vector += Vector2.Scale(self.anchorMin, component.rect.size);
            }
            result.x += vector.x;
            result.y += vector.y;
            return result;
        }
        public static void setPivotWithoutChangePosition(this RectTransform rectTransform, Vector2 pivot)
        {
            Vector2 size = rectTransform.rect.size;
            Vector2 deltaPivot = rectTransform.pivot - pivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }
        public static void setSizeWithCurrentAnchors(this RectTransform rectTransform, Vector2 size)
        {
            rectTransform.setSizeWithCurrentAnchors(size.x, size.y);
        }
        public static void setSizeWithCurrentAnchors(this RectTransform rectTransform, float width, float height)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        public static void setSizeFitToOther(this RectTransform rectTransform, RectTransform other)
        {
            var size = other.rect.size;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
        public static void stretchToParent(this RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }
        public static void anchorToParent(this RectTransform self)
        {
            if (self.parent.isNull())
            {
                return;
            }

            var rect = self.rect;
            var parentRect = self.parent.asRectTransform().rect;

            float anchorXMin;
            float anchorYMin;
            float anchorXMax;
            float anchorYMax;

            if (parentRect.width == 0)
            {
                Debug.LogWarning("width is 0", self);
                anchorXMin = self.anchorMin.x;
                anchorXMax = self.anchorMax.x;
            }
            else
            {
                anchorXMin = (rect.xMin - parentRect.x + self.localPosition.x) / parentRect.width;
                anchorXMax = (rect.xMax - parentRect.x + self.localPosition.x) / parentRect.width;
            }

            if (parentRect.height == 0)
            {
                Debug.LogWarning("height is 0", self);
                anchorYMin = self.anchorMin.y;
                anchorYMax = self.anchorMax.y;
            }
            else
            {
                anchorYMin = (rect.yMin - parentRect.y + self.localPosition.y) / parentRect.height;
                anchorYMax = (rect.yMax - parentRect.y + self.localPosition.y) / parentRect.height;
            }

            self.anchorMin = new Vector2(anchorXMin, anchorYMin);
            self.anchorMax = new Vector2(anchorXMax, anchorYMax);
            self.anchoredPosition = Vector2.zero;
            self.sizeDelta = Vector2.zero;
        }
        #endregion

        #region Camera matrix

        public static Matrix4x4 getWorldToViewportMatrix(Camera camera)
        {
            return camera.projectionMatrix * camera.worldToCameraMatrix;
        }

        public static Matrix4x4 getViewportToWorldMatrix(Camera camera)
        {
            return Matrix4x4.Inverse(getWorldToViewportMatrix(camera));
        }


        public static Vector2 viewportToScreenPoint(Vector2 viewportPoint)
        {
            return new Vector2(viewportPoint.x * Screen.width, viewportPoint.y * Screen.height);
        }

        public static Vector2 screenToViewportPoint(Vector2 screenPoint)
        {
            return new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
        }

        public static Vector2 worldToViewportPoint(Matrix4x4 world2ViewportMatrix, Vector3 worldPoint)
        {
            var clipPoint = world2ViewportMatrix.MultiplyPoint3x4(worldPoint);
            float num = world2ViewportMatrix.m30 * worldPoint.x + world2ViewportMatrix.m31 * worldPoint.y + world2ViewportMatrix.m32 * worldPoint.z + world2ViewportMatrix.m33;
            if (num != 0)
            {
                num = 1 / num;
                clipPoint.x *= num;
                clipPoint.y *= num;
                clipPoint.z *= num;
            }
            return new Vector2(clipPoint.x + 1f, clipPoint.y + 1f) / 2f;
        }
        public static Vector2 worldToScreenPoint(Matrix4x4 world2ViewportMatrix, Vector3 worldPoint)
        {
            var viewportPoint = worldToViewportPoint(world2ViewportMatrix, worldPoint);
            return new Vector2(viewportPoint.x * Screen.width, viewportPoint.y * Screen.height);
        }

        public static Vector3 viewportToWorldPoint(Matrix4x4 viewport2WorldMatrix, Vector2 viewportPoint)
        {
            var clipPoint = viewportPoint * 2f - Vector2.one;
            return viewport2WorldMatrix.MultiplyPoint(clipPoint);
        }
        public static Vector3 screenToWorldPoint(Matrix4x4 viewport2WorldMatrix, Vector2 screenPoint)
        {
            var viewportPoint = screenToViewportPoint(screenPoint);
            return viewportToWorldPoint(viewport2WorldMatrix, viewportPoint);
        }


        public static Matrix4x4 getWorldToProjectionMatrix(Matrix4x4 projectionMatrix, Matrix4x4 worldToCameraMatrix)
        {
            return projectionMatrix * worldToCameraMatrix;
        }

        public static bool isPointInsideFrustum(Plane plane0, Plane plane1, Plane plane2, Plane plane3, Plane plane4, Plane plane5, Vector3 point)
        {
            if (!plane0.GetSide(point)) return false;
            if (!plane1.GetSide(point)) return false;
            if (!plane2.GetSide(point)) return false;
            if (!plane3.GetSide(point)) return false;
            if (!plane4.GetSide(point)) return false;
            if (!plane5.GetSide(point)) return false;
            return true;
        }
        public static bool isBehindPlaneAABB(Plane plane, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p5, Vector3 p6, Vector3 p7)
        {
            if (plane.GetSide(p0) == true) return false;
            if (plane.GetSide(p1) == true) return false;
            if (plane.GetSide(p2) == true) return false;
            if (plane.GetSide(p3) == true) return false;
            if (plane.GetSide(p4) == true) return false;
            if (plane.GetSide(p5) == true) return false;
            if (plane.GetSide(p6) == true) return false;
            if (plane.GetSide(p7) == true) return false;
            return true;
        }
        public static void calculateFrustumPlanes(Matrix4x4 worldToProjectionMatrix, NativeArray<Plane> planes)
        {
            if (planes == null || planes.Length < 6) throw new ArgumentException("planes must be an array of length >= 6");

            // rows of matrix
            var r0 = new Vector4(worldToProjectionMatrix.m00, worldToProjectionMatrix.m01, worldToProjectionMatrix.m02, worldToProjectionMatrix.m03);
            var r1 = new Vector4(worldToProjectionMatrix.m10, worldToProjectionMatrix.m11, worldToProjectionMatrix.m12, worldToProjectionMatrix.m13);
            var r2 = new Vector4(worldToProjectionMatrix.m20, worldToProjectionMatrix.m21, worldToProjectionMatrix.m22, worldToProjectionMatrix.m23);
            var r3 = new Vector4(worldToProjectionMatrix.m30, worldToProjectionMatrix.m31, worldToProjectionMatrix.m32, worldToProjectionMatrix.m33);

            planes[0] = makePlane(r3 + r0); // Left
            planes[1] = makePlane(r3 - r0); // Right
            planes[2] = makePlane(r3 + r1); // Bottom
            planes[3] = makePlane(r3 - r1); // Top
            planes[4] = makePlane(r3 + r2); // Near
            planes[5] = makePlane(r3 - r2); // Far
        }

        public static Plane makePlane(in Vector4 p)
        {
            var n = new Vector3(p.x, p.y, p.z);
            float d = p.w / n.magnitude;

            var plane = new Plane
            {
                normal = n.normalized,
                distance = d
            };
            return plane;
        }
        public static CullType isInFrustum(Matrix4x4 worldToProjectionMatrix, Bounds b)
        {
            var planes = new NativeArray<Plane>(6, Allocator.Temp);
            calculateFrustumPlanes(worldToProjectionMatrix, planes);
            var cullType = isInFrustum(planes[0], planes[1], planes[2], planes[3], planes[4], planes[5], b);
            planes.Dispose();
            return cullType;
        }

        public static CullType isInFrustum(Plane plane0, Plane plane1, Plane plane2, Plane plane3, Plane plane4, Plane plane5, Bounds b)
        {
            Vector3 p0 = b.min;
            Vector3 p1 = b.min + Vector3.right * b.size.x;
            Vector3 p2 = b.min + Vector3.forward * b.size.z;
            Vector3 p3 = b.min + Vector3.right * b.size.x + Vector3.forward * b.size.z;
            Vector3 p4 = p0 + Vector3.up * b.size.y;
            Vector3 p5 = p1 + Vector3.up * b.size.y;
            Vector3 p6 = p2 + Vector3.up * b.size.y;
            Vector3 p7 = p3 + Vector3.up * b.size.y;

            if (isBehindPlaneAABB(plane0, p0, p1, p2, p3, p4, p5, p6, p7)) return CullType.CULLED;
            if (isBehindPlaneAABB(plane1, p0, p1, p2, p3, p4, p5, p6, p7)) return CullType.CULLED;
            if (isBehindPlaneAABB(plane2, p0, p1, p2, p3, p4, p5, p6, p7)) return CullType.CULLED;
            if (isBehindPlaneAABB(plane3, p0, p1, p2, p3, p4, p5, p6, p7)) return CullType.CULLED;
            if (isBehindPlaneAABB(plane4, p0, p1, p2, p3, p4, p5, p6, p7)) return CullType.CULLED;
            if (isBehindPlaneAABB(plane5, p0, p1, p2, p3, p4, p5, p6, p7)) return CullType.CULLED;

            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p0)) return CullType.PARTIALLY_VISIBLE;
            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p1)) return CullType.PARTIALLY_VISIBLE;
            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p2)) return CullType.PARTIALLY_VISIBLE;
            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p3)) return CullType.PARTIALLY_VISIBLE;
            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p4)) return CullType.PARTIALLY_VISIBLE;
            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p5)) return CullType.PARTIALLY_VISIBLE;
            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p6)) return CullType.PARTIALLY_VISIBLE;
            if (!isPointInsideFrustum(plane0, plane1, plane2, plane3, plane4, plane5, p7)) return CullType.PARTIALLY_VISIBLE;

            return CullType.VISIBLE;
        }
        #endregion

        #endregion

        #region Unity Color
        public static Color setR(this Color color, float r)
        {
            color.r = r;
            return color;
        }
        public static Color setG(this Color color, float g)
        {
            color.g = g;
            return color;
        }
        public static Color setB(this Color color, float b)
        {
            color.b = b;
            return color;
        }
        public static Color setA(this Color color, float a)
        {
            color.a = a;
            return color;
        }
        public static Color setRGBA(this Color color, float r = float.NaN, float g = float.NaN, float b = float.NaN, float a = float.NaN)
        {
            if (!r.Equals(float.NaN))
            {
                color.r = r;
            }
            if (!g.Equals(float.NaN))
            {
                color.g = g;
            }
            if (!b.Equals(float.NaN))
            {
                color.b = b;
            }
            if (!a.Equals(float.NaN))
            {
                color.a = a;
            }
            return color;
        }
        /// <summary>
        /// Format: #RRGGBBAA
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string toHex(this Color color)
        {
            Color32 c = color;
            var hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
            return hex;
        }
        /// <summary>
        /// Format: #RRGGBBAA
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static void toHex(this Color color, StringBuilder stringBuilder)
        {
            Color32 c = color;
            stringBuilder.Clear();
            stringBuilder.AppendFormat("#{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
        }
        /// <summary>
        /// htmlCode format: #RGB,#RRGGBB,#RGBA, #RRGGBBAA, red, cyan, blue, darkblue, lightblue, purple, yellow, lime, fuchsia, white, silver, grey, black, orange, brown, maroon, green, olive, navy, teal, aqua, magenta
        /// </summary>
        public static Color toColor(this string htmlCode)
        {
            if (ColorUtility.TryParseHtmlString(htmlCode, out var c))
            {
                return c;
            }
            throw new Exception("Error to parse html color");
        }
        /// <summary>
        /// hex format: #RRGGBB, #RRGGBBAA, RRGGBB, RRGGBBAA
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color hexColor(this string hex)
        {
            int startIndex;
            if (hex[0] == '#')
            {
                startIndex = 1;
            }
            else
            {
                startIndex = 0;
            }
            var binary = hexToBytes(hex, startIndex);
            if (binary.Length == 3)
            {
                return new Color(binary[0] / 255f, binary[1] / 255f, binary[2] / 255f);
            }
            else
            {
                return new Color(binary[0] / 255f, binary[1] / 255f, binary[2] / 255f, binary[3] / 255f);
            }
        }
        public static byte[] to4Bytes(this Color color)
        {
            return NConverter.getBytes(color.asNumber());
        }
        /// <summary>
        /// 4 bytes to color
        /// </summary>
        public static Color bytesToColor(byte[] inData, int startIndex = 0)
        {
            return asColor(NConverter.fromBytesWithoutChecks<uint>(inData, startIndex));
        }
        public static uint asNumber(this Color color)
        {
            byte r = (byte)Math.Round(color.r * 255);
            byte g = (byte)Math.Round(color.g * 255);
            byte b = (byte)Math.Round(color.b * 255);
            byte a = (byte)Math.Round(color.a * 255);
            return NConverter.bitConvertWithoutChecks<int, uint>(r | g << 8 | b << 16 | a << 24);
        }
        public static float4 toHsvFloat4(this Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            return new float4(h, s, v, color.a);
        }
        public static Color asColor(this uint from)
        {
            float a = (from >> 24 & 0xFF) / 255f;
            float b = (from >> 16 & 0xFF) / 255f;
            float g = (from >> 8 & 0xFF) / 255f;
            float r = (from & 0xFF) / 255f;
            return new Color(r, g, b, a);
        }
        public static unsafe string numberColorToHex(this uint numColor)
        {
            var ptr = (byte*)&numColor;
            return bytesToHex(ptr, 4);
        }
        public static unsafe uint hexColorToNumber(this string hex)
        {
            return hexColorToNumber(hex.AsSpan());
        }
        public static unsafe uint hexColorToNumber(this ReadOnlySpan<char> hexSpan)
        {
            uint result = 0xffffffff;
            var ptr = (byte*)&result;
            internal_hexToBytes(hexSpan, ptr);
            return result;
        }
        public static float4 toFloat4(this Color from)
        {
            return NConverter.bitConvertWithoutChecks<Color, float4>(from);
        }
        public static Color toColor(this float4 from)
        {
            return NConverter.bitConvertWithoutChecks<float4, Color>(from);
        }
        public static Color toHsvColor(this float4 from)
        {
            return Color.HSVToRGB(from.x, from.y, from.z).setA(from.w);
        }
        #endregion

        #region String
        public static string formatToMMSS(this int totalSeconds)
        {
            var m = totalSeconds / 60;
            var s = totalSeconds % 60;
            return $"{m:0#}:{s:0#}";
        }
        public static string formatToMMSS(this long totalSeconds)
        {
            var m = totalSeconds / 60;
            var s = totalSeconds % 60;
            return $"{m:0#}:{s:0#}";
        }
        public static string formatToHHMMSS(this int totalSeconds)
        {
            var h = totalSeconds / 3600;
            var remainingM = totalSeconds % 3600;
            var m = remainingM / 60;
            var s = remainingM % 60;
            return $"{h:0#}:{m:0#}:{s:0#}";
        }
        public static string formatToHHMMSS(this long totalSeconds)
        {
            var h = totalSeconds / 3600;
            var remainingM = totalSeconds % 3600;
            var m = remainingM / 60;
            var s = remainingM % 60;
            return $"{h:0#}:{m:0#}:{s:0#}";
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
            return sb.ToString();
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
        public static byte byteOfHex(char hexChar)
        {
            return hexChar switch
            {
                >= '0' and <= '9' => (byte)(hexChar - '0'),
                >= 'A' and <= 'F' => (byte)(hexChar - 'A' + 10),
                >= 'a' and <= 'f' => (byte)(hexChar - 'a' + 10),
                _ => throw new ArgumentException($"Invalid hex character: {hexChar}")
            };
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
        public static unsafe string computeMD5(this string s)
        {
            var strSpan = s.AsSpan();
            using var provider = System.Security.Cryptography.MD5.Create();
            Span<byte> dst = stackalloc byte[16];
            provider.TryComputeHash(strSpan.asSpan<char, byte>(), dst, out _);
            return bytesToHex(dst);
        }
        public static unsafe decimal computeMD5AsDecimal(this string s)
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
            using var output = new MemoryStream();
            var deflate = new DeflateStream(output, CompressionMode.Compress);
            deflate.Write(data, 0, data.Length);
            deflate.Dispose();
            var outputBytes = output.ToArray();
            return $":{version}:{Convert.ToBase64String(outputBytes)}";
        }
        public static string compressToDeflateString(this ReadOnlySpan<byte> data, int version = 0)
        {
            using var output = new MemoryStream();
            var deflate = new DeflateStream(output, CompressionMode.Compress);
            deflate.Write(data);
            deflate.Dispose();
            var outputBytes = output.ToArray();
            return $":{version}:{Convert.ToBase64String(outputBytes)}";
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
        public static void CopyTo<T>(this IEnumerable<T> colletion, IList<T> list)
        {
            list.Clear();
            foreach (var item in colletion)
            {
                list.Add(item);
            }
        }
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
        public unsafe static byte[] toBytes<T>(this NativeArray<T> self) where T : unmanaged
        {
            var dst = new byte[self.Length * NUtils.sizeOf<T>()];
            fixed (byte* dstPtr = dst)
            {
                var srcPtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(self);
                Buffer.MemoryCopy(srcPtr, dstPtr, dst.Length, dst.Length);
            }
            return dst;
        }
        public static NPArray<T> toNPArray<T>(this IEnumerable<T> colletion)
        {
            return NPArray<T>.get(colletion);
        }
        public static NPArray<T> toNPArrayWithoutTracking<T>(this IEnumerable<T> colletion)
        {
            return NPArray<T>.getWithoutTracking(colletion);
        }
        public static NPUArray<T> toNPUArray<T>(this IEnumerable<T> colletion) where T : unmanaged
        {
            return NPUArray<T>.get(colletion);
        }
        public static NPUArray<T> toNPUArrayWithoutTracking<T>(this IEnumerable<T> colletion) where T : unmanaged
        {
            return NPUArray<T>.getWithoutTracking(colletion);
        }
        public static NPHSet<T> toNPHSet<T>(this IEnumerable<T> colletion)
        {
            return NPHSet<T>.get(colletion);
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
        public static void addRange<T>(this ICollection<T> self, ReadOnlySpan<T> values)
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
        public static void removeLast(this IBList self)
        {
            self.RemoveAt(self.Count - 1);
        }
        public static void removeLast<T>(this NativeList<T> self) where T : unmanaged
        {
            self.RemoveAt(self.Length - 1);
        }
        public static void removeLast<T>(this NPUArray<T> self) where T : unmanaged
        {
            self.RemoveAt(self.Count - 1);
        }

        public static T takeAndRemoveAt<T>(this List<T> self, int index)
        {
            var item = self[index];
            self.RemoveAt(index);
            return item;
        }
        public static T takeAndRemoveAt<T>(this IBList<T> self, int index)
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
        public static T takeAndRemoveLast<T>(this IBList<T> self)
        {
            var item = self[^1];
            self.removeLast();
            return item;
        }
        public static T takeAndRemoveLast<T>(this NativeList<T> self) where T : unmanaged
        {
            var item = self[^1];
            self.removeLast();
            return item;
        }
        public static T takeAndRemoveLast<T>(this NPUArray<T> self) where T : unmanaged
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
        public static void removeAtSwapBack<T>(this NPUArray<T> self, int index) where T : unmanaged
        {
            var lastIndex = self.Count - 1;
            self[index] = self[lastIndex];
            self.RemoveAt(lastIndex);
        }
        public static void removeAtSwapBack<T>(this NList<T> self, int index)
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
        public static bool removeSwapBack<T>(this NPUArray<T> self, T item) where T : unmanaged
        {
            var index = self.IndexOf(item);
            if (index < 0) return false;
            self.removeAtSwapBack(index);
            return true;
        }
        public static bool removeSwapBack<T>(this NList<T> self, T item)
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
        public static T takeAndRemoveAtSwapBack<T>(this NativeList<T> self, int index) where T : unmanaged
        {
            var item = self[index];
            self.RemoveAtSwapBack(index);
            return item;
        }
        public static T takeAndRemoveAtSwapBack<T>(this NPUArray<T> self, int index) where T : unmanaged
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
        public unsafe static void fill<T>(this T[] self, T value) where T : unmanaged
        {
            if (self.Length == 0) return;
            fixed (T* ptr = self)
            {
                UnsafeUtility.MemCpyReplicate(ptr, &value, UnsafeUtility.SizeOf<T>(), self.Length);
            }
        }
        public static unsafe IntPtr getIntPtr<T>(this T[] self) where T : unmanaged
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
        #endregion

        #region Random
        private static uint s_randCount;
        public static Random getRandom(uint seed = 0)
        {
            if (seed == 0)
            {
                var randIndex = (DateTimeOffset.Now.ToUnixTimeMilliseconds() + s_randCount).GetHashCode() & 0x7fffffff;
                var rand = Random.CreateFromIndex(NConverter.bitConvert<int, uint>(randIndex));
                s_randCount = rand.state;
                return rand;
            }
            return new Random(seed);
        }
        public static Random getRandomFromState(uint state)
        {
            return new Random() { state = state };
        }
        public static int randInt32(int min, int max, ICollection<int> exclusiveNumbers, uint seed = 0)
        {
            return randInt32(min, max, exclusiveNumbers, getRandom(seed));
        }
        public static int randInt32(int min, int max, ICollection<int> exclusiveNumbers, Random rand)
        {
            return randInt32(min, max, exclusiveNumbers, ref rand);
        }
        public static int randInt32(int min, int max, ICollection<int> exclusiveNumbers, ref Random rand)
        {
            if (exclusiveNumbers == null || exclusiveNumbers.Count == 0)
            {
                return rand.NextInt(min, max);
            }

            Span<int> nums = stackalloc int[max - min];
            int count = 0;
            for (int i = min; i < max; i++)
            {
                if (!exclusiveNumbers.Contains(i))
                {
                    nums[count++] = i;
                }
            }
            return nums[rand.NextInt(count)];
        }
        /// <summary>
        /// return random int array
        /// </summary>
        public static int[] createRandomIntArray(int arrayLength, int fillCount, uint seed = 0)
        {
            var rand = getRandom(seed);
            return createRandomIntArray(arrayLength, fillCount, ref rand);
        }
        public static int[] createRandomIntArray(int arrayLength, int fillCount, Random rand)
        {
            return createRandomIntArray(arrayLength * fillCount, fillCount, ref rand);
        }
        public static int[] createRandomIntArray(int arrayLength, int fillCount, ref Random rand)
        {
            int[] array = new int[arrayLength];
            for (int i = 1; i < fillCount; i++)
            {
                array[i] = i;
            }
            array.shuffle(fillCount, rand);
            return array;
        }

        public static NPUArray<int> getRandomIndices(int maxIndex, int count, int minIndex = 0, uint seed = 0)
        {
            var rand = getRandom(seed);
            return getRandomIndices(maxIndex, count, minIndex, ref rand);
        }
        public static NPUArray<int> getRandomIndices(int maxIndex, int count, int minIndex, Random rand)
        {
            return getRandomIndices(maxIndex, count, minIndex, ref rand);
        }
        public static NPUArray<int> getRandomIndices(int maxIndex, int count, int minIndex, ref Random rand)
        {
            Span<int> nums = stackalloc int[maxIndex - minIndex];
            int itemCount = 0;
            for (int i = minIndex; i < maxIndex; i++)
            {
                nums[itemCount++] = i;
            }
            shuffle(nums, rand);
            var result = NPUArray<int>.get();
            foreach (var num in nums[..(itemCount < count ? itemCount : count)])
            {
                result.Add(num);
            }
            return result;
        }


        public static T randItem<T>(this IList<T> list, out int randIndex, uint seed = 0)
        {
            return randItem(list, out randIndex, getRandom(seed));
        }
        public static T randItem<T>(this IList<T> list, out int randIndex, Random rand)
        {
            randIndex = rand.NextInt(list.Count);
            return list[randIndex];
        }
        public static T randItem<T>(this IList<T> list, out int randIndex, ref Random rand)
        {
            randIndex = rand.NextInt(list.Count);
            return list[randIndex];
        }


        public static T randItem<T>(this IList<T> list, uint seed = 0)
        {
            return list.randItem(out _, getRandom(seed));
        }

        public static T randItem<T>(this IList<T> list, Random rand)
        {
            return list.randItem(out _, rand);
        }

        public static T randItem<T>(this IList<T> list, ref Random rand)
        {
            return list[rand.NextInt(list.Count)];
        }


        public static T randItem<T>(this Span<T> self, uint seed = 0)
        {
            return randItem(self, out _, seed);
        }

        public static T randItem<T>(this Span<T> self, Random rand)
        {
            return randItem(self, out _, ref rand);
        }

        public static T randItem<T>(this Span<T> self, ref Random rand)
        {
            return randItem(self, out _, ref rand);
        }

        public static T randItem<T>(this Span<T> self, out int index, uint seed = 0)
        {
            return randItem(self, out index, getRandom(seed));
        }

        public static T randItem<T>(this Span<T> self, out int index, Random rand)
        {
            return randItem(self, out index, ref rand);
        }
        public static T randItem<T>(this Span<T> self, out int index, ref Random rand)
        {
            index = rand.NextInt(self.Length);
            return self[index];
        }
        /// <summary>
        /// get a random item in contain list, return default if self is empty
        /// </summary>
        /// <typeparam name="T">is a built-in type, struct or class</typeparam>
        /// <param name="self">is contain list</param>
        /// <param name="index">index of returned item</param>
        /// <param name="exclusiveIndices">exclude indices if you didn't want it is returned</param>
        /// <returns></returns>
        public static T randItem<T>(this IList<T> self, out int index, ICollection<int> exclusiveIndices, uint seed = 0)
        {
            return randItem(self, out index, exclusiveIndices, getRandom(seed));
        }
        public static T randItem<T>(this IList<T> self, out int index, ICollection<int> exclusiveIndices, Random rand)
        {
            index = randInt32(0, self.Count, exclusiveIndices, rand);
            return self[index];
        }
        public static T randItem<T>(this IList<T> self, out int index, ICollection<int> exclusiveIndices, ref Random rand)
        {
            index = randInt32(0, self.Count, exclusiveIndices, ref rand);
            return self[index];
        }

        public static T randItem<T>(this Span<T> self, out int index, Func<T, bool> exclusivePredicate, ref Random rand)
        {
            using var validIndices = NPUArray<int>.getWithoutTracking();
            for (int i = self.Length - 1; i >= 0; i--)
            {
                if (!exclusivePredicate(self[i])) validIndices.Add(i);
            }
            index = validIndices[rand.NextInt(validIndices.Count)];
            return self[index];
        }
        public static T randItem<T>(this Span<T> self, out int index, Func<T, bool> exclusivePredicate, Random rand)
        {
            return randItem(self, out index, exclusivePredicate, ref rand);
        }
        public static T randItem<T>(this Span<T> self, out int index, Func<T, bool> exclusivePredicate, uint seed = 0)
        {
            return randItem(self, out index, exclusivePredicate, getRandom(seed));
        }
        public static T randItem<T>(this Span<T> self, Func<T, bool> exclusivePredicate, uint seed = 0)
        {
            return randItem(self, out _, exclusivePredicate, getRandom(seed));
        }

        public static void shuffle<T>(this IList<T> list, int startIndex, int count, ref Random rand)
        {
            int n = startIndex + count;
            if (n > list.Count) n = list.Count;

            int startIndexPlus1 = startIndex + 1;
            while (n > startIndexPlus1)
            {
                int k = rand.NextInt(startIndex, n--);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
        public static void shuffle<T>(this IList<T> list, int startIndex, int count, Random rand)
        {
            shuffle(list, startIndex, count, ref rand);
        }

        public static void shuffle<T>(Span<T> self, uint seed = 0)
        {
            shuffle(self, getRandom(seed));
        }
        public static void shuffle<T>(Span<T> self, ref Random rand)
        {
            int n = self.Length;
            while (n > 1)
            {
                int k = rand.NextInt(n--);
                (self[n], self[k]) = (self[k], self[n]);
            }
        }
        public static void shuffle<T>(Span<T> self, Random rand)
        {
            shuffle(self, ref rand);
        }


        public static void shuffle<T>(this IList<T> self, uint seed = 0)
        {
            shuffle(self, 0, self.Count, getRandom(seed));
        }

        public static void shuffle<T>(this IList<T> self, Random rand)
        {
            shuffle(self, 0, self.Count, rand);
        }

        public static void shuffle<T>(this IList<T> self, ref Random rand)
        {
            shuffle(self, 0, self.Count, ref rand);
        }


        public static void shuffle<T>(this IList<T> self, int count, uint seed = 0)
        {
            shuffle(self, 0, count, getRandom(seed));
        }

        public static void shuffle<T>(this IList<T> self, int count, Random rand)
        {
            shuffle(self, 0, count, rand);
        }

        public static void shuffle<T>(this IList<T> self, int count, ref Random rand)
        {
            shuffle(self, 0, count, ref rand);
        }
        public static void shuffle<T>(this IList<T> self, int startIndex, int count, uint seed = 0)
        {
            shuffle(self, startIndex, count, getRandom(seed));
        }
        #endregion

        #region GameObject and Component

        public static void setActive(this Component target, bool isActive)
        {
            target.gameObject.setActive(isActive);
        }
        public static void setActive(this UnityEngine.Object target, bool isActive)
        {
            if (target is Component component)
            {
                component.setActive(isActive);
            }
            else if (target is GameObject gameObject)
            {
                gameObject.setActive(isActive);
            }
        }

        public static void setEnable(this Behaviour target, bool isEnable)
        {
            if (target.enabled != isEnable)
            {
                target.enabled = isEnable;
            }
        }

        public static void setEnable(this Renderer target, bool isEnable)
        {
            if (target.enabled != isEnable)
            {
                target.enabled = isEnable;
            }
        }

        public static void setActive(this GameObject target, bool isActive)
        {
            if (isActive != target.activeSelf)
            {
                target.SetActive(isActive);
            }
        }

        public static T getOrAddComponent<T>(this GameObject target) where T : Component
        {
            if (!target.TryGetComponent<T>(out var com))
            {
                com = target.AddComponent<T>();
            }
            return com;
        }

        public static T getOrAddComponent<T>(this Component target) where T : Component
        {
            return target.gameObject.getOrAddComponent<T>();
        }

        public static T getOrAddComponent<T>(this UnityEngine.Object target) where T : Component
        {
            if (target is GameObject go)
            {
                return go.getOrAddComponent<T>();
            }
            else if (target is Component component)
            {
                return component.gameObject.getOrAddComponent<T>();
            }
            else
            {
                throw new Exception($"target is not a GameObject or Component, type: {target.GetType()}");
            }
        }
        internal static NPList<T> getComponents_CachedList<T>(this GameObject target)
        {
            var list = NPList<T>.get();
            target.GetComponents(list.Collection);
            return list;
        }
        internal static NPList<T> getComponentsInChildren_CachedList<T>(this GameObject target, bool isIncludeInactive = false)
        {
            var list = NPList<T>.get();
            target.GetComponentsInChildren(isIncludeInactive, list.Collection);
            return list;
        }

        public static void removeComponent(Component c)
        {
            GameObject.DestroyImmediate(c);
        }
        public static void removeComponents<T>(this GameObject target, bool isIncludeChildren = false) where T : Component
        {
            T[] components;
            if (isIncludeChildren)
            {
                components = target.GetComponentsInChildren<T>(true);
            }
            else
            {
                components = target.GetComponents<T>();
            }

            for (int i = components.Length - 1; i >= 0; i--)
            {
                removeComponent(components[i]);
            }
        }
        public static void removeChildrenComponents<T>(this GameObject target) where T : Component
        {
            var components = target.GetComponentsInChildren<T>(true);

            for (int i = components.Length - 1; i >= 1; i--)
            {
                removeComponent(components[i]);
            }
        }
        public static T keepSingleComponent<T>(this GameObject target, bool isIncludeChildren = false) where T : Component
        {
            T[] findResults;
            if (isIncludeChildren)
            {
                findResults = target.GetComponentsInChildren<T>(true);
            }
            else
            {
                findResults = target.GetComponents<T>();
            }

            if (findResults.Length == 0)
            {
                findResults = new T[] { target.AddComponent<T>() };
            }
            else
            {
                for (int i = findResults.Length - 1; i >= 1; i--)
                {
                    removeComponent(findResults[i]);
                }
            }
            return findResults[0];
        }

        public static void setLayer(this GameObject target, string layerName, bool isIncludeChildren = true)
        {
            int layer = LayerMask.NameToLayer(layerName);
            target.setLayer(layer, isIncludeChildren);
        }
        public static void setLayer(this GameObject target, int layer, bool isIncludeChildren = true)
        {
            if (isIncludeChildren)
            {
                var trans = target.GetComponentsInChildren<Transform>(true);
                foreach (var t in trans)
                {
                    t.gameObject.layer = layer;
                }
            }
            else
            {
                target.layer = layer;
            }
        }
        private static void innerDestroy(UnityEngine.Object target, bool isImmediate = false)
        {
            if (isImmediate || !NStartRunner.IsPlaying)
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
            else
            {
                UnityEngine.Object.Destroy(target);
            }
        }
        public static void destroy(UnityEngine.Object target, bool isImmediate = false)
        {
            if (target == null) return;
            innerDestroy(target, isImmediate);
        }
        public static void destroyObject(UnityEngine.Object target, bool isImmediate = false)
        {
            if (target == null) return;
            if (target is Component component)
            {
                innerDestroy(component.gameObject, isImmediate);
            }
            else if (target is GameObject gameObject)
            {
                innerDestroy(gameObject, isImmediate);
            }
            else
            {
                innerDestroy(target, isImmediate);
            }
        }
        public static void destroyObject(Component component, bool isImmediate = false)
        {
            if (component == null) return;
            innerDestroy(component.gameObject, isImmediate);
        }
        public static void destroyObject(GameObject go, bool isImmediate = false)
        {
            if (go == null) return;
            innerDestroy(go, isImmediate);
        }
        public static Transform getChild(this Transform g, string name)
        {
            var p = g.transform;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            for (int i = 0; i < p.childCount; ++i)
            {
                var child = p.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }

                if (child.childCount > 0)
                {
                    var c = getChild(child, name);
                    if (c != null)
                    {
                        return c;
                    }
                }
            }
            return null;
        }
        public static Transform getChild(this Transform g, string name, string parentName)
        {
            var p = g.transform;
            for (int i = 0; i < p.childCount; ++i)
            {
                var child = p.GetChild(i);
                if (child.name == name && child.parent.name == parentName)
                {
                    return child;
                }
                else
                {
                    if (child.childCount > 0)
                    {
                        var c = getChild(child, name, parentName);
                        if (c != null)
                        {
                            return c;
                        }
                    }
                }
            }
            return null;
        }
        public static Transform getChildWithChainNames(this Transform g, params string[] names)
        {
            if (names.Length == 1)
            {
                return getChild(g, names[0]);
            }
            if (names.Length == 2)
            {
                return getChild(g, names[1], names[0]);
            }

            var tmp = g;
            int index = 0;
            while (index < names.Length - 1)
            {
                var tGo = getChild(tmp, names[index + 1], names[index]);
                if (tGo)
                {
                    index++;
                    tmp = tGo;
                    if (index == names.Length - 1)
                    {
                        return tmp;
                    }
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
        public static Transform getChildFromPath(this Transform g, string path)
        {
            var ps = path.Split('/');
            return getChildWithChainNames(g, ps);
        }
        /// <summary>
        /// exclusive root name in path
        /// </summary>
        /// <param name="g"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static string getPathFromRoot(this Transform g, Transform root)
        {
            var path = g.name;
            var parent = g.parent;
            while (parent != null && parent != root)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        public static void setParent(this Transform transform, Transform parent, bool worldPositionStays = true)
        {
            transform.SetParent(parent, worldPositionStays);
            if (parent == null)
            {
                SceneManager.MoveGameObjectToScene(transform.gameObject, SceneManager.GetActiveScene());
            }
        }
        public static bool isNull<T>(this T self)
        {
            if (self == null || self.Equals(null)) return true;
            return false;
        }
        public static bool isNullOrDefault<T>(this T self)
        {
            if (isNull(self)) return true;
            return equals(self, getDefaultGeneric<T>());
        }
        #endregion

        #region Bounds and Collider
        public static Bounds getLocalBounds(this GameObject target, bool isIncludeChildren = true, bool isIncludeInactive = true)
        {
            var cloneContainer = new GameObject("Container");
            cloneContainer.SetActive(false);
            var clone = GameObject.Instantiate(target, cloneContainer.transform);

            clone.transform.resetTransform(false);

            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            if (!isIncludeChildren)
            {
                bounds = clone.TryGetComponent<Renderer>(out var renderer) ? renderer.bounds : bounds;
            }
            else
            {
                foreach (Renderer r in clone.GetComponentsInChildren<Renderer>(isIncludeInactive))
                {
                    bounds.Encapsulate(r.bounds);
                }
            }
            NUtils.destroyObject(cloneContainer);
            return bounds;
        }
        public static BoxCollider addBoxCollider(this GameObject target, bool isIncludeChildren = true, bool keepSingleComponent = false)
        {
            BoxCollider targetBoxCollider;
            if (keepSingleComponent)
            {
                target.keepSingleComponent<Collider>(isIncludeChildren);
                targetBoxCollider = target.keepSingleComponent<BoxCollider>();
            }
            else
            {
                targetBoxCollider = target.AddComponent<BoxCollider>();
            }

            var cloneContainer = new GameObject("Container");
            cloneContainer.SetActive(false);
            var clone = GameObject.Instantiate(target, cloneContainer.transform);

            clone.transform.resetTransform(false);
            var bounds = clone.getLocalBounds(isIncludeChildren);
            targetBoxCollider.center = bounds.center;
            targetBoxCollider.size = bounds.size;

            NAssetUtils.setDirty(target);
            NUtils.destroyObject(cloneContainer);
            return targetBoxCollider;
        }
        public static BoxCollider generateBoxCollider(this RectTransform rt)
        {
            var rect = rt.rect;
            return generateBoxCollider(rt, rect.width, rect.height);
        }
        public static BoxCollider generateBoxCollider(this RectTransform rt, float width, float height)
        {
            var box = rt.gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(width, height, 1);
            box.center = new Vector3((0.5f - rt.pivot.x) * box.size.x, (0.5f - rt.pivot.y) * box.size.y);
            box.isTrigger = true;
            return box;
        }
        public static BoxCollider copyBoxColliderFrom(this GameObject target, BoxCollider from, bool applyPosition = false, bool applyRotation = false, bool applyScale = false)
        {
            if (from == null)
            {
                return null;
            }

            var cloneFrom = GameObject.Instantiate(from);
            var cloneTranform = cloneFrom.transform;
            var targetTransform = target.transform;

            if (applyPosition)
            {
                targetTransform.position = cloneTranform.position;
            }
            if (applyRotation)
            {
                targetTransform.rotation = cloneTranform.rotation;
            }
            if (applyScale)
            {
                if (!targetTransform.parent || targetTransform.lossyScale.hasZeroAxis())
                {
                    if (cloneTranform.lossyScale.hasZeroAxis())
                    {
                        targetTransform.localScale = cloneTranform.localScale;
                    }
                    else
                    {
                        targetTransform.localScale = cloneTranform.localScale.div(cloneTranform.lossyScale);
                    }
                }
                else
                {
                    targetTransform.localScale = cloneTranform.lossyScale.div(targetTransform.lossyScale);
                }
            }

            target.removeComponents<Collider>();
            var boxCollider = target.keepSingleComponent<BoxCollider>();

            if (applyPosition)
            {
                boxCollider.center = targetTransform.InverseTransformPoint(cloneTranform.TransformPoint(cloneFrom.center));
            }
            else
            {
                boxCollider.center = cloneFrom.center;
            }

            if (cloneTranform.lossyScale.hasZeroAxis())
            {
                boxCollider.size = cloneFrom.size;
            }
            else
            {
                if (targetTransform.lossyScale.hasZeroAxis())
                {
                    boxCollider.size = cloneFrom.size.mul(cloneTranform.lossyScale);
                }
                else
                {
                    boxCollider.size = cloneFrom.size.mul(cloneTranform.lossyScale.div(targetTransform.lossyScale));
                }
            }

            NAssetUtils.setDirty(target);
            NUtils.destroyObject(cloneFrom, true);
            return boxCollider;
        }
        #endregion

        #region Animator
        public static void setIntegerFromTo(this Animator animator, int fromHash, int toHash)
        {
            var value = animator.GetInteger(fromHash);
            animator.SetInteger(toHash, value);
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
            var constructors = type.GetConstructors(getAllBindingFlags());
            if (constructors.Length > 0)
            {
                var c = constructors[0];
                if (c.GetParameters().Length == 0)
                {
                    return c.Invoke(null);
                }
            }
            return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        }
        public static T createInstance<T>(this Type type)
        {
            return (T)createInstance(type);
        }
        public static T createInstance<T>()
        {
            return (T)createInstance(typeof(T));
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

        private static ExpirableValue<NBListCompareHashCode<Type>> _customTypeCached;
        public static NBListCompareHashCode<Type> getCustomTypes()
        {
            if (_customTypeCached == null)
            {
                var typeList = new NBListCompareHashCode<Type>();
                var assembles = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assembles)
                {
                    var assemblyName = assembly.GetName().Name;

                    if (assemblyName == "mscorlib")
                    {
                        continue;
                    }
                    if (assemblyName == "ExCSS.Unity")
                    {
                        continue;
                    }
                    if (assemblyName.StartsWith("System"))
                    {
                        continue;
                    }
                    if (assemblyName.StartsWith("Mono."))
                    {
                        continue;
                    }

                    if (assemblyName.StartsWith("unity", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (assemblyName.StartsWith("nunit.", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    typeList.AddRange(assembly.GetTypes());
                }

                typeList.Sort();
                _customTypeCached = new(typeList, 5, () =>
                {
                    _customTypeCached = null;
                });
            }
            return _customTypeCached;
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
        public static NTask<byte[]> getBinaryFrom(string url)
        {
            return getBinaryFrom(new Uri(url));
        }
        public static async NTask<byte[]> getBinaryFrom(Uri uri)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (uri.IsFile)
            {
                return await System.IO.File.ReadAllBytesAsync(uri.LocalPath);
            }
#endif
            using var unityWebRequest = UnityWebRequest.Get(uri);
            await unityWebRequest.SendWebRequest();
            if (string.IsNullOrEmpty(unityWebRequest.error))
            {
                return unityWebRequest.downloadHandler.data;
            }
            throw new Exception(unityWebRequest.error);
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
        public static PlayerLoopSystem addPlayerLoopSystem<TLoopSystemType>(PlayerLoopSystem defaultPlayerLoop, PlayerLoopSystem sys) where TLoopSystemType : struct
        {
            var loopSystemType = typeof(TLoopSystemType);
            var subSystemList = defaultPlayerLoop.subSystemList.clone();
            bool added = false;

            for (int i = 0; i < subSystemList.Length; ++i)
            {
                var subSytem = subSystemList[i];
                if (loopSystemType == subSytem.type)
                {
                    subSytem.subSystemList = subSytem.subSystemList.createOrAdd(sys);
                    subSystemList[i] = subSytem;
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                Debug.LogWarning($"Can't PlayerLoopSystem, reason: Not found type `{loopSystemType}`");
            }
            defaultPlayerLoop.subSystemList = subSystemList;
            return defaultPlayerLoop;
        }
        public static void set(this UnityEvent self, UnityAction call)
        {
            self.RemoveAllListeners();
            if (call != null)
            {
                self.AddListener(call);
            }
        }
        public static void setAction(this UnityEvent self, Action call)
        {
            self.RemoveAllListeners();
            if (call != null)
            {
                self.AddListener(new UnityAction(call));
            }
        }
        public static void set<T>(this UnityEvent<T> self, UnityAction<T> call)
        {
            self.RemoveAllListeners();
            if (call != null)
            {
                self.AddListener(call);
            }
        }
        public static void setAction<T>(this UnityEvent<T> self, Action<T> call)
        {
            self.RemoveAllListeners();
            if (call != null)
            {
                self.AddListener(new UnityAction<T>(call));
            }
        }
        public static void quit()
        {
            NStartRunner.quit();
        }
        #endregion
    }
}