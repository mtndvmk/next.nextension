using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public interface IPoolable
    {
        void onSpawned() { }
        void onDespawned() { }
        void onDestroyed() { }
    }
    public class NPool<T> where T : class, IPoolable
    {
        private const uint MAX_CAPACITY_DEFAULT = 100;
        private NBListUseHashCode<T> pool;
        private static Type typeOfT = typeof(T);

        public int CountAll { get; private set; }
        public int PoolCount => pool.Count;
        public uint MaxCapacity { get; private set; }
        public NPool()
        {
            pool = new NBListUseHashCode<T>();
            MaxCapacity = MAX_CAPACITY_DEFAULT;
        }
        public T get()
        {
            T item;
            if (pool.Count > 0)
            {
                item = pool[0];
                pool.removeAt(0);
            }
            else
            {
                item = typeOfT.createInstance<T>();
                CountAll++;
            }
            item.onSpawned();
            return item;
        }
        public bool release(T item)
        {
            if (pool.bContains(item))
            {
                return false;
            }

            if (pool.Count >= MaxCapacity) 
            {
                item.onDestroyed();
                return true;
            }
            else
            {
                item.onDespawned();
                pool.addAndSort(item);
                return true;
            }
        }
        public void clear()
        {
            pool.clear();
        }
    }
}
