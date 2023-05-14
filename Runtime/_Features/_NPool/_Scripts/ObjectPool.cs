using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    public static Dictionary<int, Queue<PoolIdentity>> Pool { get; set; } = new Dictionary<int, Queue<PoolIdentity>>();
    private static PoolIdentity findObject(int id)
    {
        if (Pool.ContainsKey(id))
        {
            var queue = Pool[id];
            if (queue == null || queue.Count == 0)
            {
                return null;
            }
            var o = queue.Dequeue();
            o.gameObject.SetActive(true);
            return o;
        }
        return null;
    }
    public static T getObject<T>(T prefab, Transform parent = null, bool worldPositionStays = true) where T : Component
    {
        var obj = getObject(prefab.gameObject, parent, worldPositionStays);
        return obj.GetComponent<T>();
    }
    public static GameObject getObject(GameObject gameObject, Transform parent = null, bool worldPositionStays = true)
    {
        if (gameObject == null)
        {
            Debug.LogWarning("gameObject is null");
            return null;
        }
        var poolId = gameObject.GetComponent<PoolIdentity>();
        if (poolId == null)
        {
            Debug.LogWarning("No have PoolIdentity");
            return null;
        }
        var o = findObject(poolId.Id);
        if (o == null)
        {
            o = GameObject.Instantiate(gameObject).GetComponent<PoolIdentity>();
        }
        o.transform.SetParent(parent, worldPositionStays);
        o.invokeRespawnedEvent();
        return o.gameObject;
    }
    public static void sendToPool(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }
        var poolId = gameObject.GetComponent<PoolIdentity>();
        if (poolId == null)
        {
            Debug.Log("No have PoolIdentity");
            return;
        }

        poolId.gameObject.SetActive(false);
        if (!Pool.ContainsKey(poolId.Id))
        {
            Pool.Add(poolId.Id, new Queue<PoolIdentity>());
        }
        var queue = Pool[poolId.Id];
        queue.Enqueue(poolId);
        poolId?.invokeSentToPoolEvent();
    }
    public static void cleanPool()
    {
        foreach (var queue in Pool.Values)
        {
            while (queue.Count > 0)
            {
                GameObject.Destroy(queue.Dequeue());
            }
        }
        Pool.Clear();
    }
}
