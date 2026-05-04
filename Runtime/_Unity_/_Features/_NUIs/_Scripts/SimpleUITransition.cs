using System;

namespace Nextension.UI
{
    public sealed class SimpleUITransition : UITransition
    {
        public void show(bool isImmediate = false)
        {
            innerShow(isImmediate);
        }

        public Action onBeforeShowEvent;
        public Action onBeforeHideEvent;

        protected override void onDerivedBeforeShow()
        {
            onBeforeShowEvent?.Invoke();
        }
        protected override void onDerivedBeforeHide()
        {
            onBeforeHideEvent?.Invoke();
        }
    }
}