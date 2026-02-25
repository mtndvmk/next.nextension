using System;
using UnityEngine;

namespace Nextension
{
    public class ExpirableValue<T>
    {
        private NTask _expirationTask;

        public T Value { get; set; }
        public float ExpirationDurationInSecond { get; private set; }
        public bool IsExpired { get; private set; }
        public readonly NCallback onExpired = new();


        public ExpirableValue(T value, float expirationDurationInSecond, Action onExpiredCallback)
        {
            Value = value;
            onExpired.add(onExpiredCallback);
            renew(expirationDurationInSecond);
        }
        public void renew(float expirationDurationInSecond = 10)
        {
            if (IsExpired)
            {
                Debug.LogWarning("ExpirableValue is expired");
            }
            else
            {
                ExpirationDurationInSecond = expirationDurationInSecond;
#if UNITY_EDITOR
                if (NStartRunner.IsPlaying)
#endif
                {
                    checkExpiration();
                }
            }
        }

        private void checkExpiration()
        {
            _expirationTask.tryCancel();
            _expirationTask = waitExpiration();
        }

        private async NTask waitExpiration()
        {
            await new NWaitSecond(ExpirationDurationInSecond);
            _expirationTask.forget();
            _expirationTask = default;
            IsExpired = true;
            onExpired.tryInvoke();
        }

        public static implicit operator T(ExpirableValue<T> eValue) => eValue.Value;
    }
}
