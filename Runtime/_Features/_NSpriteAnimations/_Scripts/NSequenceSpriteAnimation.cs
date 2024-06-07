using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    public class NSequenceSpriteAnimation : MonoBehaviour
    {
        public enum TargetType
        {
            SpriteRenderer,
            Image
        }
        [SerializeField] private TargetType _targetType;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Image _image;
        [SerializeField] private Sprite[] _spriteFrames;
        [SerializeField] private uint _fps;
        [SerializeField] private bool _isLoop;
        [SerializeField] private bool _autoPlayOnEnable;
        [SerializeField] private bool _autoDisableOnEndOfFrames;
        [SerializeField] private bool _autoClearSpriteOnEndOfFrames;

        private int _currentFrameIndex = 0;
        private int _requestFrameIndex = -1;
        private float _startPlayTime;

        
        public float Duration => _fps == 0 ? 0 : (float)_spriteFrames.Length / _fps;
        public int CurrentFrameIndex => _currentFrameIndex;
        public int FrameCount => _spriteFrames.Length;

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _image = GetComponent<Image>();
            _targetType = _image.isNull() ? TargetType.SpriteRenderer : TargetType.Image;
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
                    setSprite(null);
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
                setSprite(_spriteFrames[_currentFrameIndex]);
            }
        }

        private void setSprite(Sprite sprite)
        {
            switch (_targetType)
            {
                case TargetType.SpriteRenderer:
                    {
                        _spriteRenderer.sprite = sprite;
                        break;
                    }
                case TargetType.Image:
                    {
                        _image.sprite = sprite;
                        break;
                    }
                default:
                    throw new System.NotImplementedException();
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
