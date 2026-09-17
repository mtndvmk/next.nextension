using System.Threading;

namespace Nextension
{
    public abstract class AbsPoolable : IPoolable
    {
        private long _rentNum;

        ref long IPoolable.RentNum => ref _rentNum;

        public bool IsRented => Volatile.Read(ref _rentNum) != 0;

        public long ReturnToken => Volatile.Read(ref _rentNum);

        void IPoolable.onCreated() => onCreated();
        void IPoolable.onSpawn() => onSpawn();
        void IPoolable.onDespawn() => onDespawn();
        void IPoolable.onDestroy() => onDestroy();

        protected virtual void onCreated() { }
        protected virtual void onSpawn() { }
        protected virtual void onDespawn() { }
        protected virtual void onDestroy() { }
    }
}