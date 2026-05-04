using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    public class AutoMove : AbsAutoTransform
    {
        [SerializeField] private float3 _speed;
        internal override float3 AutoValue => _speed;
        internal override AutoTransformType AutoTransformType => AutoTransformType.AutoMove;

        public void setSpeed(float3 speed)
        {
            if (!_speed.Equals(speed))
            {
                _speed = speed;
                invokeAutoValueChanged();
            }
        }
    }
}
