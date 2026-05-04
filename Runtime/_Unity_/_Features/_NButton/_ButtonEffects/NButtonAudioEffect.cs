using UnityEngine;

namespace Nextension
{
    [DisallowMultipleComponent]
    public class NButtonAudioEffect : AbsNButtonEffect
    {
        [SerializeField] private AudioClip _onDownSfx;
        [SerializeField] private AudioClip _onUpSfx;
        [SerializeField] private AudioClip _onClickSfx;

        public override void onButtonDown()
        {
            if (!enabled) return;
            if (!_onDownSfx) return;
            AudioManager.Instance.playSfx(_onDownSfx);
        }
        public override void onButtonUp()
        {
            if (!enabled) return;
            if (!_onUpSfx) return;
            AudioManager.Instance.playSfx(_onUpSfx);
        }
        public override void onButtonClick()
        {
            if (!enabled) return;
            if (!_onClickSfx) return;
            AudioManager.Instance.playSfx(_onClickSfx);
        }
    }
}
