using System;
using System.Runtime.CompilerServices;

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
        public readonly NCallback onDirtyCleanedEvent = new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void setDirty()
        {
            notify();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void cleanImmediate()
        {
            checkNeedNotify();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void cleanWithoutNotify()
        {
            _IsNeedNotify = false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void removeAllListeners()
        {
            onDirtyCleanedEvent.clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void onNotified()
        {
            onDirtyCleanedEvent.tryInvoke();
        }
    }
}
