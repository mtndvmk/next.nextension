using Unity.Mathematics;
using UnityEngine;

namespace Nextension
{
    public abstract class AbsAutoTransform : MonoBehaviour
    {

        [SerializeField] protected bool _isLocalSpace = true;
        [SerializeField] protected bool _isStartOnEnable = true;

        protected float _startTime;

        internal AutoTransformHandle handler;
        internal abstract float3 AutoValue { get; }
        internal abstract AutoTransformType AutoTransformType { get; }

        public bool IsStartOnEnable
        {
            get => _isStartOnEnable;
            set => _isStartOnEnable = value;
        }
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
        public bool IsStarted => handler.isValid();
        public float PlayedTime
        {
            get
            {
                if (IsStarted)
                {
                    return getCurrentTime() - _startTime;
                }
                return 0;
            }
        }

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

        protected static float getCurrentTime()
        {
            return Time.time + 1;
        }

        protected void invokeAutoValueChanged()
        {
            if (handler.isValid())
            {
                AutoTransformSystem.updateAutoValue(this);
            }
        }

        public void start()
        {
            if (handler.isValid())
            {
                return;
            }
            handler = AutoTransformSystem.start(this);
            _startTime = getCurrentTime();
        }
        public void stop()
        {
            if (handler.isValid())
            {
                AutoTransformSystem.stop(handler);
                handler = default;
            }
        }
    }
}
