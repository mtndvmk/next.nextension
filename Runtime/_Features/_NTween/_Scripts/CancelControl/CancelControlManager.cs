using System.Collections.Generic;
using UnityEngine;

namespace Nextension.Tween
{
    internal class CancelControlManager
    {
        private readonly SimpleDictionary<long, HashSet<NTweener>> _controlledTweeners = new();
        private readonly SimpleDictionary<long, Object> _objectKeys = new();
        public CancelControlKey createKey(uint key)
        {
            if (key == 0) throw new System.Exception("key cannot equals 0");
            return new CancelControlKey((long)key << 32);
        }
        public CancelControlKey createKey(Object key)
        {
            if (key == null) throw new System.Exception("key cannot be null");
            var longKey = CancelControlKey.getLongKey(key);
            var objectKey = new CancelControlKey(longKey);
            _objectKeys.tryAdd(longKey, key);
            return objectKey;
        }
        public bool isInvalid(CancelControlKey key)
        {
            if (_objectKeys.TryGetValue(key.longKey, out var target) && !target) return false;
            return true;
        }

        public void addTweener(NTweener tweener)
        {
            var key = tweener.controlKey.longKey;
            if (!_controlledTweeners.TryGetValue(key, out var hashset))
            {
                hashset = new() { tweener };
                _controlledTweeners.Add(key, hashset);
            }
            else
            {
                hashset.Add(tweener);
            }
        }
        public void removeTweener(NTweener tweener)
        {
            var key = tweener.controlKey.longKey;
            if (_controlledTweeners.TryGetValue(key, out var hashset))
            {
                if (hashset.Remove(tweener) && hashset.Count == 0)
                {
                    _controlledTweeners.Remove(key);
                }
            }
        }
        public void cancel(long longKey)
        {
            if (_controlledTweeners.tryTakeAndRemove(longKey, out var hashset))
            {
                using var array = hashset.toNPArray();
                foreach (var item in array.AsSpan())
                {
                    item.cancel();
                }
            }
            if (CancelControlKey.isObjectKey(longKey))
            {
                _objectKeys.Remove(longKey);
            }
        }
        public void cancelInvalid()
        {
            if (_controlledTweeners.Count > 0 && _objectKeys.Count > 0)
            {
                using var keys = NPUArray<long>.get();
                foreach (var (k, obj) in _objectKeys)
                {
                    if (!obj) keys.Add(k);
                }
                foreach (var k in keys)
                {
                    cancel(k);
                }
            }
        }
#if UNITY_EDITOR
        public void clear()
        {
            _controlledTweeners.Clear();
            _objectKeys.Clear();
        }
#endif
    }
}
