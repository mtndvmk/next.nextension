using UnityEngine.EventSystems;

namespace Nextension
{
    public static class EventSystemUtil
    {
        private static EventSystem _eventSystem;
        public static bool isOverUI()
        {
            if (_eventSystem.isNull())
                _eventSystem = EventSystem.current;
            if (_eventSystem.isNull())
                return false;
#if (UNITY_EDITOR || UNITY_WEBGL || UNITY_STANDALONE)
            return _eventSystem.IsPointerOverGameObject(-1);
#else
            return _eventSystem.IsPointerOverGameObject(0);
#endif
        }
    }
}
