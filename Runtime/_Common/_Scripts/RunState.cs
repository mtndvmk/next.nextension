using System.Runtime.CompilerServices;

namespace Nextension
{
    public enum RunState : byte
    {
        None = 0,
        Running = 1,
        Cancelled = 2,
        Completed = 3,
        Exception = 4,
    }
    public enum CompleteState : byte
    {
        None,
        Cancelled = 2,
        Completed = 3,
        Exception = 4,
    }
    public static class NStateExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isFinished(this RunState state)
        {
            return state >= RunState.Cancelled;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isFinished(this CompleteState state)
        {
            return state >= CompleteState.Cancelled;
        }
    }
}
