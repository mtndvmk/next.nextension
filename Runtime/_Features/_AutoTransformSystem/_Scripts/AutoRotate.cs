using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class AutoRotate : AbsAutoTransform
    {
        [SerializeField] private float3 _degreePerSecond;
        internal override float3 AutoValue => _degreePerSecond;
        internal override AutoTransformType AutoTransformType => AutoTransformType.AutoRotate;
        
        public void setSpeed(float3 degreePerSecond)
        {
            if (_degreePerSecond.Equals(degreePerSecond))
            {
                _degreePerSecond = degreePerSecond;
                invokeAutoValueChanged();
            }
        }
    }
}
