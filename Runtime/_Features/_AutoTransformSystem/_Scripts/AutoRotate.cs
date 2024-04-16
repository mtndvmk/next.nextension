using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class AutoRotate : AbsAutoTransform
    {
        [SerializeField, FormerlySerializedAs("_degreePerSecond")] private float3 _degreesPerSecond;
        internal override float3 AutoValue => _degreesPerSecond;
        internal override AutoTransformType AutoTransformType => AutoTransformType.AutoRotate;
        
        public void setSpeed(float3 degreesPerSecond)
        {
            if (!_degreesPerSecond.Equals(degreesPerSecond))
            {
                _degreesPerSecond = degreesPerSecond;
                invokeAutoValueChanged();
            }
        }
    }
}
