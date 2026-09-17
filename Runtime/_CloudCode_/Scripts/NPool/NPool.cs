
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nextension
{
    public readonly struct LeasedItem<T> : IDisposable where T : class
    {
        public readonly NPool<T> pool;
        public readonly T value;
        public readonly long returnToken;

        public LeasedItem(NPool<T> pool, T value)
        {
            this.pool = pool;
            this.value = value;
            this.returnToken = (value is IPoolable poolable) ? poolable.RentNum : 1;
        }

        public LeasedItem(NPool<T> pool, T value, long returnToken)
        {
            this.pool = pool;
            this.value = value;
            this.returnToken = returnToken;
        }

        public readonly void Dispose()
        {
            pool?.SafeReturn(value, returnToken);
        }

        public static implicit operator T(LeasedItem<T> leasedItem) => leasedItem.value;
    }

    public interface IInsCreator<T>
    {
        T Create();

        // return true if allow return to pool
        bool Reset(T item);
    }

    public sealed class InsCreator<T> : IInsCreator<T>
    {
        public readonly static InsCreator<T> Default = new();

        private InsCreator() { }

        public T Create()
        {
            return ObjectFactory.createInstance<T>();
        }

        public bool Reset(T item)
        {
            return true;
        }
    }

    public sealed class NPool<T> where T : class
    {
        // Explicit 64-byte padding matching standard CPU Cache Line boundaries
        [StructLayout(LayoutKind.Sequential)]
        private struct Element
        {
            public T Value; // 8 bytes (reference pointer)

            // 56 bytes padding
            private long _p1, _p2, _p3, _p4, _p5, _p6, _p7;
        }

        private static readonly bool _isPoolable = typeof(IPoolable).IsAssignableFrom(typeof(T));

        private static class SharedHolder
        {
            public readonly static NPool<T> sharedPool = new NPool<T>(Math.Max(16, Environment.ProcessorCount * 2));
        }

        public static NPool<T> Shared => SharedHolder.sharedPool;

        private readonly IInsCreator<T> _insCreator;
        private readonly Element[] _items;
        private T _firstItem; // Fast-path single item
        private int _totalItemsCreated;
        private long _globalRentCounter = 1;

        public int TotalItemsCreated => _totalItemsCreated;

        public NPool() : this(null, 16) { }

        public NPool(int capacity) : this(null, capacity)
        {
        }

        public NPool(IInsCreator<T> insCreator, int capacity = 16)
        {
            _insCreator = insCreator ?? InsCreator<T>.Default;
            int arrayCapacity = Math.Max(0, capacity - 1);
            _items = new Element[arrayCapacity];
        }

        private long __generateNextRentNum()
        {
            long next = Interlocked.Add(ref _globalRentCounter, 2);
            return (next & 0x7FFFFFFFFFFFFFFF) | 1L;
        }

        private static void __onRented(T item, long rentNum)
        {
            if (rentNum == 0) rentNum = 1;
            if (item is IPoolable poolable)
            {
                Volatile.Write(ref poolable.RentNum, rentNum);
                poolable.onSpawn();
            }
        }

        public LeasedItem<T> Rent()
        {
            var rentNum = _isPoolable ? __generateNextRentNum() : 1;
            // 1. Fast Path
            T item = _firstItem;
            if (item != null && Interlocked.CompareExchange(ref _firstItem, null, item) == item)
            {
                __onRented(item, rentNum);
                return new LeasedItem<T>(this, item, rentNum);
            }

            // 2. Slow Path: Scan cache-line padded array
            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                item = items[i].Value;
                if (item != null && Interlocked.CompareExchange(ref items[i].Value, null, item) == item)
                {
                    __onRented(item, rentNum);
                    return new LeasedItem<T>(this, item, rentNum);
                }
            }

            // 3. Allocate via creator
            item = _insCreator.Create();
            Interlocked.Increment(ref _totalItemsCreated);
            if (item is IPoolable poolable)
            {
                poolable.onCreated();
            }
            __onRented(item, rentNum);
            return new LeasedItem<T>(this, item, rentNum);
        }

        public bool Return(T item)
        {
            long returnToken = (item is IPoolable poolable) ? poolable.RentNum : 1;
            return SafeReturn(item, returnToken);
        }

        public bool SafeReturn(T item, long rentNum)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item is IPoolable poolable)
            {
                if (poolable.RentNum == 0) return false;

                if (Interlocked.CompareExchange(ref poolable.RentNum, 0, rentNum) != rentNum)
                {
                    return false;
                }

                poolable.onDespawn();
            }

            // Clean state; discard if policy rejects (e.g. oversize buffer)
            if (!_insCreator.Reset(item))
            {
                if (item is IPoolable p) p.onDestroy();
                return true;
            }

            // Fast Path Return
            if (_firstItem == null && Interlocked.CompareExchange(ref _firstItem, item, null) == null)
            {
                return true;
            }

            // Slow Path Return
            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Value == null && Interlocked.CompareExchange(ref items[i].Value, item, null) == null)
                {
                    return true;
                }
            }

            // Excess objects beyond pool capacity fall through to standard Garbage Collection
            if (item is IPoolable p2) p2.onDestroy();
            return true;
        }

        public void Clear()
        {
            T first = Interlocked.Exchange(ref _firstItem, null);
            if (first != null)
            {
                if (first is IPoolable p) p.onDestroy();
                if (first is IDisposable disposableFirst)
                {
                    disposableFirst.Dispose();
                }
            }

            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                T item = Interlocked.Exchange(ref items[i].Value, null);
                if (item != null)
                {
                    if (item is IPoolable p) p.onDestroy();
                    if (item is IDisposable disposableItem)
                    {
                        disposableItem.Dispose();
                    }
                }
            }
        }
    }
}