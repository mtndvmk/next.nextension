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

        protected override void onBeforeShow()
        {
            onBeforeShowEvent?.Invoke();
        }
        protected override void onBeforeHide()
        {
            onBeforeHideEvent?.Invoke();
        }
    }
}