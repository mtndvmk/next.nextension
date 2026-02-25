using System;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public struct NMinMax01
    {
        [SerializeField, Range(0, 1)] private float min;
        [SerializeField, Range(0, 1)] private float max;

        public float Min
        {
            readonly get
            {
                return min;
            }
            set
            {
                min = Mathf.Clamp01(value);
            }
        }
        public float Max
        {
            readonly get
            {
                return max;
            }
            set
            {
                max = Mathf.Clamp01(value);
            }
        }

        public readonly float rand()
        {
            return rand(0);
        }
        public readonly float rand(uint seed)
        {
            return NUtils.getRandom(seed).NextFloat(min, max);
        }
    }
    [Serializable]
    public struct NMinMax
    {
        public float min;
        public float max;

        public readonly float rand()
        {
            return rand(0);
        }
        public readonly float rand(uint seed)
        {
            return NUtils.getRandom(seed).NextFloat(min, max);
        }
    }
}
