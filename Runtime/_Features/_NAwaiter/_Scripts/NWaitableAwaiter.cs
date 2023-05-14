namespace Nextension
{
    public class NWaitableAwaiter : AbsAwaiter, ICancellable
    {
        internal NWaitableHandle handle;
        public NWaitableAwaiter(IWaitable waitable)
        {
            var handle = new NWaitableHandle(waitable);
            setup(handle);
        }
        public NWaitableAwaiter(IWaitableFromCancellable waitable)
        {
            var handle = new NWaitableHandle(waitable);
            setup(handle);
        }
        public NWaitableAwaiter(IWaitable_Editor waitable)
        {
            var handle = new NWaitableHandle(waitable);
            setup(handle);
        }
        internal NWaitableAwaiter(NWaitableHandle handle)
        {
            setup(handle);
        }
        public NWaitableAwaiter(NWaitable waitable)
        {
            if (waitable.Status == RunState.Completed)
            {
                invokeComplete();
            }
            else if (waitable.Status == RunState.None)
            {
                var handle = new NWaitableHandle((waitable as IWaitable).buildCompleteFunc());
                setup(handle);
            }
        }
        internal void setup(NWaitableHandle handle)
        {
            this.handle = handle;
            if (handle.Status == RunState.Completed)
            {
                invokeComplete();
            }
            else if (handle.Status == RunState.None)
            {
                handle.Status = RunState.Running;
                handle.setContinuationFunc(invokeComplete);
#if UNITY_EDITOR
                if (handle.isEditorWaitable)
                {
                    NAwaiter_EdtitorLoop.addAwaitable(handle);
                    return;
                }
#endif
                NAwaiterLoop.addAwaitable(handle);
            }
        }

        public void cancel()
        {
            handle.cancel();
        }
    }
    public class NWaitableAwaiter<T> : NWaitableAwaiter
    {
        private NWaitable<T> waitable;
        public NWaitableAwaiter(NWaitable<T> waitable) : base(waitable)
        {
            this.waitable = waitable;
        }
        public new T GetResult() => waitable.Result;
    }
}
