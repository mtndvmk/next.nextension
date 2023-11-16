using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    public abstract class AbsAutoTransform : MonoBehaviour
    {
        internal int autoIndex = -1;

        [SerializeField] private bool _isLocalSpace = true;
        [SerializeField] private bool _isStartOnEnable = true;

        internal abstract float3 AutoValue { get; }
        internal abstract AutoTransformType AutoTransformType { get; }
        
        public bool IsWorldSpace
        {
            get => _isLocalSpace;
            set
            {
                if (_isLocalSpace != value)
                {
                    _isLocalSpace = value;
                    invokeAutoValueChanged();
                }
            }
        }
        public bool IsStarted => autoIndex != -1;

        protected virtual void OnEnable()
        {
            if (_isStartOnEnable)
            {
                start();
            }
        }
        protected virtual void OnDisable()
        {
            stop();
        }
        protected virtual void OnValidate()
        {
            invokeAutoValueChanged();
        }
        protected void invokeAutoValueChanged()
        {
            if (autoIndex != -1)
            {
                AutoTransformSystem.updateAutoValue(this);
            }
        }

        public void start()
        {
            if (autoIndex == -1)
            {
                autoIndex = AutoTransformSystem.add(this);
            }
        }
        public void stop()
        {
            if (autoIndex >= 0)
            {
                AutoTransformSystem.remove(autoIndex);
                autoIndex = -1;
            }
        }
    }
}
