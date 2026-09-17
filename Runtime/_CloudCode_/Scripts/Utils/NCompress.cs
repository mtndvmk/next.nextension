using System;
using System.IO;
using System.IO.Compression;
using ZstdSharp;

namespace Nextension
{
    public enum CompressType : byte
    {
        Deflate = 0,
        Zstd = 1,
    }
    public static class NCompress
    {
        public const char SEPARATOR = '~';
        public const char LEGACY_SEPARATOR = ':';

        public static bool isSeparator(char c) => c == SEPARATOR || c == LEGACY_SEPARATOR;

        public static void appendBase64(NStringBuilder sb, ReadOnlySpan<byte> buffer)
        {
            int maxBase64Length = (buffer.Length + 2) / 3 * 4;
            var spanChars = sb.EnsureCapacityAsSpan(sb.Length + maxBase64Length)[sb.Length..];
            Convert.TryToBase64Chars(buffer, spanChars, out var charsWritten);
            sb.SetCount(sb.Length + charsWritten);
        }
        public static CompressType readCompressType(ReadOnlySpan<char> str, out ReadOnlySpan<char> remaining)
        {
            if (str.Length < 3) throw new FormatException("Invalid compressed string format");
            var separator = str[0];
            if (!isSeparator(separator)) throw new FormatException("Invalid compressed string format");
            ReadOnlySpan<char> remainingStr = str[1..];
            int secondSeparator = remainingStr.IndexOf(separator);
            if (secondSeparator == -1) throw new FormatException("Invalid compressed string format");

            ReadOnlySpan<char> versionSpan = remainingStr[..secondSeparator];
            if (!int.TryParse(versionSpan, out var type))
            {
                throw new FormatException("Invalid compressed string format: unsupported compression type");
            }

            remaining = remainingStr[(secondSeparator + 1)..];
            return (CompressType)type;
        }
        public static PUList<byte> readBase64(ReadOnlySpan<char> base64Span)
        {
            if (isSeparator(base64Span[0]))
            {
                readCompressType(base64Span, out base64Span);
            }
            int maxBase64Length = base64Span.Length * 3 / 4;
            var bytePool = PUList<byte>.get();
            Span<byte> resultSpan = bytePool.EnsureCapacityAsSpan(maxBase64Length);

            if (!Convert.TryFromBase64Chars(base64Span, resultSpan, out int bytesWritten))
            {
                throw new FormatException("Invalid base64 string");
            }

            bytePool.SetCount(bytesWritten);
            return bytePool;
        }
        public static NStringBuilder compressToStr(ReadOnlySpan<byte> buffer, CompressType type)
        {
            return type switch
            {
                CompressType.Deflate => Deflate.compressToStr(buffer),
                CompressType.Zstd => Zstd.compressToStr(buffer),
                _ => throw new NotSupportedException($"Unsupported compression type: {type}"),
            };
        }
        public static PUList<byte> decompress(ReadOnlySpan<char> base64Span)
        {
            var type = readCompressType(base64Span, out var remaining);
            return type switch
            {
                CompressType.Deflate => Deflate.decompress(remaining),
                CompressType.Zstd => Zstd.decompress(remaining),
                _ => throw new NotSupportedException($"Unsupported compression type: {type}"),
            };
        }
        public static ReadOnlyMemory<byte> decompressToMemory(ReadOnlySpan<char> base64Span)
        {
            var type = readCompressType(base64Span, out var remaining);
            return type switch
            {
                CompressType.Deflate => Deflate.decompressToMemory(remaining),
                CompressType.Zstd => Zstd.decompressToMemory(remaining),
                _ => throw new NotSupportedException($"Unsupported compression type: {type}"),
            };
        }

        public static class Deflate
        {
            public static NStringBuilder compressToStr(ReadOnlySpan<byte> buffer)
            {
                using var outputStream = PooledMemoryStream.get();

                var deflate = new DeflateStream(outputStream, CompressionMode.Compress, true);
                deflate.Write(buffer);
                deflate.Dispose();

                var sb = NStringBuilder.get()[SEPARATOR][(byte)CompressType.Deflate][SEPARATOR];
                NCompress.appendBase64(sb, outputStream.AsSpan());
                return sb;
            }

