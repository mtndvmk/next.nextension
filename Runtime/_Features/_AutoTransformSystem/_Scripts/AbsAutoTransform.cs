using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    public abstract class AbsAutoTransform : MonoBehaviour
    {
        internal AutoTransformHandle handler;

        [SerializeField] private bool _isLocalSpace = true;
        [SerializeField] private bool _isStartOnEnable = true;

        internal abstract float3 AutoValue { get; }
        internal abstract AutoTransformType AutoTransformType { get; }
        
        public bool IsLocalSpace
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
        public bool IsStarted => handler != null;

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
            if (handler != null)
            {
                AutoTransformSystem.updateAutoValue(this);
            }
        }

        public void start()
        {
            handler ??= AutoTransformSystem.start(this);
        }
        public void stop()
        {
            if (handler != null)
            {
                AutoTransformSystem.stop(handler);
                handler = default;
            }
        }
    }
}
