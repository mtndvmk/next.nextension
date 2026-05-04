using System;

namespace Nextension
{
    internal static class NExceptionHelper
    {
        public readonly static Exception CanceledException = new OperationCanceledException();
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
