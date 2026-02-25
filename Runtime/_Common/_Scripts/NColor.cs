using System;
using UnityEngine;

namespace Nextension
{
    public struct NColor : IEquatable<NColor>, IComparable<NColor>
    {
        // aaggbbrr
        public const uint Clear = 0;
        public const uint Black = 0xff000000;
        public const uint White = 0xffffffff;
        public const uint Green = 0xff00ff00;
        public const uint Red = 0xff0000ff;
        public const uint Blue = 0xffff0000;
        public const uint Slime = 0xff00ff65;
        public const uint Grey = 0xff808080;

        private uint _number;

        public readonly uint Number => _number;
        public readonly string Hex => NUtils.numberColorToHex(_number);
        public readonly Color Color => NUtils.asColor(_number);

        public byte a { readonly get => getChannel(24); set => setChannel(24, value); }
        public byte g { readonly get => getChannel(16); set => setChannel(16, value); }
        public byte b { readonly get => getChannel(8); set => setChannel(8, value); }
        public byte r { readonly get => getChannel(0); set => setChannel(0, value); }


        private void setChannel(int byteIndex, byte value)
        {
            _number &= (uint)value << byteIndex;
        }
        private readonly byte getChannel(int byteIndex)
        {
            return (byte)(_number >> byteIndex);
        }

        public NColor(string hex)
        {
            _number = hex.hexColorToNumber();
        }
        public NColor(Color color)
        {
            _number = color.asNumber();
        }
        public NColor(uint intColor)
        {
            _number = intColor;
        }

        public readonly override bool Equals(object obj)
        {
            if (obj is not NColor nColor) return false;
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

        public static implicit operator NColor(Color color)
        {
            return new NColor(color);
        }

        public static implicit operator NColor(uint intColor)
        {
            return new NColor(intColor);
        }

        public static implicit operator Color(NColor nColor)
        {
            return nColor.Color;
        }
    }
}
