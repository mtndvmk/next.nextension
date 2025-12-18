using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
        [NIndent, SerializeField, NShowIf(nameof(_targetType), TargetType.SpriteRenderer)] private SpriteRenderer _spriteRenderer;
        [NIndent, SerializeField, NShowIf(nameof(_targetType), TargetType.Image)] private Image _image;
        [SerializeField] private List<Sprite> _spriteFrames;
        [SerializeField] private bool _playOnEnable;
        [SerializeField] private uint _fps = 12;
        [SerializeField] private bool _isLoop;
        [NIndent, NShowIf(nameof(_isLoop)), SerializeField] private float _delayInNewLoop;
        [NIndent, NShowIf(nameof(_isLoop)), SerializeField] private bool _isBackAndForthLoop;

        [SerializeField, NSlider(-1, nameof(MaxIndex))] private int _requestFrameIndex = -1;
        [NGroup("Event")] public UnityEvent onEndOfAnimation;

        private bool _isPlaying;
        private int _currentFrameIndex = 0;
        private float _nextFrameTime;
        private bool _loopbackIsBacking;

        public float Duration => _fps == 0 ? 0 : (float)FrameCount / _fps;
        public int CurrentFrameIndex => _currentFrameIndex;
        public int FrameCount => _spriteFrames == null ? 0 : _spriteFrames.Count;
        public bool IsPlaying => _isPlaying;
        public int MaxIndex => FrameCount - 1;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public Image Image => _image;

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _image = GetComponent<Image>();
            _targetType = _image.isNull() ? TargetType.SpriteRenderer : TargetType.Image;
        }

        private void OnValidate()
        {
            _currentFrameIndex = _requestFrameIndex;
            setSprite(_currentFrameIndex);
        }

        private void OnEnable()
        {
            if (_playOnEnable)
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
            if (!_isPlaying || _requestFrameIndex < 0 || _fps == 0 || FrameCount == 0)
            {
                return;
            }

            var lastestFrameIndex = _spriteFrames.Count - 1;
            
            if (Time.time >= _nextFrameTime)
            {
                if (_isLoop)
                {
                    if (_isBackAndForthLoop)
                    {
                        updateNextFrameOnBackAndForthLoop(lastestFrameIndex);
                    }
                    else
                    {
                        updateNextFrameOnForthLoop(lastestFrameIndex);
                    }
                }
                else
                {

                    if (_requestFrameIndex < lastestFrameIndex)
                    {
                        _requestFrameIndex++;
                        _nextFrameTime = Time.time + 1f / _fps;
                    }
                }
            }

            if (_requestFrameIndex != _currentFrameIndex)
            {
                _currentFrameIndex = _requestFrameIndex;
                setSprite(_spriteFrames[_currentFrameIndex]);
                
                if (_requestFrameIndex >= lastestFrameIndex && !_isLoop)
                {
                    onEndOfAnimation?.Invoke();
                    stop();
                }
            }
        }

        private void updateNextFrameOnBackAndForthLoop(int lastestFrameIndex)
        {
            if (_requestFrameIndex <= 0)
            {
                _loopbackIsBacking = false;
                _requestFrameIndex = 1;
            }
            else if (_requestFrameIndex >= lastestFrameIndex)
            {
                _loopbackIsBacking = true;
                _requestFrameIndex = lastestFrameIndex - 1;
            }
            else
            {
                if (_loopbackIsBacking)
                {
                    _requestFrameIndex--;
                }
                else
                {
                    _requestFrameIndex++;
                }
            }
            if (_requestFrameIndex == 0)
            {
                _nextFrameTime = Time.time + 1f / _fps + _delayInNewLoop;
            }
            else
            {
                _nextFrameTime = Time.time + 1f / _fps;
            }
        }
        private void updateNextFrameOnForthLoop(int lastestFrameIndex)
        {
            if (_requestFrameIndex >= lastestFrameIndex)
            {
                _requestFrameIndex = 0;
                _nextFrameTime = Time.time + 1f / _fps + _delayInNewLoop;
            }
            else
            {
                _requestFrameIndex++;
                _nextFrameTime = Time.time + 1f / _fps;
            }
        }

        public void setSprite(int frameIndex)
        {
            if (_spriteFrames == null || _spriteFrames.Count == 0)
            {
                return;
            }
            if (frameIndex < 0 || frameIndex >= _spriteFrames.Count)
            {
                _currentFrameIndex = 0;
            }
            else
            {
                _currentFrameIndex = frameIndex;
            }
            setSprite(_spriteFrames[_currentFrameIndex]);
        }
        public void setSprite(Sprite sprite)
        {
            switch (_targetType)
            {
                case TargetType.SpriteRenderer:
                    {
                        if (_spriteRenderer) _spriteRenderer.sprite = sprite;
                        break;
                    }
                case TargetType.Image:
                    {
                        if (_image) _image.sprite = sprite;
                        break;
                    }
                default:
                    throw new System.NotImplementedException();
            }
        }

        public void play(int startFrameIndex = 0)
        {
            if (_fps == 0)
            {
                Debug.LogWarning("Can't play sequence sprite animation with fps = 0");
                return;
            }
            _requestFrameIndex = startFrameIndex;
            _nextFrameTime = Time.time + 1 / _fps;
            _isPlaying = true;
            setSprite(startFrameIndex);
        }

        public void play(ICollection<Sprite> sprites, uint fps, int startFrameIndex = 0)
        {
            if (sprites.Count == 0)
            {
                Debug.LogWarning("Can't play sequence sprite animation with empty sprites");
                return;
            }
            if (fps == 0)
            {
                Debug.LogWarning("Can't play sequence sprite animation with fps = 0");
                return;
            }
            _spriteFrames.Clear();
            _spriteFrames.AddRange(sprites);
            _fps = fps;
            play(startFrameIndex);
        }

        public void stop()
        {
            if (IsPlaying)
            {
                _isPlaying = false;
                _requestFrameIndex = -1;
            }
        }

        public void stopAndClearFrames()
        {
            if (IsPlaying)
            {
                stop();
                _spriteFrames.Clear();
                _currentFrameIndex = 0;
            }
        }
    }
}
