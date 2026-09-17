using System;

namespace Nextension.UI
{
    public sealed class SimpleUITransition : UITransition
    {
        public void show(bool isImmediate = false)
        {
            __innerShow(isImmediate);
        }

        public Action onBeforeShowEvent;
        public Action onBeforeHideEvent;

        protected override void __onDerivedBeforeShow()
        {
            onBeforeShowEvent?.Invoke();
        }
        protected override void __onDerivedBeforeHide()
        {
            onBeforeHideEvent?.Invoke();
        }
    }
}