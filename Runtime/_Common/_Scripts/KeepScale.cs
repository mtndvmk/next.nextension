using UnityEngine;

namespace Nextension
{
    public class KeepScale : MonoBehaviour
    {
        [HideInInspector, SerializeField] private Vector3? _OriginScale;
        [HideInInspector, SerializeField] private float _ParentMaxScale;
        [HideInInspector, SerializeField] private float _ParentOriginScale;
        [HideInInspector, SerializeField] private float _TimeToOrigin = 1f;

        private float _StartScaleTime = 0;

        private void Start()
        {
            if (!_OriginScale.HasValue)
            {
                _OriginScale = transform.lossyScale;
            }
            if (transform.lossyScale.x < _ParentOriginScale || _ParentOriginScale == 0 && _ParentMaxScale == 0)
            {
                transform.localScale = Vector3.zero;
            }
        }
        public KeepScale setup()
        {
            _OriginScale = transform.lossyScale;
            return this;
        }
        public KeepScale setParentOriginScale(float parentOriginScale)
        {
            _ParentOriginScale = parentOriginScale;
            return this;
        }
        public KeepScale setTimeToOrigin(float time)
        {
            _TimeToOrigin = time;
            return this;
        }

        private void LateUpdate()
        {
            if (transform.parent)
            {
                var scale = transform.parent.lossyScale.x;
                if (scale != 0)
                {
                    if (_StartScaleTime == 0)
                    {
                        _StartScaleTime = Time.time;
                    }
                    float parentOriginScale = _ParentOriginScale;
                    if (parentOriginScale == 0)
                    {
                        _ParentMaxScale = Mathf.Max(_ParentMaxScale, scale);
                        parentOriginScale = _ParentMaxScale;
                    }

                    var targetScale = _OriginScale.Value / parentOriginScale;
                    if (_TimeToOrigin > 0)
                    {
                        targetScale = Vector3.Lerp(transform.localScale, targetScale, (Time.time - _StartScaleTime) / _TimeToOrigin * Time.deltaTime);
                    }
                    transform.localScale = targetScale;
                }
            }
        }
    }
}