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

        private readonly NPUArray<char> _charArray;
        public readonly int Count => _charArray.Count;
        public readonly bool IsCreated => _charArray.IsCreated;
        public readonly bool IsReadOnly => _charArray.IsReadOnly;

        public void stopTracking()
        {
            _charArray.stopTracking();
        }

        public readonly NStringBuilder this[int value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[long value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[ulong value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[char value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[string value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[float value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[object value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[NStringBuilder value]
        {
            get
            {
                Append(value);
                return this;
            }
        }

        public readonly NStringBuilder Append(char item)
        {
            _charArray.Add(item);
            return this;
        }
        public readonly NStringBuilder Append(int value)
        {
            return Append((long)value);
        }
        public readonly NStringBuilder Append(uint value)
        {
            return Append((ulong)value);
        }
        public readonly NStringBuilder Append(long value)
        {
            if (value == 0)
            {
                _charArray.Add('0');
                return this;
            }

            if (value < 0)
            {
                _charArray.Add('-');
                if (value == long.MinValue)
                {
                    return Append("9223372036854775808");
                }
                value = -value;
            }

            unsafe
            {
                char* buffer = stackalloc char[20];
                int bufferIndex = 20;
                while (value > 0)
                {
                    bufferIndex--;
                    buffer[bufferIndex] = (char)('0' + (value % 10));
                    value /= 10;
                }
                for (int i = bufferIndex; i < 20; i++)
                {
                    _charArray.Add(buffer[i]);
                }
            }

            return this;
        }
        public readonly NStringBuilder Append(ulong value)
        {
            if (value == 0)
            {
                _charArray.Add('0');
                return this;
            }

            unsafe
            {
                char* buffer = stackalloc char[20];
                int bufferIndex = 20;
                while (value > 0)
                {
                    bufferIndex--;
                    buffer[bufferIndex] = (char)('0' + (value % 10));
                    value /= 10;
                }
                for (int i = bufferIndex; i < 20; i++)
                {
                    _charArray.Add(buffer[i]);
                }
            }

            return this;
        }
        public readonly NStringBuilder Append(float value)
        {
            return Append(value, 0, 7);
        }
        public readonly NStringBuilder Append(float value, int digits)
        {
            return Append(value, 0, digits);
        }
        public readonly NStringBuilder Append(float value, int minDigits, int maxDigits)
        {
            if (float.IsNaN(value)) return Append("NaN");
            if (float.IsPositiveInfinity(value)) return Append("Infinity");
            if (float.IsNegativeInfinity(value)) return Append("-Infinity");

            if (value < 0)
            {
                _charArray.Add('-');
                value = -value;
            }

            // Clamp digit counts
            if (minDigits < 0) minDigits = 0;
            if (maxDigits < minDigits) maxDigits = minDigits;
            if (maxDigits > 7) maxDigits = 7;   // float precision limit

            long intPart = (long)value;
            Append(intPart);

            if (maxDigits == 0)
            {
                return this;
            }

            // Build fractional digits into a small buffer
            float frac = value - intPart;

            unsafe
            {
                char* buf = stackalloc char[8];
                int lastNonZero = -1;
                for (int i = 0; i < maxDigits; i++)
                {
                    frac *= 10f;
                    int digit = (int)frac;
                    buf[i] = (char)('0' + digit);
                    frac -= digit;
                    if (digit != 0) lastNonZero = i;
                }

                // Trim trailing zeros down to minDigits
                int writeCount = lastNonZero + 1;
                if (writeCount < minDigits) writeCount = minDigits;

                if (writeCount > 0)
                {
                    _charArray.Add('.');
                    for (int i = 0; i < writeCount; i++)
                    {
                        _charArray.Add(buf[i]);
                    }
                }
            }

            return this;
        }

        public readonly NStringBuilder Append(bool value)
        {
            if (value)
            {
                _charArray.Add('T');
                _charArray.Add('r');
                _charArray.Add('u');
                _charArray.Add('e');
            }
            else
            {
                _charArray.Add('F');
                _charArray.Add('a');
                _charArray.Add('l');
                _charArray.Add('s');
                _charArray.Add('e');
            }
            return this;
        }

        public readonly NStringBuilder Append(double value)
        {
            return Append((float)value);
        }
        public readonly NStringBuilder Append(string value)
        {
            return Insert(Count, value);
        }
        public readonly NStringBuilder Append(object value)
        {
            if (value == null) throw new ArgumentNullException();
            return Insert(Count, value.ToString());
        }
        public readonly NStringBuilder Append(ReadOnlySpan<char> value)
        {
            return Insert(Count, value);
        }
        public readonly NStringBuilder Append(NStringBuilder value)
        {
            return Insert(Count, value);
        }
        public readonly NStringBuilder Append(NStringFormat value)
        {
            return Insert(Count, value.AsSpan());;
        }

        public readonly NStringBuilder AppendLine()
        {
            _charArray.Add('\n');
            return this;
        }
        public readonly NStringBuilder AppendLine(char value)
        {
            Append(value);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(long value)
        {
            Append(value);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(ulong value)
        {
            Append(value);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(float value)
        {
            Append(value);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(float value, int digits)
        {
            Append(value, digits);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(float value, int minDigits, int maxDigits)
        {
            Append(value, minDigits, maxDigits);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(string value)
        {
            Insert(Count, value);
            AppendLine();
            return this;
        }
        public readonly NStringBuilder AppendLine(ReadOnlySpan<char> value)
        {
            Insert(Count, value);
            AppendLine();
            return this;
        }
        public readonly NStringBuilder AppendLine(NStringBuilder value)
        {
            return Append(value).AppendLine();
        }
        public readonly NStringBuilder AppendLine(NStringFormat value)
        {
            return Append(value).AppendLine();
        }

        public readonly NStringBuilder Append<T>(T value)
        {
            if (value is int i) return Append((long)i);
            if (value is float f) return Append(f);
            if (value is string s) return Append(s);
            if (value is char c) return Append(c);
            if (value is long l) return Append(l);
            if (value is ulong ul) return Append(ul);
            if (value is uint ui) return Append((ulong)ui);
            if (value is bool b) return Append(b);
            if (value is double d) return Append(d);
            if (value is short sh) return Append((long)sh);
            if (value is ushort ush) return Append((ulong)ush);
            if (value is byte by) return Append((ulong)by);
            if (value is sbyte sb) return Append((long)sb);
            if (value is NStringBuilder nsb) return Append(nsb);
            if (value is NStringFormat nsf) return Append(nsf);

            return Append(value?.ToString() ?? "");
        }

        public readonly NStringBuilder Insert(int index, char item)
        {
            _charArray.Insert(index, item);
            return this;
        }
        public readonly unsafe NStringBuilder Insert(int index, string value)
        {
            fixed (char* p = value)
            {
                _charArray.InsertRange(index, p, value.Length);
                return this;
            }
        }
        public readonly unsafe NStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            fixed (char* p = value)
            {
                _charArray.InsertRange(index, p, value.Length);
                return this;
            }
        }
        public readonly unsafe NStringBuilder Insert(int index, NStringBuilder value)
        {

            byte* bPtr = value._charArray.i_array.Collection.GetUnsafePtr();
            {
                _charArray.InsertRange(index, (char*)bPtr, value.Count);
                return this;
            }
        }

        public readonly NStringBuilder SetValue(int index, char value)
        {
            _charArray[index] = value;
            return this;
        }

        public readonly bool Remove(char item)
        {
            return _charArray.Remove(item);
        }
        public readonly NStringBuilder RemoveRange(int index, int count)
        {
            _charArray.RemoveRange(index, count);
            return this;
        }

        public readonly void Clear()
        {
            _charArray.Clear();
        }

        public readonly bool Contains(char item)
        {
            return _charArray.Contains(item);
        }

        public readonly void Dispose()
        {
            _charArray.Dispose();
        }

        public readonly string consume(bool removeLastLineBreak = false)
        {
#if UNITY_EDITOR
            if (_charArray.IsDisposed)
            {
                throw new InvalidOperationException("NStringBuilder has been disposed");
            }
#endif
            var str = getString(removeLastLineBreak);
            Dispose();
            return str;
        }

        public readonly void endGetter() { }

        public readonly Span<char> AsSpan() => _charArray.AsSpan();

        public readonly string getString(bool removeLastLineBreak = false)
        {
            if (removeLastLineBreak)
            {
                if (Count > 0 && _charArray[Count - 1] == '\n')
                {
                    return ToString(0, Count - 1);
                }
            }
            return ToString();
        }

        public readonly unsafe override string ToString()
        {
            byte* bPtr = _charArray.i_array.Collection.GetUnsafePtr();
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
            byte* bPtr = _charArray.i_array.Collection.GetUnsafePtr();
            {
                return new string((char*)bPtr, startIndex, length);
            }
        }
        public static implicit operator string(NStringBuilder value)
        {
            return value.consume();
        }
    }
}
