using System;
using System.Security.Cryptography;
using System.Text;

namespace Nextension
{
    public static class NHmacHasher
    {
        public static byte[] compute(ReadOnlySpan<byte> message, byte[] secretKey)
        {
            var hashBytes = new byte[32];
            compute(message, hashBytes, secretKey);
            return hashBytes;
        }

        public static void compute(ReadOnlySpan<byte> message, Span<byte> hashSpan, byte[] secretKey)
        {
            using var hmac = new HMACSHA256(secretKey);
            hmac.TryComputeHash(message, hashSpan, out int bytesWritten);
            if (bytesWritten != 32)
            {
                throw new Exception("HMACSHA256 computation failed");
            }
        }

        public static bool verify(ReadOnlySpan<byte> contentSpan, byte[] secretKey, ReadOnlySpan<byte> receivedHash)
        {
            using var hmac = new HMACSHA256(secretKey);
            Span<byte> expectedHash = stackalloc byte[32];
            hmac.TryComputeHash(contentSpan, expectedHash, out int bytesWritten);
            return expectedHash.SequenceEqual(expectedHash);
        }

        public static byte[] compute(string message, byte[] secretKey)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            return compute(messageBytes, secretKey);
        }

        public static bool verify(string message, byte[] secretKey, ReadOnlySpan<byte> receivedHash)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            return verify(messageBytes, secretKey, receivedHash);
        }
    }
}
