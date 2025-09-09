using UnityEngine;

namespace Nextension
{
    [ExecuteAlways]
    public class LossyScale : MonoBehaviour
    {
        [SerializeField, NConstrainable] private Vector3 _scale;
        [SerializeField] private Transform _relativeTransform;

        public Transform RelativeTransform
        {
            get => _relativeTransform;
            set => _relativeTransform = value;
        }

        public Vector3 Scale
        {
            get => _scale;
            set => _scale = value;
        }

        private void Reset()
        {
            _scale = transform.lossyScale;
        }

        [ContextMenu("Set scale to Vector3.one")]
        public void setToOne()
        {
            _scale = Vector3.one;
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (_scale == Vector3.zero)
            {
                Debug.LogError("<b>Scale</b> value must be different from (0, 0, 0)", this);
                return;
            }
#endif
            var parent = transform.parent;
            if (parent != null)
            {
                var parrentLossyScale = parent.lossyScale;
                if (!parrentLossyScale.hasZeroAxis())
                {
                    if (_relativeTransform == null)
                    {
                        transform.localScale = _scale.div(parrentLossyScale);
                    }
                    else
                    {
                        transform.localScale = _scale.div(parrentLossyScale).mul(_relativeTransform.lossyScale);
                    }
                }
            }
            else
            {
                if (_relativeTransform == null)
                {
                    transform.localScale = _scale;
                }
                else
                {
                    transform.localScale = _scale.mul(_relativeTransform.lossyScale);
                }
            }
        }
    }
}