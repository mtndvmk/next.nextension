using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    [Serializable]
    public sealed class NDirty : AbsNNotify
    {
        public NDirty(NotificationType type = NotificationType.IMMEDIATE)
        {
            setNotificationType(type);
            setDirty();
        }
        public bool IsDirty => _IsNeedNotify;
        public readonly NCallback onDirtyCleanedEvent = new NCallback();
        public void setDirty()
        {
            notify();
        }
        public void cleanImmediate()
        {
            checkNeedNotify();
        }
        public void cleanWithoutNotify()
        {
            _IsNeedNotify = false;
        }
        public void removeAllListeners()
        {
            onDirtyCleanedEvent.clear();
        }
        protected override void onNotified()
        {
            onDirtyCleanedEvent.tryInvoke(Debug.LogException);
        }
    }
}
