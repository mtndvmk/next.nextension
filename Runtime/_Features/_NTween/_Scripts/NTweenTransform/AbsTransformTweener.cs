using UnityEngine;

namespace Nextension.Tween
{
    internal abstract class AbsTransformTweener<T> : BasicTweener<T>
        where T : struct
    {
        public Transform target;
        public AbsTransformTweener(Transform target, T destination, TweenType tweenType) : base(default, destination, null, tweenType)
        {
            this.target = target;
        }
    }
}