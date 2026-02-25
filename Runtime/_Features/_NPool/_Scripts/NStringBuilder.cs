using System;

namespace Nextension
{
    public readonly struct NStringBuilder : IDisposable
    {
        public static NStringBuilder get()
        {
            return new NStringBuilder(NPUArray<char>.get());
        }
        public static NStringBuilder getWithoutTracking()
        {
            return new NStringBuilder(NPUArray<char>.getWithoutTracking());
        }
        public static NStringBuilder get(int capacity)
        {
            return new NStringBuilder(NPUArray<char>.get(capacity));
        }
        public static NStringBuilder getWithoutTracking(int capacity)
        {
            return new NStringBuilder(NPUArray<char>.getWithoutTracking(capacity));
        }

        private NStringBuilder(NPUArray<char> arr)
        {
            _charArray = arr;
        }

        public readonly char this[int index] => _charArray[index];

        private readonly NPUArray<char> _charArray;
        public readonly int Count => _charArray.Count;
        public readonly bool IsCreated => _charArray.IsCreated;
        public readonly bool IsReadOnly => _charArray.IsReadOnly;

        public void stopTracking()
        {
            _charArray.stopTracking();
        }

        public NStringBuilder Append(char item)
        {
            _charArray.Add(item);
            return this;
        }
        public NStringBuilder AppendLine()
        {
            _charArray.Add('\n');
            return this;
        }
        public NStringBuilder Insert(int index, char item)
        {
            _charArray.Insert(index, item);
            return this;
        }
        public NStringBuilder Append(string value)
        {
            return Insert(Count, value);
        }
        public NStringBuilder AppendLine(string value)
        {
            Insert(Count, value);
            AppendLine();
            return this;
        }
        public unsafe NStringBuilder Insert(int index, string value)
        {
            fixed (char* p = value)
            {
                _charArray.InsertRange(index, p, value.Length);
                return this;
            }
        }
        public NStringBuilder Append(ReadOnlySpan<char> value)
        {
            return Insert(Count, value);
        }
        public NStringBuilder AppendLine(ReadOnlySpan<char> value)
        {
            Insert(Count, value);
            AppendLine();
            return this;
        }
        public unsafe NStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            fixed (char* p = value)
            {
                _charArray.InsertRange(index, p, value.Length);
                return this;
            }
        }
        public NStringBuilder SetValue(int index, char value)
        {
            _charArray[index] = value;
            return this;
        }
        public void Clear()
        {
            _charArray.Clear();
        }
        public readonly bool Contains(char item)
        {
            return _charArray.Contains(item);
        }
        public bool Remove(char item)
        {
            return _charArray.Remove(item);
        }
        public void Dispose()
        {
            _charArray.Dispose();
        }
        public readonly unsafe override string ToString()
        {
            fixed (byte* bPtr = _charArray.i_array.Collection.i_Items)
            {
                return new string((char*)bPtr, 0, Count);
            }
        }
        public readonly unsafe string ToString(int startIndex, int length)
        {
            if (startIndex < 0 || length <= 0 || startIndex + length >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            fixed (byte* bPtr = _charArray.i_array.Collection.i_Items)
            {
                return new string((char*)bPtr, startIndex, length);
            }
        }
    }
}
