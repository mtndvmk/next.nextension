using System;
using UnityEngine;

namespace Nextension
{
    public class ExpirableValue<T>
    {
        private NWaitable _expirationWaitable;

        public T Value { get; set; }
        public float ExpirationTime { get; private set; }
        public bool IsExpired { get; private set; }
        public readonly NCallback onExpired = new();


        public ExpirableValue(T value, float expirationTimeInSecond, Action onExpiredCallback)
        {
            Value = value;
            onExpired.add(onExpiredCallback);
            renew(expirationTimeInSecond);
        }
        public void renew(float expirationTimeInSecond = 10)
        {
            if (IsExpired)
            {
                Debug.LogWarning("ExpirableValue is expired");
            }
            else
            {
                ExpirationTime = expirationTimeInSecond;
                if (NStartRunner.IsPlaying)
                {
                    checkExpiration();
                }
            }
        }

        private void checkExpiration()
        {
            _expirationWaitable?.cancel();
            _expirationWaitable = new NWaitSecond(ExpirationTime).startWaitable();
            _expirationWaitable.addCompletedEvent(() =>
            {
                IsExpired = true;
                _expirationWaitable = null;
                onExpired.tryInvoke();
            });
        }

        public static implicit operator T(ExpirableValue<T> eValue) => eValue.Value;
    }
}
