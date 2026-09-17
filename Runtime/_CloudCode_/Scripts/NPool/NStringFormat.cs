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

        private void __performReplace(ReadOnlySpan<char> searchSpan, ReadOnlySpan<char> valSpan)
        {
            if (!_sb.IsCreated) return;

            int searchIndex = 0;
            int foundIdx;
            while ((foundIdx = _sb.AsSpan()[searchIndex..].IndexOf(searchSpan)) >= 0)
            {
                int matchIdx = searchIndex + foundIdx;
                _sb.RemoveRange(matchIdx, searchSpan.Length);
                _sb.Insert(matchIdx, valSpan);
                searchIndex = matchIdx + valSpan.Length;
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
                __performReplace(tagBuilder.AsSpan(), (value ? NStringConst.True : NStringConst.False).AsSpan());
            }
            return this;
        }

        public NStringFormat FormatNext(int value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using var valBuilder = NStringBuilder.get(16);
                valBuilder.Append(value);
                __performReplace(tagBuilder.AsSpan(), valBuilder.AsSpan());
            }
            return this;
        }

        public NStringFormat FormatNext(long value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using var valBuilder = NStringBuilder.get(24);
                valBuilder.Append(value);
                __performReplace(tagBuilder.AsSpan(), valBuilder.AsSpan());
            }
            return this;
        }

        public NStringFormat FormatNext(ulong value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using var valBuilder = NStringBuilder.get(24);
                valBuilder.Append(value);
                __performReplace(tagBuilder.AsSpan(), valBuilder.AsSpan());
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
                __performReplace(tagBuilder.AsSpan(), valSpan);
            }
            return this;
        }

        public NStringFormat FormatNext(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                __performReplace(tagBuilder.AsSpan(), value.AsSpan());
            }
            return this;
        }

        public NStringFormat FormatNext(float value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using var valBuilder = NStringBuilder.get(16);
                valBuilder.Append(value);
                __performReplace(tagBuilder.AsSpan(), valBuilder.AsSpan());
            }
            return this;
        }

        public NStringFormat FormatNext(float value, int maxDigits)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using var valBuilder = NStringBuilder.get(16);
                valBuilder.Append(value, maxDigits);
                __performReplace(tagBuilder.AsSpan(), valBuilder.AsSpan());
            }
            return this;
        }

        public NStringFormat FormatNext(ReadOnlySpan<char> value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                __performReplace(tagBuilder.AsSpan(), value);
            }
            return this;
        }

        public NStringFormat FormatNext(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using var valBuilder = NStringBuilder.get();
                valBuilder.Append(value);
                __performReplace(tagBuilder.AsSpan(), valBuilder.AsSpan());
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
                __performReplace(tagBuilder.AsSpan(), value.AsSpan());
            }
            if (consume) value.Dispose();
            return this;
        }
        public NStringFormat FormatNext<T>(T value)
        {
            using (var tagBuilder = NStringBuilder.get(16))
            {
                tagBuilder.Append('{').Append(_currentIndex).Append('}');
                using var valBuilder = NStringBuilder.get();
                valBuilder.Append(value);
                __performReplace(tagBuilder.AsSpan(), valBuilder.AsSpan());
            }
            return this;
        }

        public override readonly string ToString()
        {
            return _sb.ToString();
        }

        public readonly Span<char> AsSpan() => _sb.AsSpan()[.._sb.Count];

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
