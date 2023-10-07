using System.Runtime.CompilerServices;

namespace Nextension
{
    public enum RunState : byte
    {
        None = 0,
        Running = 1,
        Canceled = 2,
        Completed = 3,
        Exception = 4,
    }
    public enum CompleteState : byte
    {
        None,
        Canceled = 2,
        Completed = 3,
        Exception = 4,
    }
    public static class NStateExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isFinished(this RunState state)
        {
            return state >= RunState.Canceled;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isFinished(this CompleteState state)
        {
            return state >= CompleteState.Canceled;
        }
    }
}
