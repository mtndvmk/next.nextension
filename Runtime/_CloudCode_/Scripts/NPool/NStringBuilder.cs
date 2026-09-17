using System;

namespace Nextension
{
    public readonly struct NStringBuilder : IDisposable
    {
        public static NStringBuilder get()
        {
            return new NStringBuilder(PUList<char>.get());
        }

        public static NStringBuilder get(int capacity)
        {
            return new NStringBuilder(PUList<char>.get(capacity));
        }

        private NStringBuilder(PUList<char> arr)
        {
            _charList = arr;
        }

        private static readonly double[] _pow10 = new double[]
        {
            1.0, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, 10000000.0,
            100000000.0, 1000000000.0, 10000000000.0, 100000000000.0, 1000000000000.0,
            10000000000000.0, 100000000000000.0, 1000000000000000.0
        };

        private readonly PUList<char> _charList;
        public readonly int Count => _charList.Count;
        public readonly int Length => _charList.Count;
        public readonly bool IsCreated => _charList.IsCreated;
        public readonly bool IsDisposed => _charList.IsDisposed;

        public readonly NStringBuilder this[int value]
        {
            get
            {
                Append(value);
                return this;
            }
        }
        public readonly NStringBuilder this[uint value]
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

        public readonly NStringBuilder this[NStringBuilder value]
        {
            get
            {
                Append(value);
                return this;
            }
        }

        public readonly NStringBuilder this[NStringFormat value]
        {
            get
            {
                Append(value);
                return this;
            }
        }

        public readonly NStringBuilder Append(char item)
        {
            _charList.Add(item);
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
                _charList.Add('0');
                return this;
            }

            if (value == long.MinValue)
            {
                return Append(NStringConst.MinLong);
            }

            if (value < 0)
            {
                _charList.Add('-');
                value = -value;
            }

            __appendUlongUncheck((ulong)value);

            return this;
        }

        public readonly NStringBuilder Append(ulong value)
        {
            if (value == 0)
            {
                _charList.Add('0');
                return this;
            }

            __appendUlongUncheck(value);

            return this;
        }

        private readonly void __appendUlongUncheck(ulong value)
        {
            Span<char> buffer = stackalloc char[20];
            int bufferIndex = 20;
            while (value > 0)
            {
                bufferIndex--;
                buffer[bufferIndex] = (char)('0' + (value % 10));
                value /= 10;
            }
            _charList.AddRange(buffer[bufferIndex..]);
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
            if (float.IsNaN(value)) return Append(NStringConst.NaN);
            if (float.IsPositiveInfinity(value)) return Append(NStringConst.Infinity);
            if (float.IsNegativeInfinity(value)) return Append(NStringConst.NegativeInfinity);

            if (value < 0 || (value == 0.0f && 1.0f / value < 0.0f))
            {
                _charList.Add('-');
                value = -value;
            }

            if (value >= 9e18f)
            {
                return Append(value.ToString());
            }

            if (maxDigits > 7) maxDigits = 7;

            if (maxDigits == 0)
            {
                long intOnly = (long)Math.Round(value);
                return Append(intOnly);
            }

            double pow = _pow10[maxDigits];
            double scaledVal = Math.Round((double)value * pow);
            long intPart = (long)(scaledVal / pow);
            ulong fracPart = (ulong)Math.Abs(scaledVal - (intPart * pow));

            Append(intPart);

            Span<char> buf = stackalloc char[maxDigits];
            for (int i = maxDigits - 1; i >= 0; i--)
            {
                buf[i] = (char)('0' + (fracPart % 10));
                fracPart /= 10;
            }

            int lastNonZero = maxDigits - 1;
            while (lastNonZero >= minDigits && buf[lastNonZero] == '0')
            {
                lastNonZero--;
            }

            int writeCount = lastNonZero + 1;
            if (writeCount > 0)
            {
                _charList.Add('.');
                _charList.AddRange(buf[..writeCount]);
            }

            return this;
        }

        public readonly NStringBuilder Append(bool value)
        {
            _charList.AddRange((value ? NStringConst.True : NStringConst.False).AsSpan());
            return this;
        }

