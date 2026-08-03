using System;

namespace Nextension
{
    public struct NStringFormat : IDisposable
    {
        private NStringBuilder _sb;
        private int _currentIndex;

        public readonly bool IsCreated => _sb.IsCreated;

        public static NStringFormat get(string content)
        {
            return new NStringFormat(content);
        }

        public NStringFormat(string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            _sb = NStringBuilder.get(content.Length + 64);
            if (!string.IsNullOrEmpty(content))
            {
                _sb.Append(content);
            }
            _currentIndex = 0;
        }

        private void PerformReplace(ReadOnlySpan<char> searchSpan, ReadOnlySpan<char> valSpan)
        {
            if (!_sb.IsCreated) return;

            // Manual reverse search to avoid Span.LastIndexOf issues and handle shifting correctly
            for (int i = _sb.Count - searchSpan.Length; i >= 0; i--)
            {
                var currentSpan = _sb.AsSpan();
                bool match = true;
                for (int j = 0; j < searchSpan.Length; j++)
                {
                    if (currentSpan[i + j] != searchSpan[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    _sb.RemoveRange(i, searchSpan.Length);
                    _sb.Insert(i, valSpan);
                }
            }
            _currentIndex++;
        }

        public NStringFormat SkipFormat(int count = 1)
        {
            _currentIndex += count;
            return this;
        }

        public NStringFormat FormatNext(bool value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get(8))
                {
                    valBuilder.Append(value);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public NStringFormat FormatNext(int value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get(16))
                {
                    valBuilder.Append(value);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public NStringFormat FormatNext(long value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get(24))
                {
                    valBuilder.Append(value);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public NStringFormat FormatNext(ulong value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get(24))
                {
                    valBuilder.Append(value);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public NStringFormat FormatNext(char value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                Span<char> valSpan = stackalloc char[1];
                valSpan[0] = value;
                PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valSpan);
            }
            return this;
        }

        public NStringFormat FormatNext(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), value.AsSpan());
            }
            return this;
        }

        public NStringFormat FormatNext(float value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get(16))
                {
                    valBuilder.Append(value);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public NStringFormat FormatNext(float value, int maxDigits)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get(16))
                {
                    valBuilder.Append(value, maxDigits);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public NStringFormat FormatNext(ReadOnlySpan<char> value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), value);
            }
            return this;
        }

        public NStringFormat FormatNext(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get())
                {
                    valBuilder.Append(value);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public NStringFormat FormatNext(NStringBuilder value)
        {
            return FormatNext(value, true);
        }
        public NStringFormat FormatNext(NStringBuilder value, bool consume = true)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), value.AsSpan().Slice(0, value.Count));
            }
            if (consume) value.Dispose();
            return this;
        }
        public NStringFormat FormatNext<T>(T value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using (var valBuilder = NStringBuilder.get())
                {
                    valBuilder.Append(value);
                    PerformReplace(tagBuilder.AsSpan().Slice(0, tagBuilder.Count), valBuilder.AsSpan().Slice(0, valBuilder.Count));
                }
            }
            return this;
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public Span<char> AsSpan() => _sb.AsSpan().Slice(0, _sb.Count);

        public string consume()
        {
            var str = ToString();
            Dispose();
            return str;
        }

        public void Dispose()
        {
            _sb.Dispose();
        }
    }

    public static class NStringFormatExtension
    {
        public static NStringFormat FormatNext(this string content, int value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, long value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, ulong value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, char value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, string value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, float value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, float value, int maxDigits)
        {
            return NStringFormat.get(content).FormatNext(value, maxDigits);
        }

        public static NStringFormat FormatNext(this string content, ReadOnlySpan<char> value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, object value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }

        public static NStringFormat FormatNext(this string content, NStringBuilder value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }
        public static NStringFormat FormatNext<T>(this string content, T value)
        {
            return NStringFormat.get(content).FormatNext(value);
        }
    }
}
