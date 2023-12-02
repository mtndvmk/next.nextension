using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nextension.Tween
{
    internal class CancelControlManager
    {
        private Dictionary<long, HashSet<NTweener>> _controlledTweeners = new();
        private Dictionary<long, Object> _objectKeys = new();
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
            if (!_objectKeys.ContainsKey(longKey))
            {
                _objectKeys.Add(longKey, key);
            }
            return objectKey;
        }
        public bool isInvalid(CancelControlKey key)
        {
            if (_objectKeys.TryGetValue(key.LongKey, out var target) && !target) return false;
            return true;
        }

        public void addTweener(NTweener tweener)
        {
            if (!_controlledTweeners.TryGetValue(tweener.controlKey.LongKey, out var hashset))
            {
                hashset = new(1) { tweener };
                _controlledTweeners.Add(tweener.controlKey.LongKey, hashset);
            }
            else
            {
                if (!hashset.Contains(tweener))
                {
                    hashset.Add(tweener);
                }
            }
        }
        public void removeTweener(NTweener tweener)
        {
            if (_controlledTweeners.TryGetValue(tweener.controlKey.LongKey, out var hashset))
            {
                if (hashset.Remove(tweener) && hashset.Count == 0)
                {
                    _controlledTweeners.Remove(tweener.controlKey.LongKey);
                }
            }
        }
        public void cancel(long longKey)
        {
            if (_controlledTweeners.TryGetValue(longKey, out var hashset))
            {
                using var array = hashset.toNPArray();
                foreach (var item in array.asSpan())
                {
                    item.cancel();
                }
                _controlledTweeners.Remove(longKey);
            }
        }
        public void cancelInvalid()
        {
            if (_controlledTweeners.Count > 0)
            {
                var keys = _objectKeys.ToArray();
                foreach (var k in keys)
                {
                    if (!k.Value)
                    {
                        cancel(k.Key);
                        _objectKeys.Remove(k.Key);
                    }
                }
            }
        }
    }
}
