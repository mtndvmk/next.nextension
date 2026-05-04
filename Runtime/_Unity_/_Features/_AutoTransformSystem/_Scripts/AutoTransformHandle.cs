namespace Nextension
{
    public class AutoTransformHandle : IPoolable
    {
        public int Index { get; internal set; }

        private AutoTransformHandle()
        {
        }

        internal static AutoTransformHandle create(int index)
        {
            var handle = NStaticPool<AutoTransformHandle>.get();
            handle.Index = index;
            return handle;
        }
        internal static void release(AutoTransformHandle handle)
        {
            handle.Index = -1;
            NStaticPool<AutoTransformHandle>.release(handle);
        }
        public void stop()
        {
            AutoTransformSystem.stop(this);
        }
    }

    public static class AutoTransformHandleExtensions
    {
        public static bool isValid(this AutoTransformHandle handle)
        {
            return handle != null && handle.Index >= 0;
        }
    }
}
