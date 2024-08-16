using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class NLoadingButton : AbsNButtonEffect
    {
        [SerializeField] private GameObject _loadingObject;
        public Func<Task> onClick;

        protected override void Awake()
        {
            base.Awake();
            _loadingObject.setActive(false);
        }

        public override async void onButtonClick()
        {
            if (onClick != null)
            {
                _button.setInteratableFromListener(this, false);
                _loadingObject.setActive(true);
                try
                {
                    await onClick();
                }
                finally
                {
                    _loadingObject.setActive(false);
                    _button.setInteratableFromListener(this, true);
                }
            }
        }
    }
}
