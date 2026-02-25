using UnityEngine.EventSystems;

namespace Nextension
{
    public static class PointerManager
    {
        private static EventSystem _eventSystem;
        public static bool isOverUI()
        {
            if (_eventSystem.isNull())
                _eventSystem = EventSystem.current;
            if (_eventSystem.isNull())
                return false;
            return _eventSystem.IsPointerOverGameObject();
        }
    }
}
