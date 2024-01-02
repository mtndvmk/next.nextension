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
            get
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
            get
            {
                return max;
            }
            set
            {
                max = Mathf.Clamp01(value);
            }
        }
    }
    [Serializable]
    public struct NMinMax
    {
        public float min;
        public float max;
    }
}
