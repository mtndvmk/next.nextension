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
            private PNList<TValue> _group;
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
                    _group = PNList<TValue>.getWithoutTracking();
                    _group.Add(_single);
                }
                _group.Add(value);
                _count++;
            }

            internal bool AddIfNotPresent(TValue value)
            {
                if (_count == 0)
                {
                    _single = value;
                    _group = null;
                    _count = 1;
                    return true;
                }

                if (EqualityComparer<TValue>.Default.Equals(_single, value)) return false;

                if (_group == null)
                {
                    _group = PNList<TValue>.getWithoutTracking();
                    _group.Add(_single);
                }
                else
                {
                    foreach (var item in _group)
                    {
                        if (EqualityComparer<TValue>.Default.Equals(item, value)) return false;
                    }   
                }

                _group.Add(value);
                _count++;
                return true;
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
                            _group.Dispose();
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
                    _group.Dispose();
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

        private readonly SimpleDictionary<TKey, ValueCollection> _stored = new();

        public void Add(TKey key, TValue value)
        {
            ref var refVal = ref _stored.GetAsRef(key);
            if (!refVal.isNullRef())
            {
                refVal.Add(value);
            }
            else
            {
                _stored[key] = new ValueCollection(value);
            }
        }

        public bool AddIfNotPresent(TKey key, TValue value)
        {
            ref var refVal = ref _stored.GetAsRef(key);
            if (!refVal.isNullRef())
            {
                return refVal.AddIfNotPresent(value);
            }
            else
            {
                _stored[key] = new ValueCollection(value);
                return true;
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            if (!_stored.ContainsKey(key)) return false;
            ref var container = ref _stored.GetAsRef(key);
            bool removed = container.Remove(value);
            if (removed && container.Count == 0)
            {
                _stored.Remove(key);
            }
            return removed;
        }

        public ValueCollection GetValues(TKey key)
        {
            return _stored.TryGetValue(key, out var container) ? container : default;
        }

        public SimpleDictionary<TKey, ValueCollection>.Enumerator GetEnumerator() => _stored.GetEnumerator();

        public void Clear()
        {
            foreach (var container in _stored)
            {
                container.Value.Clear();
            }
            _stored.Clear();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var kvp in _stored)
            {
                foreach (var v in kvp.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, v);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