            public static PUList<byte> decompress(ReadOnlySpan<char> base64Span)
            {
                using var compressedBytes = NCompress.readBase64(base64Span);
                var compressedSpan = compressedBytes.AsSpan();

                using var input = compressedBytes.GetStream();
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                using var outputStream = PooledMemoryStream.get();

                deflate.CopyTo(outputStream);
                var result = PUList<byte>.get();
                result.AddRange(outputStream.AsSpan());
                return result;
            }

            public static ReadOnlyMemory<byte> decompressToMemory(ReadOnlySpan<char> base64Span)
            {
                using var compressedBytes = NCompress.readBase64(base64Span);
                var compressedSpan = compressedBytes.AsSpan();

                using var input = compressedBytes.GetStream();
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                using var outputStream = new MemoryStream();

                deflate.CopyTo(outputStream);
                return outputStream.GetBuffer().AsMemory(0, (int)outputStream.Length);
            }
        }

        public static class Zstd
        {
            public const int DefaultLevel = 3;
            public const int HighLevel = 19;

            public static NStringBuilder compressToStr(ReadOnlySpan<byte> buffer, int level = DefaultLevel)
            {
                var bound = Compressor.GetCompressBound(buffer.Length);
                using var dst = PUList<byte>.get();
                var dstSpan = dst.EnsureCapacityAsSpan(bound);
                using var compressor = PooledCompressor.get(level);
                var byteWritten = compressor.Value.Wrap(buffer, dstSpan);
                var sb = NStringBuilder.get()[SEPARATOR][(byte)CompressType.Zstd][SEPARATOR];
                NCompress.appendBase64(sb, dstSpan[..byteWritten]);
                return sb;
            }

            public static PUList<byte> decompress(ReadOnlySpan<char> base64Span)
            {
                using var compressedBytes = NCompress.readBase64(base64Span);
                var compressedSpan = compressedBytes.AsSpan();

                var decompressSize = Decompressor.GetDecompressedSize(compressedSpan);
                var result = PUList<byte>.get();
                var resultSpan = result.EnsureCapacityAsSpan((int)decompressSize);
                using var decompressor = PooledDecompressor.get();
                var bytesWritten = decompressor.Value.Unwrap(compressedSpan, resultSpan);
                result.SetCount(bytesWritten);
                return result;
            }

            public static ReadOnlyMemory<byte> decompressToMemory(ReadOnlySpan<char> base64Span)
            {
                using var compressedBytes = NCompress.readBase64(base64Span);
                var compressedSpan = compressedBytes.AsSpan();

                var decompressSize = Decompressor.GetDecompressedSize(compressedSpan);
                var result = new byte[decompressSize];
                using var decompressor = PooledDecompressor.get();
                var bytesWritten = decompressor.Value.Unwrap(compressedSpan, result);
                return result.AsMemory(0, bytesWritten);
            }
        }
    }

    public sealed class PooledCompressor : AbsPoolable, IDisposable
    {
        private static NPool<PooledCompressor> _pool => NPool<PooledCompressor>.Shared;

        public static PooledCompressor get(int level)
        {
            var p = _pool.Rent().value;
            var compressor = p._compressor ??= new Compressor(level);
            if (compressor.Level != level) compressor.Level = level;
            return p;
        }

        private Compressor _compressor;

        public Compressor Value => _compressor;

        private PooledCompressor() { }

        protected override void onDestroy()
        {
            var compressor = _compressor;
            _compressor = null;
            compressor?.Dispose();
        }

        public void Dispose()
        {
            if (IsRented) _pool.Return(this);
        }
    }

    public sealed class PooledDecompressor : AbsPoolable, IDisposable
    {
        private static NPool<PooledDecompressor> _pool => NPool<PooledDecompressor>.Shared;

        public static PooledDecompressor get()
        {
            var p = _pool.Rent().value;
            p._decompressor ??= new Decompressor();
            return p;
        }

        private Decompressor _decompressor;

        public Decompressor Value => _decompressor;

        private PooledDecompressor() { }

        protected override void onDestroy()
        {
            var decompressor = _decompressor;
            _decompressor = null;
            decompressor?.Dispose();
        }

        public void Dispose()
        {
            if (IsRented) _pool.Return(this);
        }
    }
}