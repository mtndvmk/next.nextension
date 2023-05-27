using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nextension
{
    public static class NUtils
    {
        #region Number Utils
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isPOT(float n)
        {
            int int_n = (int)n;
            if (n - int_n > Mathf.Epsilon)
            {
                return false;
            }
            return isPOT(int_n);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isPOT(int n)
        {
            return (n & (n - 1)) == 0 && n > 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool checkBitMask<T>(T mask, int bitIndex) where T : Enum
        {
            return checkBitMask((int)(object)mask, bitIndex);
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool checkBitMask(int mask, int bitIndex)
        {
            return (mask & (1 << bitIndex)) != 0;
        }
        /// <summary>
        /// return true if bit at bitIndex is 1, otherwise return false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool checkBitMask(long longMask, int bitIndex)
        {
            return (longMask & (1L << bitIndex)) != 0;
        }
        public static bool checkBitMask(byte[] byteMask, int bitIndex)
        {
            int byteIndex = bitIndex / 8;
            int maskIndex = bitIndex % 8;
            byte mask = byteMask[byteIndex];
            return (mask & 1 << maskIndex) != 0;
        }
        public static sbyte get0BitIndex(int mask)
        {
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
                for (;startIndex < endIndex; startIndex++)
                {
                    if ((mask & 1 << startIndex) != 0)
                    {
                        return startIndex;
                    }
                }
            }

            return -1;
        }
        public static sbyte get0BitIndex(long longMask)
        {
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
        public static int get0BitIndex(byte[] byteMask)
        {
            int byteIndex = -1;
            for (int i = 0; i < byteMask.Length; i++)
            {
                if (byteMask[i] != 0)
                {
                    byteIndex = i;
                    break;
                }
            }

            if (byteIndex >= 0)
            {
                var mask = byteMask[byteIndex];
                for (byte i = 0; i < 8; i++)
                {
                    if ((mask & 1 << i) != 0)
                    {
                        return byteIndex * 8 + i;
                    }
                }
            }
            return -1;
        }
        #endregion

        #region Unity Transform and Point Utils

        #region Vector2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        #endregion

        #region Vector3
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public static Vector3 set(this Vector3 vector3, float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue)
            {
                vector3.x = x.Value;
            }
            if (y.HasValue)
            {
                vector3.y = y.Value;
            }
            if (z.HasValue)
            {
                vector3.z = z.Value;
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
        public static Vector3 plus(this Vector3 vector3, float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue)
            {
                vector3.x += x.Value;
            }
            if (y.HasValue)
            {
                vector3.y += y.Value;
            }
            if (z.HasValue)
            {
                vector3.z += z.Value;
            }
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
        public static Vector3 mul(this Vector3 vector3, float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue)
            {
                vector3.x *= x.Value;
            }
            if (y.HasValue)
            {
                vector3.y *= y.Value;
            }
            if (z.HasValue)
            {
                vector3.z *= z.Value;
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
        #endregion

        #region Vector4
        public static Vector4 toVector4(this Quaternion quaternion)
        {
            return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }
        public static Quaternion toQuaternion(this Vector4 vector4)
        {
            return new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
        }
        #endregion

        #region Transform & Rect Transform
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
        public static bool hasParent(this Transform self, Transform other)
        {
            var parent = self.parent;
            while (parent)
            {
                if (parent == other)
                {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }
        public static bool hasChild(this Transform self, Transform other)
        {
            var children = self.GetComponentsInChildren<Transform>();
            return children.Contains(other);
        }
        public static Vector3 getBotomLeft(this RectTransform self, bool isWorldSpace = true)
        {
            var poses = new Vector3[4];
            if (isWorldSpace)
            {
                self.GetWorldCorners(poses);
                return poses[0];
            }
            else
            {
                self.GetLocalCorners(poses);
                return poses[0];
            }
        }
        public static Vector3 getTopLeft(this RectTransform self, bool isWorldSpace = true)
        {
            var poses = new Vector3[4];
            if (isWorldSpace)
            {
                self.GetWorldCorners(poses);
                return poses[1];
            }
            else
            {
                self.GetLocalCorners(poses);
                return poses[1];
            }
        }
        public static Vector3 getTopRight(this RectTransform self, bool isWorldSpace = true)
        {
            var poses = new Vector3[4];
            if (isWorldSpace)
            {
                self.GetWorldCorners(poses);
                Debug.Log(poses[2]);
                return poses[2];
            }
            else
            {
                self.GetLocalCorners(poses);
                return poses[2];
            }
        }

        public static Vector3 getBotomRight(this RectTransform self, bool isWorldSpace = true)
        {
            var poses = new Vector3[4];
            if (isWorldSpace)
            {
                self.GetWorldCorners(poses);
                return poses[3];
            }
            else
            {
                self.GetLocalCorners(poses);
                return poses[3];
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 getWorldToViewportMatrix(Camera camera)
        {
            return camera.projectionMatrix * camera.worldToCameraMatrix;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 getViewportToWorldMatrix(Camera camera)
        {
            return Matrix4x4.Inverse(getWorldToViewportMatrix(camera));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 viewportToScreenPoint(Vector2 viewportPoint)
        {
            return new Vector2(viewportPoint.x * Screen.width, viewportPoint.y * Screen.height);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        #endregion

        #region Unity Color Utils
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
        public static Color setRGBA(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            if (r.HasValue)
            {
                color.r = r.Value;
            }
            if (g.HasValue)
            {
                color.g = g.Value;
            }
            if (b.HasValue)
            {
                color.b = b.Value;
            }
            if (a.HasValue)
            {
                color.a = a.Value;
            }
            return color;
        }

        public static string toHex(this Color color)
        {
            Color32 c = color;
            var hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
            return hex;
        }
        /// <summary>
        /// </summary>
        /// <param name="htmlCode">format: #XXXXXXXX</param>
        /// <returns></returns>
        public static Color toColor(this string htmlCode)
        {
            Color c = Color.clear;
            var b = ColorUtility.TryParseHtmlString(htmlCode, out c);
            if (!b)
            {
                throw new Exception("---> Error to parse html color");
            }
            return c;
        }
        public static byte[] to4Bytes(this Color color)
        {
            byte r = (byte)(color.r * 255);
            byte g = (byte)(color.g * 255);
            byte b = (byte)(color.b * 255);
            byte a = (byte)(color.a * 255);
            var rgba = new byte[4] { r, g, b, a };
            return rgba;
        }
        /// <summary>
        /// 4 bytes to color
        /// </summary>
        public static Color bytesToColor(byte[] inData, int startIndex = 0)
        {
            var r = inData[startIndex] / 255f;
            var g = inData[startIndex + 1] / 255f;
            var b = inData[startIndex + 2] / 255f;
            var a = inData[startIndex + 3] / 255f;
            return new Color(r, g, b, a);
        }
        public static int toInt32(this Color color)
        {
            return NConverter.toInt32(color.to4Bytes(), 0);
        }
        public static Color fromInt32(int from)
        {
            return bytesToColor(NConverter.getBytes(from));
        }
        #endregion

        #region String Utils
        public static string firstCharToUpper(this string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("Input is null or empty");
            return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
        }
        public static string numberToMMSS(int totalSeconds)
        {
            var m = totalSeconds / 60;
            var s = totalSeconds % 60;
            return $"{m.ToString("0#")}:{s.ToString("0#")}";
        }
        public static uint stringToAdler32(string str)
        {
            const int mod = 65521;
            uint a = 1, b = 0;
            foreach (char c in str)
            {
                a = (a + c) % mod;
                b = (b + a) % mod;
            }
            return (b << 16) | a;
        }
        #endregion

        #region List and Array Utils
        public static List<T> add<T>(this List<T> self, T item, bool igoreIfExist = true)
        {
            if (igoreIfExist)
            {
                if (self.Contains(item))
                {
                    return self;
                }
            }
            self.Add(item);
            return self;
        }
        public static List<T> addAndSort<T>(this List<T> self, T item, bool igoreIfExist = true)
        {
            if (igoreIfExist)
            {
                if (self.Contains(item))
                {
                    return self;
                }
            }
            self.Add(item);
            self.Sort();
            return self;
        }
        public static List<T> addAndSort<T>(this List<T> self, T item, Comparison<T> comparison, bool igoreIfExist = true)
        {
            if (igoreIfExist)
            {
                if (self.Contains(item))
                {
                    return self;
                }
            }
            self.Add(item);
            self.Sort(comparison);
            return self;
        }
        /// <summary>
        /// append appendArray to self (target array)
        /// </summary>
        /// <typeparam name="T">is a built-in type, struct or class</typeparam>
        /// <param name="self">is target array</param>
        /// <param name="appendArray">is append array</param>
        /// <returns></returns>
        public static T[] appendArray<T>(this T[] self, ICollection<T> appendArray)
        {
            int oldLength = self.Length;
            Array.Resize(ref self, self.Length + appendArray.Count);
            appendArray.CopyTo(self, oldLength);
            return self;
        }

        /// <summary>
        /// remove items in list
        /// </summary>
        /// <typeparam name="T">is a built-in type, struct or class</typeparam>
        /// <param name="self">is list bContains item which you want remove</param>
        /// <param name="indexList">index of items which you want remove</param>
        public static void removeAt<T>(this IList<T> self, params int[] indexs)
        {
            for (int i = indexs.Length - 1; i >= 0; --i)
            {
                self.RemoveAt(indexs[i]);
            }
        }
        public static bool isSameItem<T>(this ICollection<T> a, ICollection<T> b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Count != b.Count)
            {
                return false;
            }
            return Enumerable.SequenceEqual(a, b);
        }

        #endregion

        #region Random Utils
        public static int randInt32(int min, int max, ICollection<int> exceptNumbers)
        {
            var intRand = new System.Random();
            if (exceptNumbers == null || exceptNumbers.Count == 0)
            {
                return intRand.Next(min, max);
            }
            var allNumbers = Enumerable.Range(min, max).Where(i => !exceptNumbers.Contains(i)).ToArray();
            var randIdx = intRand.Next(allNumbers.Length);
            return allNumbers[randIdx];
        }
        /// <summary>
        /// return random int array
        /// </summary>
        public static int[] createRandomIntArray(int arrayLength, int fillCount)
        {
            int[] array = new int[arrayLength];
            var rnd = new System.Random();
            var randomNumbers = Enumerable.Range(0, arrayLength).OrderBy(x => rnd.Next()).Take(fillCount).ToArray();
            Array.Copy(randomNumbers, array, randomNumbers.Length);
            return array;
        }

        public static int[] getRandomIndexs(int maxIndex, int count, int startIndex = 0)
        {
            var rnd = new System.Random();
            return Enumerable.Range(startIndex, maxIndex).OrderBy(x => rnd.Next()).Take(count).ToArray();
        }
        public static T randItem<T>(this IList<T> list, out int randIndex)
        {
            if (list.Count == 0)
            {
                randIndex = -1;
                return default;
            }
            randIndex = UnityEngine.Random.Range(0, list.Count);
            return list[randIndex];
        }
        public static T randItem<T>(this IList<T> list)
        {
            return list.randItem(out _);
        }
        /// <summary>
        /// get a random item in contain list, return default if self is empty
        /// </summary>
        /// <typeparam name="T">is a built-in type, struct or class</typeparam>
        /// <param name="self">is contain list</param>
        /// <param name="index">index of returned item</param>
        /// <param name="exceptIndex">exclude indexes if you didn't want it is returned</param>
        /// <returns></returns>
        public static T randItem<T>(this IList<T> self, out int index, IList<int> exceptIndex = null)
        {
            if (self.Count <= 0)
            {
                index = -1;
                return default;
            }
            index = randInt32(0, self.Count, exceptIndex);
            return self[index];
        }
        #endregion

        #region Buffer Utils
        public static byte[] merge(params byte[][] arrays)
        {
            int totalLength = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                totalLength += arrays[i].Length;
            }
            var data = new byte[totalLength];
            int pointer = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                Buffer.BlockCopy(arrays[i], 0, data, pointer, arrays[i].Length);
                pointer += arrays[i].Length;
            }
            return data;
        }
        public static byte[] mergeTo(this byte[] a, byte[] b)
        {
            var aOffset = a.Length;
            Array.Resize(ref a, a.Length + b.Length);
            Buffer.BlockCopy(b, 0, a, aOffset, b.Length);
            return a;
        }
        public static T[] getBlock<T>(T[] src, int startIndex, int count)
        {
            T[] b = new T[count];
            Buffer.BlockCopy(src, startIndex, b, 0, count);
            return b;
        }
        public static T[] getBlockToEnd<T>(T[] src, int startIndex)
        {
            return getBlock(src, startIndex, src.Length - startIndex);
        }
        #endregion

        #region GameObject and Component
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void setActive(this Component target, bool isActive)
        {
            target.gameObject.setActive(isActive);
        }
        public static void setActive(this UnityEngine.Object target, bool isActive)
        {
            if (target is Component)
            {
                (target as Component).setActive(isActive);
            }
            else if (target is GameObject)
            {
                (target as GameObject).setActive(isActive);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void setEnable(this Behaviour target, bool isEnable)
        {
            if (target.enabled != isEnable)
            {
                target.enabled = isEnable;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void setActive(this GameObject target, bool isActive)
        {
            if (isActive != target.activeSelf)
            {
                target.SetActive(isActive);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getOrAddComponent<T>(this GameObject target) where T : Component
        {
            var com = target.GetComponent<T>();
            if (com == null)
            {
                com = target.AddComponent<T>();
            }
            return com;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T getOrAddComponent<T>(this Component target) where T : Component
        {
            return target.gameObject.getOrAddComponent<T>();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public static T getParentContainComponent<T>(this GameObject target) where T : Component
        {
            Transform parent = target.transform;
            while (parent != null)
            {
                var comp = parent.GetComponent<T>();
                if (comp)
                {
                    return comp;
                }
                parent = parent.parent;
            }
            return null;
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
            if (!Application.isPlaying || isImmediate)
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
            else
            {
                UnityEngine.Object.Destroy(target);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void destroy(UnityEngine.Object target, bool isImmediate = false)
        {
            if (target == null) return;
            innerDestroy(target, isImmediate);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void destroyObject(UnityEngine.Object target, bool isImmediate = false)
        {
            if (target == null) return;
            if (target is Component)
            {
                innerDestroy((target as Component).gameObject, isImmediate);
            }
            else if (target is GameObject)
            {
                innerDestroy((target as GameObject), isImmediate);
            }
            else
            {
                innerDestroy(target, isImmediate);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void destroyObject(Component component, bool isImmediate = false)
        {
            if (component == null) return;
            innerDestroy(component.gameObject, isImmediate);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void destroyObject(GameObject go, bool isImmediate = false)
        {
            if (go == null) return;
            innerDestroy(go.gameObject, isImmediate);
        }
        public static Transform getChild(this Transform g, string name)
        {
            var p = g.transform;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            for (int i = 0; i < p.childCount; i++)
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
            for (int i = 0; i < p.childCount; i++)
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
        /// not include root name in path
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

        #endregion

        #region Bounds and Collider
        public static Bounds getLocalBounds(this GameObject target, bool isIncludeChildren = true)
        {
            var cloneContainer = new GameObject("Container");
            cloneContainer.gameObject.SetActive(false);
            var clone = GameObject.Instantiate(target, cloneContainer.transform);

            clone.transform.resetTransform(false);

            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            if (!isIncludeChildren)
            {
                var renderer = clone.GetComponent<Renderer>();
                bounds = renderer ? renderer.bounds : bounds;
            }
            else
            {
                foreach (Renderer r in clone.GetComponentsInChildren<Renderer>())
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
            cloneContainer.gameObject.SetActive(false);
            var clone = GameObject.Instantiate(target, cloneContainer.transform);

            clone.transform.resetTransform(false);
            var bounds = clone.getLocalBounds(isIncludeChildren);
            targetBoxCollider.center = bounds.center;
            targetBoxCollider.size = bounds.size;

            NEditorUtils.setDirty(target);
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

            if (applyPosition)
            {
                target.transform.position = cloneFrom.transform.position;
            }
            if (applyRotation)
            {
                target.transform.rotation = cloneFrom.transform.rotation;
            }
            if (applyScale)
            {
                if (!target.transform.parent || target.transform.lossyScale.hasZeroAxis())
                {
                    if (cloneFrom.transform.lossyScale.hasZeroAxis())
                    {
                        target.transform.localScale = cloneFrom.transform.localScale;
                    }
                    else
                    {
                        target.transform.localScale = cloneFrom.transform.localScale.div(cloneFrom.transform.lossyScale);
                    }
                }
                else
                {
                    target.transform.localScale = cloneFrom.transform.lossyScale.div(target.transform.lossyScale);
                }
            }

            target.removeComponents<Collider>();
            target.keepSingleComponent<BoxCollider>();

            var boxCollider = target.GetComponent<BoxCollider>();
            if (!boxCollider)
            {
                boxCollider = target.AddComponent<BoxCollider>();
            }

            if (applyPosition)
            {
                boxCollider.center = target.transform.InverseTransformPoint(cloneFrom.transform.TransformPoint(cloneFrom.center));
            }
            else
            {
                boxCollider.center = cloneFrom.center;
            }

            if (cloneFrom.transform.lossyScale.hasZeroAxis())
            {
                boxCollider.size = cloneFrom.size;
            }
            else
            {
                if (target.transform.lossyScale.hasZeroAxis())
                {
                    boxCollider.size = cloneFrom.size.mul(cloneFrom.transform.lossyScale);
                }
                else
                {
                    boxCollider.size = cloneFrom.size.mul(cloneFrom.transform.lossyScale.div(target.transform.lossyScale));
                }
            }

            NEditorUtils.setDirty(target);
            NUtils.destroyObject(cloneFrom, true);
            return boxCollider;
        }
        public static KeepScale safeSetParent(this Transform transform, Transform parent)
        {
            //var f = new GameObject("SafeParent").AddComponent<FollowTransform>();
            //f.setup(transform, parent);
            var kScale = transform.gameObject.AddComponent<KeepScale>().setup();
            transform.SetParent(parent);
            return kScale;
        }
        #endregion

        #region Animator Utils
        public static void setIntegerFromTo(this Animator animator, int fromHash, int toHash)
        {
            var value = animator.GetInteger(fromHash);
            animator.SetInteger(toHash, value);
        }
        #endregion

        #region C# Type Utils
        public static object createInstance(this Type type)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length > 0)
            {
                if (constructors[0].GetParameters().Length == 0)
                {
                    return constructors[0].Invoke(null);
                }
            }
            return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        }
        public static T createInstance<T>(this Type type)
        {
            return (T)type.createInstance();
        }

        private static Type[] _typeCached;
        public static Type[] getCustomTypes()
        {
            if (_typeCached == null)
            {
                var typeList = new List<Type>();
                var assembles = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assembles)
                {
                    if (assembly.GetName().Name.ToLower().StartsWith("unity"))
                    {
                        continue;
                    }
                    if (assembly.GetName().Name.ToLower().StartsWith("nunit."))
                    {
                        continue;
                    }
                    if (assembly.GetName().Name == "mscorlib")
                    {
                        continue;
                    }
                    if (assembly.GetName().Name == "ExCSS.Unity")
                    {
                        continue;
                    }
                    if (assembly.GetName().Name.StartsWith("System"))
                    {
                        continue;
                    }
                    if (assembly.GetName().Name.StartsWith("Mono."))
                    {
                        continue;
                    }
                    var types = assembly.GetTypes();
                    typeList.AddRange(types);
                }
                _typeCached = typeList.ToArray();
                if (Application.isPlaying)
                {
                    _ = NAwaiter.runDelay(1, () =>
                    {
                        _typeCached = null;
                    });
                }
            }
            return _typeCached;
        }
        public static bool isInherited(Type child, Type parent)
        {
            while (child != null)
            {
                child = child.BaseType;
                if (child != null && child == parent)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Others
        public static void dispose(params IDisposable[] disposeables)
        {
            for (int i = 0; i < disposeables.Length; i++)
            {
                disposeables[i].Dispose();
            }
        }
        #endregion
    }
}