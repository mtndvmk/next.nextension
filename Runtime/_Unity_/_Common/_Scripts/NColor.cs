using System;
using UnityEngine;

namespace Nextension
{
    public partial struct NColor : IEquatable<NColor>, IComparable<NColor>
    {
        public readonly Color Color => NUtils.asColor(_number);
        public NColor(Color color)
        {
            _number = color.asNumber();
        }
        public static implicit operator NColor(Color color)
        {
            return new NColor(color);
        }
        public static implicit operator Color(NColor nColor)
        {
            return nColor.Color;
        }
    }
}
