using System;

namespace Nextension
{
    internal static class InternalCheck
    {
        internal static void checkValidArray(Array inData, int startIndex, int lengthRequired)
        {
            if (inData == null)
            {
                NExceptionHelper.throwArgNullException("inData");
            }

            if ((uint)startIndex >= inData.Length)
            {
                NExceptionHelper.throwArgOutOfRangeException("startIndex", startIndex, inData.Length);
            }

            if (startIndex > inData.Length - lengthRequired)
            {
                NExceptionHelper.throwArrayLengthToSmallException("inData", startIndex, inData.Length, lengthRequired);
            }
        }
        internal static void checkValidArray<T>(ReadOnlySpan<T> inData, int startIndex, int lengthRequired)
        {
            if (inData == null)
            {
                NExceptionHelper.throwArgNullException("inData");
            }

            if ((uint)startIndex >= inData.Length)
            {
                NExceptionHelper.throwArgOutOfRangeException("startIndex", startIndex, inData.Length);
            }

            if (startIndex > inData.Length - lengthRequired)
            {
                NExceptionHelper.throwArrayLengthToSmallException("inData", startIndex, inData.Length, lengthRequired);
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
}
