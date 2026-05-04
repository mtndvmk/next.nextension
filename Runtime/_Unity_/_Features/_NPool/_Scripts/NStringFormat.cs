using System;

namespace Nextension
{
    public struct NStringFormat : IDisposable
    {
        private NStringBuilder _sb;
        private string _content;
        private int _currentIndex;

        public readonly bool IsCreated => _sb.IsCreated;

        public static NStringFormat get(string content)
        {
            return new NStringFormat(content);
        }

        public NStringFormat(string content)
        {
            _content = content;
            _sb = NStringBuilder.get(content != null ? content.Length + 64 : 64);
            _currentIndex = 0;
        }

        private bool PrepareNextFormat()
        {
            if (string.IsNullOrEmpty(_content)) return false;

            int nextBrace = _content.IndexOf('{', _currentIndex);
            if (nextBrace >= 0)
            {
                int endBrace = _content.IndexOf('}', nextBrace);
                if (endBrace >= 0)
                {
                    if (nextBrace > _currentIndex)
                    {
                        _sb.Append(_content.AsSpan(_currentIndex, nextBrace - _currentIndex));
                    }
                    _currentIndex = endBrace + 1;
                    return true;
                }
            }
            return false;
        }

        public NStringFormat FormatNext(bool value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public NStringFormat FormatNext(int value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public NStringFormat FormatNext(long value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;

        }

        public NStringFormat FormatNext(ulong value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public NStringFormat FormatNext(char value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public NStringFormat FormatNext(string value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public NStringFormat FormatNext(float value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;

        }

        public NStringFormat FormatNext(float value, int maxDigits)
        {
            if (PrepareNextFormat()) _sb.Append(value, maxDigits);
            return this;

        }

        public NStringFormat FormatNext(ReadOnlySpan<char> value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public NStringFormat FormatNext(object value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public NStringFormat FormatNext(NStringBuilder value)
        {
            return FormatNext(value, true);
        }
        public NStringFormat FormatNext(NStringBuilder value, bool consume = true)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            if (consume) value.Dispose();
            return this;
        }
        public NStringFormat FormatNext<T>(T value)
        {
            if (PrepareNextFormat()) _sb.Append(value);
            return this;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_content) && _currentIndex < _content.Length)
            {
                int prevCount = _sb.Count;
                _sb.Append(_content.AsSpan(_currentIndex));
                string result = _sb.ToString();
                _sb.RemoveRange(prevCount, _sb.Count - prevCount);
                return result;
            }
            return _sb.ToString();
        }

        public Span<char> AsSpan() => _sb.AsSpan();

        public string consume()
        {
            var str = ToString();
            Dispose();
            return str;
        }

        public void Dispose()
        {
            _sb.Dispose();
            _content = null;
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
