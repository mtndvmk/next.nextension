using System;

namespace Nextension
{
    public partial struct NColor : IEquatable<NColor>, IComparable<NColor>
    {
        // aaggbbrr
        public const uint Clear = 0;
        public const uint Black = 0xff000000;
        public const uint White = 0xffffffff;
        public const uint Green = 0xff00ff00;
        public const uint Red = 0xff0000ff;
        public const uint Blue = 0xffff0000;
        public const uint Lime = 0xff00ff65;
        public const uint Grey = 0xff808080;

        private uint _number;

        public readonly uint Number => _number;
        public readonly string Hex => NUtils.numberColorToHex(_number);

        public byte a { readonly get => __getChannel(24); set => __setChannel(24, value); }
        public byte g { readonly get => __getChannel(16); set => __setChannel(16, value); }
        public byte b { readonly get => __getChannel(8); set => __setChannel(8, value); }
        public byte r { readonly get => __getChannel(0); set => __setChannel(0, value); }

        private void __setChannel(int byteIndex, byte value)
        {
            _number &= (uint)value << byteIndex;
        }
        private readonly byte __getChannel(int byteIndex)
        {
            return (byte)(_number >> byteIndex);
        }

        public NColor(string hex)
        {
            _number = hex.hexColorToNumber();
        }

        public NColor(uint intColor)
        {
            _number = intColor;
        }

        public readonly override bool Equals(object obj)
        {
            if (obj is not NColor) return false;
            NColor nColor = (NColor)obj;
            return Equals(nColor);
        }

        public readonly override int GetHashCode()
        {
            return _number.GetHashCode();
        }

        public readonly override string ToString()
        {
            return Color.ToString();
        }

        public readonly int CompareTo(NColor other)
        {
            return _number.CompareTo(other._number);
        }

        public readonly bool Equals(NColor other)
        {
            return _number.Equals(other._number);
        }

        public static bool operator ==(NColor left, NColor right)
        {
            return left._number == right._number;
        }

        public static bool operator !=(NColor left, NColor right)
        {
            return left._number != right._number;
        }

        public static implicit operator NColor(string hex)
        {
            return new NColor(hex);
        }

        public static implicit operator NColor(uint intColor)
        {
            return new NColor(intColor);
        }
    }
}