        public readonly NStringBuilder Append(double value)
        {
            return Append(value, 0, 15);
        }
        public readonly NStringBuilder Append(double value, int digits)
        {
            return Append(value, 0, digits);
        }
        public readonly NStringBuilder Append(double value, int minDigits, int maxDigits)
        {
            if (double.IsNaN(value)) return Append(NStringConst.NaN);
            if (double.IsPositiveInfinity(value)) return Append(NStringConst.Infinity);
            if (double.IsNegativeInfinity(value)) return Append(NStringConst.NegativeInfinity);

            if (value < 0 || (value == 0.0 && 1.0 / value < 0.0))
            {
                _charList.Add('-');
                value = -value;
            }

            if (value >= 9e18)
            {
                return Append(value.ToString());
            }

            if (maxDigits > 15) maxDigits = 15;

            if (maxDigits == 0)
            {
                long intOnly = (long)Math.Round(value);
                return Append(intOnly);
            }

            double pow = _pow10[maxDigits];
            double scaledVal = Math.Round(value * pow);
            long intPart = (long)(scaledVal / pow);
            ulong fracPart = (ulong)Math.Abs(scaledVal - (intPart * pow));

            Append(intPart);

            Span<char> buf = stackalloc char[maxDigits];
            for (int i = maxDigits - 1; i >= 0; i--)
            {
                buf[i] = (char)('0' + (fracPart % 10));
                fracPart /= 10;
            }

            int lastNonZero = maxDigits - 1;
            while (lastNonZero >= minDigits && buf[lastNonZero] == '0')
            {
                lastNonZero--;
            }

            int writeCount = lastNonZero + 1;
            if (writeCount > 0)
            {
                _charList.Add('.');
                _charList.AddRange(buf[..writeCount]);
            }

            return this;
        }

        public readonly NStringBuilder Append(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _charList.AddRange(value.AsSpan());
            return this;
        }
        public readonly NStringBuilder Append(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Append(value.ToString());
        }
        public readonly NStringBuilder Append(ReadOnlySpan<char> value)
        {
            _charList.AddRange(value);
            return this;
        }
        public readonly NStringBuilder Append(NStringBuilder value)
        {
            return Append(value, true);
        }
        public readonly NStringBuilder Append(NStringBuilder value, bool consume)
        {
            if (_charList.Equals(value._charList))
            {
                int origCount = Count;
                _charList.EnsureCapacity(origCount * 2);
                _charList.AddRange(_charList.AsSpan()[..origCount]);
                return this;
            }
            _charList.AddRange(value.AsSpan());
            if (consume) value.Dispose();
            return this;
        }

        public readonly NStringBuilder Append(NStringFormat value)
        {
            return Append(value, true);
        }
        public readonly NStringBuilder Append(NStringFormat value, bool consume)
        {
            _charList.AddRange(value.AsSpan());
            if (consume) value.Dispose();
            return this;
        }

