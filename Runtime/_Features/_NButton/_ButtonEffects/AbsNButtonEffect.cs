using UnityEngine;

namespace Nextension
{
    public abstract class AbsNButtonEffect : MonoBehaviour, INButtonListener
    {
        protected INButton _button;
        public INButton Button
        {
            get
            {
                return _button ??= GetComponentInParent<INButton>(true);
            }
        }

        protected virtual void Awake()
        {
            Button.addNButtonListener(this);
        }
        protected virtual void OnDestroy()
        {
            Button.removeNButtonListener(this);
        }

        public virtual void onButtonDown() { }
        public virtual void onButtonUp() { }
        public virtual void onButtonClick() { }
        public virtual void onButtonEnter() { }
        public virtual void onButtonExit() { }
        public virtual void onInteractableChanged(bool isInteractable) { }
    }
}
