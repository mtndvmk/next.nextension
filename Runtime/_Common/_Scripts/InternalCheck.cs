using System;

namespace Nextension
{
    internal static class InternalCheck
    {
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
        internal static void checkValidArray<T>(ReadOnlySpan<T> inData, int startIndex, int lengthRequired)
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

    public static class EditorCheck
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        internal static void checkEditorMode(string note = null)
        {
            if (!NStartRunner.IsPlaying)
            {
                var err = "Not support editor mode";
                if (!string.IsNullOrEmpty(note))
                {
                    err += ": " + note;
                }
                throw new Exception(err);
            }
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        internal static void requireNotNull<T>(T target)
        {
            if (target == null) throw new ArgumentNullException("target");
        }
    }

    internal static class NThrowHelper
    {
        public static void throwKeepStackTraceException(Exception exception)
        {
            throw new KeepStackTraceException(exception);
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
