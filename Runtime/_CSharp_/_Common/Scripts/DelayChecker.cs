namespace Nextension
{
    public class DelayChecker
    {
        private long _nextTimeMs;
        private int _delayTime;

        public DelayChecker(int delayTimeInMs)
        {
            _delayTime = delayTimeInMs;
        }
        public DelayChecker(int delayTimeInMs, bool delayFromInit)
        {
            _delayTime = delayTimeInMs;
            if (delayFromInit)
            {
                _nextTimeMs = NUpdater.LatestUpdatedTimeMs + _delayTime;
            }
        }

        public bool isDelay()
        {
            var current = NUpdater.LatestUpdatedTimeMs;
            if (current >= _nextTimeMs)
            {
                _nextTimeMs = current + _delayTime;
                return false;
            }
            return true;
        }
    }
}
