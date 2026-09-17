using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nextension
{
    public static class NCompressExtensions
    {
        public static NBytesReader toNBytesReader(this string compressedString)
        {
            var decompressed = NCompress.Zstd.decompressToMemory(compressedString);
            return new NBytesReader(decompressed.Span);
        }
        public static string toCompressedString(this NBytesWriter writer)
        {
            var compressed = NCompress.Zstd.compressToStr(writer.Buffer);
            return compressed.consume();
        }

        public static string toCompressedString<T>(this Dictionary<T, string> data) where T : struct, Enum
        {
            using var writer = new NBytesWriter();
            writer.writeQUIC2bit(data.Count);
            foreach (var kvp in data)
            {
                var k = kvp.Key;
                writer.writeQUIC2bit(Unsafe.As<T, uint>(ref k));
                writer.writeUTF8(kvp.Value);
            }
            return writer.toCompressedString();
        }

        public static void fromCompressedString<T>(this Dictionary<T, string> data, string compressedString) where T : struct, Enum
        {
            data.Clear();
            if (string.IsNullOrEmpty(compressedString)) return;
            var reader = compressedString.toNBytesReader();
            var count = reader.readQUIC2bitCount();
            for (int i = 0; i < count; i++)
            {
                var uNum = reader.readQUIC2bitUInt32();
                var k = Unsafe.As<uint, T>(ref uNum);
                var v = reader.readUTF8();
                data[k] = v;
            }
        }
    }
}