        public readonly NStringBuilder AppendLine()
        {
            _charList.Add('\n');
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
        public readonly NStringBuilder AppendLine(double value)
        {
            Append(value);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(double value, int digits)
        {
            Append(value, digits);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(double value, int minDigits, int maxDigits)
        {
            Append(value, minDigits, maxDigits);
            return AppendLine();
        }
        public readonly NStringBuilder AppendLine(string value)
        {
            Append(value);
            AppendLine();
            return this;
        }
        public readonly NStringBuilder AppendLine(ReadOnlySpan<char> value)
        {
            Append(value);
            AppendLine();
            return this;
        }
        public readonly NStringBuilder AppendLine(NStringBuilder value, bool consume = true)
        {
            var sb = Append(value, consume);
            sb.AppendLine();
            return sb;
        }
        public readonly NStringBuilder AppendLine(NStringFormat value, bool consume = true)
        {
            var sb = Append(value, consume);
            sb.AppendLine();
            return sb;
        }

        public readonly NStringBuilder Append<T>(T value)
        {
            if (typeof(T) == typeof(int)) return Append((long)UnsafeUtil.reinterpret<T, int>(value));
            if (typeof(T) == typeof(float)) return Append(UnsafeUtil.reinterpret<T, float>(value));
            if (typeof(T) == typeof(string)) return Append(UnsafeUtil.reinterpret<T, string>(value));
            if (typeof(T) == typeof(char)) return Append(UnsafeUtil.reinterpret<T, char>(value));
            if (typeof(T) == typeof(long)) return Append(UnsafeUtil.reinterpret<T, long>(value));
            if (typeof(T) == typeof(ulong)) return Append(UnsafeUtil.reinterpret<T, ulong>(value));
            if (typeof(T) == typeof(uint)) return Append((ulong)UnsafeUtil.reinterpret<T, uint>(value));
            if (typeof(T) == typeof(bool)) return Append(UnsafeUtil.reinterpret<T, bool>(value));
            if (typeof(T) == typeof(double)) return Append(UnsafeUtil.reinterpret<T, double>(value));
            if (typeof(T) == typeof(short)) return Append((long)UnsafeUtil.reinterpret<T, short>(value));
            if (typeof(T) == typeof(ushort)) return Append((ulong)UnsafeUtil.reinterpret<T, ushort>(value));
            if (typeof(T) == typeof(byte)) return Append((ulong)UnsafeUtil.reinterpret<T, byte>(value));
            if (typeof(T) == typeof(sbyte)) return Append((long)UnsafeUtil.reinterpret<T, sbyte>(value));
            if (typeof(T) == typeof(NStringBuilder)) return Append(UnsafeUtil.reinterpret<T, NStringBuilder>(value));
            if (typeof(T) == typeof(NStringFormat)) return Append(UnsafeUtil.reinterpret<T, NStringFormat>(value));

            if (value is null) throw new ArgumentNullException(nameof(value));
            return Append(value.ToString());
        }

        public readonly NStringBuilder Insert(int index, char item)
        {
            if ((uint)index > (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            _charList.Insert(index, item);
            return this;
        }
        public readonly NStringBuilder Insert(int index, string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if ((uint)index > (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (value.Length == 0) return this;
            _charList.InsertRange(index, value.AsSpan());
            return this;
        }
        public readonly NStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            if ((uint)index > (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            _charList.InsertRange(index, value);
            return this;
        }
        public readonly NStringBuilder Insert(int index, NStringBuilder value)
        {
            return Insert(index, value, true);
        }

        public readonly NStringBuilder Insert(int index, NStringBuilder value, bool consume)
        {
            if ((uint)index > (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (_charList.Equals(value._charList))
            {
                int valLen = value.Count;
                if (valLen == 0) return this;
                if (valLen <= 128)
                {
                    Span<char> temp = stackalloc char[valLen <= 128 ? valLen : 0];
                    AsSpan().CopyTo(temp);
                    _charList.InsertRange(index, temp);
                }
                else
                {
                    using var poolCopy = PUList<char>.get(valLen);
                    poolCopy.AddRange(AsSpan());
                    _charList.InsertRange(index, poolCopy.AsSpan());
                }
                return this;
            }
            Insert(index, value.AsSpan());
            if (consume) value.Dispose();
            return this;
        }

        public readonly NStringBuilder Insert(int index, NStringFormat value)
        {
            return Insert(index, value, true);
        }

        public readonly NStringBuilder Insert(int index, NStringFormat value, bool consume)
        {
            if ((uint)index > (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            Insert(index, value.AsSpan());
            if (consume) value.Dispose();
            return this;
        }

        public readonly NStringBuilder SetValue(int index, char value)
        {
            _charList[index] = value;
            return this;
        }

        public readonly bool Remove(char item)
        {
            return _charList.Remove(item);
        }
        public readonly NStringBuilder RemoveRange(int index, int count)
        {
            _charList.RemoveRange(index, count);
            return this;
        }

        public readonly void Clear()
        {
            _charList.Clear();
        }

        public readonly bool Contains(char item)
        {
            return _charList.Contains(item);
        }

        public readonly void Dispose()
        {
            _charList.Dispose();
        }

        public readonly string consume(bool removeLastLineBreak = false)
        {
            if (_charList.IsDisposed)
            {
                throw new InvalidOperationException("NStringBuilder has been disposed");
            }
            var str = ToString(removeLastLineBreak);
            Dispose();
            return str;
        }

        public readonly void endGetter() { }

        public readonly Span<char> AsSpan() => _charList.AsSpan();

        public readonly Span<char> EnsureCapacityAsSpan(int capacity) => _charList.EnsureCapacityAsSpan(capacity);

        public readonly void SetCount(int count) => _charList.SetCount(count);

        public readonly int IndexOf(char value, int startIndex = 0)
        {
            return _charList.IndexOf(value, startIndex);
        }

        public readonly int IndexOf(string value, int startIndex = 0)
        {
            if (string.IsNullOrEmpty(value)) return -1;
            if (startIndex < 0 || startIndex > Count - value.Length) return -1;

            int relIndex = AsSpan()[startIndex..].IndexOf(value.AsSpan());
            return relIndex >= 0 ? startIndex + relIndex : -1;
        }

        public readonly bool StartsWith(string value, int startIndex = 0)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (startIndex < 0 || startIndex > Count - value.Length)
                return false;

            return AsSpan()[startIndex..].StartsWith(value.AsSpan());
        }

        private readonly void __checkDisposed()
        {
            if (IsDisposed) throw new Exception("NStringBuilder has been disposed");
        }

        public readonly override string ToString()
        {
            __checkDisposed();

            int count = Count;
            if (count == 0) return string.Empty;

            return _charList.AsSpan().ToString();
        }
        public readonly string ToString(int startIndex, int length)
        {
            __checkDisposed();

            if (startIndex < 0 || length < 0 || startIndex > Count - length)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (length == 0) return string.Empty;

            return _charList.AsSpan().Slice(startIndex, length).ToString();
        }

        public readonly string ToString(bool removeLastLineBreak = false)
        {
            __checkDisposed();

            int count = Count;

            if (removeLastLineBreak)
            {
                if (count > 0 && _charList[^1] == '\n')
                {
                    return AsSpan()[..^1].ToString();
                }
            }

            if (count == 0) return string.Empty;

            return _charList.AsSpan().ToString();
        }

        public static implicit operator string(NStringBuilder value)
        {
            return value.consume();
        }
    }
}
