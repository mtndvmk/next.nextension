namespace Nextension
{
    public interface IPoolable
    {
        // 0 (Available), > 0 (Rented)
        ref long RentNum { get; }
        void onCreated() { }
        void onSpawn() { }
        void onDespawn() { }
        void onDestroy() { }
    }
}