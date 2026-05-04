namespace Nextension
{
    public abstract class AbsNNotify
    {
        public enum NotificationType
        {
            IMMEDIATE,
            LATE_UPATE,
            END_OF_FRAME
        }

        private NotificationType _NotificationType;
        protected bool _IsNeedNotify;

        protected void checkNeedNotify()
        {
            if (_IsNeedNotify)
            {
                _IsNeedNotify = false;
                onNotified();
            }
        }

        public void setNotificationType(NotificationType type)
        {
            if (_NotificationType != type)
            {
                _NotificationType = type;
                switch (_NotificationType)
                {
                    case NotificationType.LATE_UPATE:
                        NUpdater.onLateUpdateEvent.add(checkNeedNotify);
                        break;
                    case NotificationType.END_OF_FRAME:
                        NUpdater.onEndOfFrameEvent.add(checkNeedNotify);
                        break;
                }
            }
        }

        protected abstract void onNotified();
        public void notify()
        {
            if (_NotificationType == NotificationType.IMMEDIATE)
            {
                onNotified();
            }
            else
            {
                _IsNeedNotify = true;
            }
        }
    }
}
