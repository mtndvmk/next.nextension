using System;

namespace Nextension
{
    public struct NStringBuilder : IDisposable
    {
        public static NStringBuilder get()
        {
            return new NStringBuilder()
            {
                _charArray = NPUArray<char>.get()
            };
        }
        public static NStringBuilder getWithoutTracking()
        {
            return new NStringBuilder()
            {
                _charArray = NPUArray<char>.getWithoutTracking()
            };
        }
        public static NStringBuilder get(int capacity)
        {
            return new NStringBuilder()
            {
                _charArray = NPUArray<char>.get(capacity)
            };
        }
        public static NStringBuilder getWithoutTracking(int capacity)
        {
            return new NStringBuilder()
            {
                _charArray = NPUArray<char>.getWithoutTracking(capacity)
            };
        }

        public char this[int index]
        {
            get { return _charArray[index]; }
        }

        private NPUArray<char> _charArray;
        public int Count => _charArray.Count;

        public bool IsCreated => _charArray.IsCreated;
        public bool IsReadOnly => _charArray.IsReadOnly;

        public void stopTracking()
        {
            _charArray.stopTracking();
        }

        public NStringBuilder Append(char item)
        {
            _charArray.Add(item);
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
        public unsafe NStringBuilder Insert(int index, string value)
        {
            fixed (char* p = value)
            {
                _charArray.InsertRangeWithoutChecks(index, p, value.Length);
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
        public bool Contains(char item)
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
        public unsafe override string ToString()
        {
            fixed (byte* bPtr = _charArray.i_array.Collection.i_Items)
            {
                return new string((char*)bPtr, 0, Count);
            }
        }
        public unsafe string ToString(int startIndex, int length)
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
