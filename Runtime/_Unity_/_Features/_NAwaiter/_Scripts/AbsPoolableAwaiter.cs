namespace Nextension
{
    public class AbsPoolableAwaiter<T> : AbsAwaiter where T : AbsPoolableAwaiter<T>
    {
        protected static T __getNext()
        {
            var awaiter = NPool<T>.Shared.Rent().value;
            awaiter.updateId();
            return awaiter;
        }
        protected static void __release(T awaiter)
        {
            NPool<T>.Shared.Return(awaiter);
        }
    }
}
