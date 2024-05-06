using UnityEngine;

namespace Nextension
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class NSequenceSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] _spriteFrames;
        [SerializeField] private uint _fps;
        [SerializeField] private bool _isLoop;
        [SerializeField] private bool _autoPlayOnEnable;
        [SerializeField] private bool _autoDisableOnEndOfFrames;
        [SerializeField] private bool _autoClearSpriteOnEndOfFrames;

        private int _currentFrameIndex = 0;
        private int _requestFrameIndex = -1;
        private float _startPlayTime;

        private SpriteRenderer _spriteRenderer;

        public float Duration => _fps == 0 ? 0 : (float)_spriteFrames.Length / _fps;
        public int CurrentFrameIndex => _currentFrameIndex;
        public int FrameCount => _spriteFrames.Length;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        private void OnEnable()
        {
            if (_autoPlayOnEnable)
            {
                play();
            }
        }
        private void OnDisable()
        {
            stop();
        }
        private void Update()
        {
            if (_requestFrameIndex == _spriteFrames.Length - 1)
            {
                if (_autoClearSpriteOnEndOfFrames)
                {
                    _spriteRenderer.sprite = null;
                }
                if (_autoDisableOnEndOfFrames)
                {
                    enabled = false;
                }
                else if (!_isLoop)
                {
                    return;
                }
            }
            if (_spriteFrames.Length == 0 || _requestFrameIndex < 0)
            {
                return;
            }

            var deltaTime = Time.time - _startPlayTime;
            _requestFrameIndex = (int)(deltaTime * _fps);

            if (_requestFrameIndex >= _spriteFrames.Length)
            {
                if (_isLoop)
                {
                    _requestFrameIndex %= _spriteFrames.Length;
                }
                else
                {
                    _requestFrameIndex = _spriteFrames.Length - 1;
                }
            }

            if (_requestFrameIndex != _currentFrameIndex)
            {
                _currentFrameIndex = _requestFrameIndex;
                _spriteRenderer.sprite = _spriteFrames[_currentFrameIndex];
            }
        }

        public void play()
        {
            _requestFrameIndex = 0;
            _startPlayTime = Time.time;
        }
        public void stop()
        {
            _requestFrameIndex = -1;
        }
    }
}
