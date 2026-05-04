using UnityEngine;

namespace Nextension.Tween
{
    internal interface ITransformTweener
    {
        public Transform Target { get; }
    }
    internal interface ITransform2Tweener
    {
        public Transform Target { get; }
        public Transform Destination { get; }
    }
}