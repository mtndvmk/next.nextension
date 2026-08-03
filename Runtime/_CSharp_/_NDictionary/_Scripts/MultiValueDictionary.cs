using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public class MultiValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
    {
        public struct ValueCollection : IEnumerable<TValue>
        {
            private int _count;
            private NList<TValue> _group;
            private TValue _single;

            public readonly int Count => _count;
            public readonly bool IsEmpty => Count == 0;

            public TValue Get(int index)
            {
                if (index >= Count) throw new IndexOutOfRangeException();
                if (_group == null) return _single;
                return _group[index];
            }

            public ValueCollection(TValue value)
            {
                this._single = value;
                this._group = null;
                this._count = 1;
            }

            internal void Add(TValue value)
            {
                if (_count == 0)
                {
                    _single = value;
                    _group = null;
                    _count = 1;
                    return;
                }
                if (_group == null)
                {
                    _group = NStaticPool<NList<TValue>>.get();
                    _group.Add(_single);
                }
                _group.Add(value);
                _count++;
            }

            internal bool Remove(TValue value)
            {
                if (_count == 0)
                {
                    return false;
                }
                if (_group == null)
                {
                    if (_single.equals(value))
                    {
                        _count = 0;
                        return true;
                    }
                    return false;
                }
                else
                {
                    bool removed = _group.Remove(value);
                    if (removed)
                    {
                        _count--;
                        if (_count == 1)
                        {
                            _single = _group[0];
                            NStaticPool<NList<TValue>>.release(_group);
                            _group = null;
                        }
                    }
                    return removed;
                }
            }

            internal void Clear()
            {
                if (_group != null)
                {
                    NStaticPool<NList<TValue>>.release(_group);
                    _group = null;
                }
                _count = 0;
            }

            public readonly ArrayEnumerator<TValue> GetEnumerator()
            {
                if (Count == 0) return new ArrayEnumerator<TValue>(Array.Empty<TValue>());
                if (_group == null) return new ArrayEnumerator<TValue>(_single);
                return _group.GetEnumerator();
            }

            readonly IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            readonly IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private readonly Dictionary<TKey, ValueCollection> _store = new();

        public void Add(TKey key, TValue value)
        {
            if (!_store.ContainsKey(key))
            {
                _store[key] = new ValueCollection(value);
            }
            else
            {
                _store[key].Add(value);
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            if (!_store.TryGetValue(key, out var container)) return false;
            bool removed = container.Remove(value);
            if (removed && container.Count == 0)
            {
                _store.Remove(key);
            }
            return removed;
        }

        public ValueCollection GetValues(TKey key)
        {
            return _store.GetValueOrDefault(key);
        }

        public Dictionary<TKey, ValueCollection>.KeyCollection Keys => _store.Keys;

        public Dictionary<TKey, ValueCollection>.Enumerator GetEnumerator() => _store.GetEnumerator();

        public void Clear()
        {
            foreach (var container in _store.Values)
            {
                container.Clear();
            }
            _store.Clear();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach ((var key, var values) in _store)
            {
                foreach (var v in values)
                {
                    yield return new KeyValuePair<TKey, TValue>(key, v);
                }            
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

