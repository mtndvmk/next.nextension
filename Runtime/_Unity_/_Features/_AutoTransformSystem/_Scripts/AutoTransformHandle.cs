namespace Nextension
{
    public class AutoTransformHandle : AbsPoolable
    {
        public int Index { get; internal set; }

        internal int autoStopIndex;

        private AutoTransformHandle()
        {
        }

        internal static AutoTransformHandle create(int index)
        {
            var handle = NPool<AutoTransformHandle>.Shared.Rent().value;
            handle.Index = index;
            return handle;
        }
        internal static void release(AutoTransformHandle handle)
        {
            handle.Index = -1;
            NPool<AutoTransformHandle>.Shared.Return(handle);
        }
        public void stop()
        {
            AutoTransformSystem.stop(this);
        }

        public AutoTransformHandle stopOnDisable()
        {
            if (this.isValid())
            {
                autoStopIndex = Index;
                AutoTransformSystem.stopOnDisable(this);
            }
            return this;
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
