namespace Nextension
{
    public class AbsPoolableAwaiter<T> : AbsAwaiter where T : AbsPoolableAwaiter<T>
    {
        protected static T __getNext()
        {
            var awaiter = NLockedPool<T>.get();
            awaiter.updateId();
            return awaiter;
        }
        protected static void __release(T awaiter)
        {
            NLockedPool<T>.release(awaiter);
        }
    }
}
