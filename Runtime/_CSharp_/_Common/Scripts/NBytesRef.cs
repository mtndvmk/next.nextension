using System;
using System.Collections.Generic;

namespace Nextension
{
    public readonly ref struct NBytesRef
    {
        public NBytesRef(ReadOnlySpan<byte> content)
        {
            this.content = content;
        }
        public NBytesRef(string content)
        {
            this.content = content.AsSpan().asSpan<char, byte>();
        }

        public readonly ReadOnlySpan<byte> content;

        public int ContentLength => content.Length;
        public int SizeInBytes => content.Length + NInteger.estNumPartBytesLength(content.Length) + 1;



        public void writeTo(NBytesWriter writer)
        {
            new NInteger(content.Length).writeTo(writer);
            writer.write(content);
        }
        public void writeTo_UTF8(NBytesWriter writer)
        {
            var charCount = content.Length / 2;
            new NInteger(charCount).writeTo(writer);
            writer.writeUTF8(content.asSpan<byte, char>());
        }

        public byte[] toBytes()
        {
            var nLength = new NInteger(content.Length);
            var numPartBytesLength = NInteger.estNumPartBytesLength(content.Length);
            byte[] result = new byte[numPartBytesLength + 1 + content.Length];
            int startIndex = 0;
            nLength.writeTo(result, ref startIndex);
            content.CopyTo(result.AsSpan(startIndex, content.Length));
            return result;
        }
        public NBytes toNBytes()
        {
            return new NBytes(content.ToArray());
        }
        public string toUTF8String()
        {
            return NConverter.getUTF8String(content);
        }

        public NBytesReader asReader()
        {
            return new NBytesReader(this);
        }
    }
}