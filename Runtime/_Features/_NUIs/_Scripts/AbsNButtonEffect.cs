using UnityEngine;

namespace Nextension.UI
{
    [RequireComponent(typeof(NButton)), DisallowMultipleComponent]
    public abstract class AbsNButtonEffect : MonoBehaviour
    {
        [field: SerializeField] public float AnimationTime { get; set; } = 0.1f;

        public virtual void onButtonDown() { }
        public virtual void onButtonUp() { }
        public virtual void onButtonClick() { }
        public virtual void onButtonEnter() { }
        public virtual void onButtonExit() { }
    }
}
