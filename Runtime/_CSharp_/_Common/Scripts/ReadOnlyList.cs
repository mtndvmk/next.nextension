using System;
using System.Collections;
using System.Collections.Generic;

namespace Nextension
{
    public readonly struct ReadOnlyList<T>
    {
        internal readonly IReadOnlyList<T> i_list;
        
        private readonly int _start;
        private readonly int _length;

        public readonly T this[int index] => i_list[_start + index];
        public readonly int Count
        {
            get
            {
                if (i_list != null)
                {
                    var len = i_list.Count;
                    if (_start >= len)
                    {
                        return 0;
                    }
                    return Math.Min(_length, len - _start);
                }
                else
                {
                    return 0;
                }
            }
        }
        public readonly bool IsCreated => i_list != null;

        public ReadOnlyList(IReadOnlyList<T> nList)
        {
            i_list = nList;
            _start = 0;
            _length = int.MaxValue;
        }
        
        public ReadOnlyList(IReadOnlyList<T> nList, int length)
        {
            i_list = nList;
            _start = 0;
            _length = length;
        }

        public ReadOnlyList(IReadOnlyList<T> nList, int start, int length)
        {
            i_list = nList;
            _start = start;
            _length = length;
        }

        public readonly ListEnumerator<T> GetEnumerator()
        {
            return new ListEnumerator<T>(i_list, _start, Count);
        }

        public unsafe readonly Span<T> AsSpan()
        {
            if (Count == 0) return default;
            if (i_list is T[] array)
            {
                return array.AsSpan().Slice(_start, Count);
            }
            if (i_list is List<T> list)
            {
                return list.asSpan().Slice(_start, Count);
            }
            throw new NotSupportedException();
        }

        public static implicit operator ReadOnlyList<T>(List<T> list) => new ReadOnlyList<T>(list);
        
        public static implicit operator ReadOnlyList<T>(NList<T> list) => new ReadOnlyList<T>(list);
        
        public static implicit operator ReadOnlyList<T>(T[] array) => new ReadOnlyList<T>(array);
    }

    public static class ROListExtension
    {
        public static void AddRange<T>(this List<T> list, ReadOnlyList<T> roList)
        {
            for (int i = 0; i < roList.Count; i++)
            {
                list.Add(roList[i]);
            }
        }
        public static void AddRange<T>(this NList<T> list, ReadOnlyList<T> roList)
        {
            list.AddRange(roList.AsSpan());
        }
    }
}