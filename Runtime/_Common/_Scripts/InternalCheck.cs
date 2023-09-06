using System;
using UnityEngine;

namespace Nextension
{
    internal static class InternalCheck
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        internal static void checkEditorMode(string note = null)
        {
            if (!Application.isPlaying)
            {
                var err = "Not support editor mode";
                if (!string.IsNullOrEmpty(note))
                {
                    err += ": " + note;
                }
                throw new Exception(err);
            }
        }
        internal static void checkValidArray(Array inData, int startIndex, int lengthRequired)
        {
            if (inData == null)
            {
                NThrowHelper.throwArgNullException("inData");
            }

            if ((uint)startIndex >= inData.Length)
            {
                NThrowHelper.throwArgOutOfRangeException("startIndex", startIndex, inData.Length);
            }

            if (startIndex > inData.Length - lengthRequired)
            {
                NThrowHelper.throwArrayLengthToSmallException("inData", startIndex, inData.Length, lengthRequired);
            }
        }
        internal static void checkValidArray<T>(Span<T> inData, int startIndex, int lengthRequired)
        {
            if (inData == null)
            {
                NThrowHelper.throwArgNullException("inData");
            }

            if ((uint)startIndex >= inData.Length)
            {
                NThrowHelper.throwArgOutOfRangeException("startIndex", startIndex, inData.Length);
            }

            if (startIndex > inData.Length - lengthRequired)
            {
                NThrowHelper.throwArrayLengthToSmallException("inData", startIndex, inData.Length, lengthRequired);
            }
        }
    }

    internal static class NThrowHelper
    {
        public static void throwException(Exception exception)
        {
            throw exception;
        }
        public static void throwArgNullException(string name)
        {
            throw new ArgumentNullException(name);
        }
        public static void throwArgOutOfRangeException(string name, int index, int range)
        {
            throw new ArgumentOutOfRangeException($"{name} - [index: {index}][length: {range}]");
        }
        public static void throwArrayLengthToSmallException(string name, int startIndex, int length, int lengthRequired)
        {
            throw new ArgumentException($"ArrayLengthToSmall: {name} - [{startIndex}/{length}][length required: {lengthRequired}]");
        }
    }
}
